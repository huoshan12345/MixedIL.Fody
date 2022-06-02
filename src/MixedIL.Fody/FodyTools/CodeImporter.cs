namespace FodyTools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    /// <summary>
    /// A class to import code from one module to another; like e.g. ILMerge, but only imports the specified classes and their local references.
    /// </summary>
    /// <remarks>
    /// The main task of this is to copy code fragments into another module so they can be used by the weaver.
    /// It copies the types specified in the Import method, and automatically copies all required dependencies.
    /// </remarks>
    internal sealed class CodeImporter
    {
        private static readonly ConstructorInfo _instructionConstructor = typeof(Instruction).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(OpCode), typeof(object) }, null)!;
        private readonly Dictionary<string, ModuleDefinition> _sourceModuleDefinitions = new();
        private readonly Dictionary<TypeDefinition, TypeDefinition> _targetTypesBySource = new();
        private readonly HashSet<TypeDefinition> _targetTypes = new();
        private readonly Dictionary<MethodDefinition, MethodDefinition> _targetMethods = new();
        private readonly IList<Action> _deferredActions = new List<Action>();
        private readonly Dictionary<string, TypeDefinition> _targetTypesByFullName;

        private enum Priority
        {
            Instructions,
            Operands
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeImporter"/> class.
        /// </summary>
        /// <param name="targetModule">The target module into which the specified code will be imported.</param>
        public CodeImporter(ModuleDefinition targetModule)
        {
            TargetModule = targetModule;
            AssemblyResolver = targetModule.AssemblyResolver;
            _targetTypesByFullName = targetModule.GetTypes().ToDictionary(item => item.FullName);
        }

        public ModuleDefinition TargetModule { get; }
        public IModuleResolver? ModuleResolver { get; set; }
        public IAssemblyResolver? AssemblyResolver { get; set; }
        public Func<string, string> NamespaceDecorator { get; set; } = value => value;
        public bool HideImportedTypes { get; set; } = true;
        public bool CompactMode { get; set; } = true;
        public readonly Func<MethodDefinition, bool> DeferMethodImportCallback = DefaultCanDeferMethodImport;

        private static bool DefaultCanDeferMethodImport(MethodDefinition method)
        {
            if (method.IsConstructor)
                return false;

            if (method.IsStatic)
                return true;

            var declaringType = method.DeclaringType;

            return !declaringType.IsInterface && !declaringType.IsValueType && !method.IsAbstract && !method.IsVirtual && !method.IsPInvokeImpl;
        }

        private bool DeferMethodImport(MethodDefinition method)
        {
            return CompactMode && DeferMethodImportCallback(method);
        }

        public void RegisterSourceModule(ModuleDefinition sourceModule)
        {
            var assemblyName = sourceModule.Assembly.FullName;

            if (_sourceModuleDefinitions.ContainsKey(assemblyName))
                return;

            if (!sourceModule.HasSymbols)
            {
                try
                {
                    sourceModule.ReadSymbols();
                }
                catch
                {
                    // module has no symbols, just go without...
                }
            }

            _sourceModuleDefinitions[assemblyName] = sourceModule;
        }

        [return: NotNullIfNotNull("sourceType")]
        private TypeDefinition? ImportTypeDefinition(TypeDefinition? sourceType)
        {
            if (sourceType == null)
                return null;

            if (_targetTypesBySource.TryGetValue(sourceType, out var importedTargetType))
                return importedTargetType;

            if (_targetTypes.Contains(sourceType))
                return sourceType;

            if (IsLocalOrExternalReference(sourceType))
                return sourceType;

            _targetTypesByFullName.TryGetValue(sourceType.FullName, out var existingType);

            var isEmbeddedType = sourceType.IsEmbeddedType();

            if (existingType != null && isEmbeddedType)
            {
                return MergeIntoExistingType(existingType, sourceType);
            }

            RegisterSourceModule(sourceType.Module);

            string DecoratedNamespace(TypeDefinition type)
            {
                return type.IsNested || isEmbeddedType ? type.Namespace : NamespaceDecorator(type.Namespace);
            }

            string DecoratedTypeName(TypeDefinition type)
            {
                return string.IsNullOrEmpty(type.Namespace) && !type.IsNested
                    ? $"{type.Module.Name}!{type.Name}"
                    : type.Name;
            }

            var targetType = new TypeDefinition(DecoratedNamespace(sourceType), DecoratedTypeName(sourceType), sourceType.Attributes)
            {
                ClassSize = sourceType.ClassSize,
                PackingSize = sourceType.PackingSize
            };

            _targetTypesBySource.Add(sourceType, targetType);
            _targetTypes.Add(targetType);

            targetType.DeclaringType = ImportTypeDefinition(sourceType.DeclaringType);

            try
            {
                _targetTypesByFullName.Add(targetType.FullName, targetType);
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException($"IL Import failed: {targetType.FullName} already exists in target module.");
            }

            CopyGenericParameters(sourceType, targetType);

            targetType.BaseType = InternalImportType(sourceType.BaseType, null);

            foreach (var sourceTypeInterface in sourceType.Interfaces)
            {
                var targetInterface = new InterfaceImplementation(InternalImportType(sourceTypeInterface.InterfaceType, null));
                CopyAttributes(sourceTypeInterface, targetInterface);
                targetType.Interfaces.Add(targetInterface);
            }

            if (targetType.IsNested)
            {
                targetType.DeclaringType.NestedTypes.Add(targetType);
            }
            else
            {
                if (HideImportedTypes)
                {
                    targetType.IsPublic = false;
                }

                TargetModule.Types.Add(targetType);
            }

            CopyFields(sourceType, targetType);
            CopyMethods(sourceType, targetType);
            CopyProperties(sourceType, targetType);
            CopyEvents(sourceType, targetType);
            CopyAttributes(sourceType, targetType);

            return targetType;
        }

        private TypeDefinition MergeIntoExistingType(TypeDefinition existing, TypeDefinition source)
        {
            _targetTypesBySource[source] = existing;

            foreach (var method in source.Methods)
            {
                var existingMethod = existing.Methods.FirstOrDefault(m => m.HasSameNameAndSignature(method));
                if (existingMethod != null)
                {
                    _targetMethods[method] = existingMethod;
                }
                else
                {
                    if (DeferMethodImport(method))
                        continue;

                    ImportMethodDefinition(method, existing);
                }
            }

            return existing;
        }

        private void CopyMethods(TypeDefinition source, TypeDefinition target)
        {
            foreach (var method in source.Methods)
            {
                if (DeferMethodImport(method))
                    continue;

                ImportMethodDefinition(method, target);
            }
        }

        private void CopyProperties(TypeDefinition source, TypeDefinition target)
        {
            if (CompactMode)
                return;

            foreach (var sourceDefinition in source.Properties)
            {
                var targetDefinition = new PropertyDefinition(sourceDefinition.Name, sourceDefinition.Attributes, InternalImportType(sourceDefinition.PropertyType, null))
                {
                    GetMethod = ImportMethodDefinition(sourceDefinition.GetMethod, target),
                    SetMethod = ImportMethodDefinition(sourceDefinition.SetMethod, target)
                };

                CopyAttributes(sourceDefinition, targetDefinition);

                target.Properties.Add(targetDefinition);
            }
        }

        private void CopyEvents(TypeDefinition source, TypeDefinition target)
        {
            if (CompactMode)
                return;

            foreach (var sourceDefinition in source.Events)
            {
                var targetDefinition = new EventDefinition(sourceDefinition.Name, sourceDefinition.Attributes, InternalImportType(sourceDefinition.EventType, null))
                {
                    AddMethod = ImportMethodDefinition(sourceDefinition.AddMethod, target),
                    RemoveMethod = ImportMethodDefinition(sourceDefinition.RemoveMethod, target)
                };

                CopyAttributes(sourceDefinition, targetDefinition);

                target.Events.Add(targetDefinition);
            }
        }

        private void CopyFields(TypeDefinition source, TypeDefinition target)
        {
            foreach (var sourceDefinition in source.Fields)
            {
                var fieldName = sourceDefinition.Name;

                var targetDefinition = new FieldDefinition(fieldName, sourceDefinition.Attributes, InternalImportType(sourceDefinition.FieldType, null))
                {
                    InitialValue = sourceDefinition.InitialValue,
                    Offset = sourceDefinition.Offset
                };

                if (sourceDefinition.HasConstant)
                {
                    targetDefinition.Constant = sourceDefinition.Constant;
                }

                if (sourceDefinition.HasMarshalInfo)
                {
                    targetDefinition.MarshalInfo = sourceDefinition.MarshalInfo;
                }

                CopyAttributes(sourceDefinition, targetDefinition);

                target.Fields.Add(targetDefinition);
            }
        }

        private MethodDefinition? ImportMethodDefinition(MethodDefinition? sourceDefinition, TypeDefinition targetType)
        {
            if (sourceDefinition == null)
                return null;

            if (IsLocalOrExternalReference(sourceDefinition.DeclaringType))
                return sourceDefinition;

            if (_targetMethods.TryGetValue(sourceDefinition, out var target))
                return target;

            target = new MethodDefinition(sourceDefinition.Name, sourceDefinition.Attributes, TemporaryPlaceholderType)
            {
                ImplAttributes = sourceDefinition.ImplAttributes
            };

            _targetMethods.Add(sourceDefinition, target);

            foreach (var sourceOverride in sourceDefinition.Overrides)
            {
                switch (sourceOverride)
                {
                    case MethodDefinition methodDefinition:
                        target.Overrides.Add(ImportMethodDefinition(methodDefinition, targetType));
                        break;

                    default:
                        target.Overrides.Add(ImportMethodReference(sourceOverride));
                        break;
                }
            }

            CopyAttributes(sourceDefinition, target);
            CopyGenericParameters(sourceDefinition, target);
            CopyParameters(sourceDefinition, target);

            targetType.Methods.Add(target);

            if (sourceDefinition.IsPInvokeImpl)
            {
                var moduleRef = TargetModule.ModuleReferences
                    .FirstOrDefault(mr => mr.Name == sourceDefinition.PInvokeInfo.Module.Name);

                if (moduleRef == null)
                {
                    moduleRef = new ModuleReference(sourceDefinition.PInvokeInfo.Module.Name);
                    TargetModule.ModuleReferences.Add(moduleRef);
                }

                target.PInvokeInfo = new PInvokeInfo(sourceDefinition.PInvokeInfo.Attributes, sourceDefinition.PInvokeInfo.EntryPoint, moduleRef);
            }

            CopyReturnType(sourceDefinition, target);

            ImportMethodBody(sourceDefinition, target);

            return target;
        }

        private void CopyReturnType(IMethodSignature source, MethodReference target)
        {
            var sourceReturnType = source.MethodReturnType;

            var targetReturnType = new MethodReturnType(target)
            {
                ReturnType = InternalImportType(source.ReturnType, target),
                MarshalInfo = sourceReturnType.MarshalInfo,
                Attributes = sourceReturnType.Attributes,
                Name = sourceReturnType.Name
            };

            if (sourceReturnType.HasConstant)
            {
                targetReturnType.Constant = sourceReturnType.Constant;
            }

            CopyAttributes(sourceReturnType, targetReturnType);

            target.MethodReturnType = targetReturnType;
        }

        private void CopyAttributes(Mono.Cecil.ICustomAttributeProvider source, Mono.Cecil.ICustomAttributeProvider target)
        {
            if (!source.HasCustomAttributes)
                return;

            foreach (var sourceAttribute in source.CustomAttributes)
            {
                var constructor = ImportMethod(sourceAttribute.Constructor);

                if (constructor == null)
                    continue;

                var targetAttribute = new CustomAttribute(constructor);

                if (sourceAttribute.HasConstructorArguments)
                {
                    foreach (var a in sourceAttribute.ConstructorArguments)
                    {
                        var value = a.Value;
                        if (value is TypeReference typeReference)
                        {
                            value = InternalImportType(typeReference, null);
                        }

                        targetAttribute.ConstructorArguments.Add(new CustomAttributeArgument(InternalImportType(a.Type, null), value));
                    }
                }

                if (sourceAttribute.HasProperties)
                {
                    foreach (var property in sourceAttribute.Properties)
                    {
                        targetAttribute.Properties.Add(new Mono.Cecil.CustomAttributeNamedArgument(property.Name, new CustomAttributeArgument(InternalImportType(property.Argument.Type, null), property.Argument.Value)));
                    }
                }

                if (sourceAttribute.HasFields)
                {
                    foreach (var field in sourceAttribute.Fields)
                    {
                        targetAttribute.Fields.Add(new Mono.Cecil.CustomAttributeNamedArgument(field.Name, new CustomAttributeArgument(InternalImportType(field.Argument.Type, null), field.Argument.Value)));
                    }
                }

                target.CustomAttributes.Add(targetAttribute);
            }
        }

        public void ImportMethodBody(MethodDefinition source, MethodDefinition target)
        {
            if (!source.HasBody)
                return;

            var sourceMethodBody = source.Body;
            var targetMethodBody = target.Body;

            targetMethodBody.InitLocals = sourceMethodBody.InitLocals;

            foreach (var sourceVariable in sourceMethodBody.Variables)
            {
                targetMethodBody.Variables.Add(new VariableDefinition(InternalImportType(sourceVariable.VariableType, target)));
            }

            ExecuteDeferred(Priority.Instructions, () => CopyInstructions(source, target));
        }

        private void CopyParameters(IMethodSignature sourceMethod, MethodReference targetMethod)
        {
            foreach (var sourceParameter in sourceMethod.Parameters)
            {
                var targetParameter = new ParameterDefinition(sourceParameter.Name, sourceParameter.Attributes, InternalImportType(sourceParameter.ParameterType, targetMethod));

                CopyAttributes(sourceParameter, targetParameter);

                if (sourceParameter.HasMarshalInfo)
                {
                    targetParameter.MarshalInfo = sourceParameter.MarshalInfo;
                }

                if (sourceParameter.HasConstant)
                {
                    targetParameter.Constant = sourceParameter.Constant;
                }

                targetMethod.Parameters.Add(targetParameter);
            }
        }

        private void CopyGenericParameters(TypeDefinition source, TypeDefinition target)
        {
            if (!source.HasGenericParameters)
                return;

            foreach (var sourceParameter in source.GenericParameters)
            {
                var targetParameter = new GenericParameter(sourceParameter.Name, InternalImportType(sourceParameter.DeclaringType, null))
                {
                    Attributes = sourceParameter.Attributes
                };

                CopyAttributes(sourceParameter, targetParameter);
                CopyConstraints(sourceParameter, targetParameter, null);

                target.GenericParameters.Add(targetParameter);
            }
        }

        private void CopyGenericParameters(MethodReference source, MethodReference target)
        {
            if (!source.HasGenericParameters)
                return;

            foreach (var sourceParameter in source.GenericParameters)
            {
                var provider = sourceParameter.Type == GenericParameterType.Method
                    ? (IGenericParameterProvider)target
                    : InternalImportType(sourceParameter.DeclaringType, null);

                var targetParameter = new GenericParameter(sourceParameter.Name, provider)
                {
                    Attributes = sourceParameter.Attributes
                };

                CopyAttributes(sourceParameter, targetParameter);
                CopyConstraints(sourceParameter, targetParameter, target);

                target.GenericParameters.Add(targetParameter);
            }
        }

        private void CopyConstraints(GenericParameter sourceParameter, GenericParameter targetParameter, MethodReference? targetMethod)
        {
            if (!sourceParameter.HasConstraints)
                return;

            foreach (var source in sourceParameter.Constraints)
            {
                var target = new GenericParameterConstraint(InternalImportType(source.ConstraintType, targetMethod));
                CopyAttributes(source, target);
                targetParameter.Constraints.Add(target);
            }
        }

        private void CopyExceptionHandlers(Mono.Cecil.Cil.MethodBody source, Mono.Cecil.Cil.MethodBody target)
        {
            if (!source.HasExceptionHandlers)
            {
                return;
            }

            foreach (var sourceHandler in source.ExceptionHandlers)
            {
                var targetHandler = new ExceptionHandler(sourceHandler.HandlerType);
                var sourceInstructions = source.Instructions;
                var targetInstructions = target.Instructions;

                if (sourceHandler.TryStart != null)
                {
                    targetHandler.TryStart = targetInstructions[sourceInstructions.IndexOf(sourceHandler.TryStart)];
                }

                if (sourceHandler.TryEnd != null)
                {
                    targetHandler.TryEnd = targetInstructions[sourceInstructions.IndexOf(sourceHandler.TryEnd)];
                }

                if (sourceHandler.HandlerStart != null)
                {
                    targetHandler.HandlerStart = targetInstructions[sourceInstructions.IndexOf(sourceHandler.HandlerStart)];
                }

                if (sourceHandler.HandlerEnd != null)
                {
                    targetHandler.HandlerEnd = targetInstructions[sourceInstructions.IndexOf(sourceHandler.HandlerEnd)];
                }

                if (sourceHandler.FilterStart != null)
                {
                    targetHandler.FilterStart = targetInstructions[sourceInstructions.IndexOf(sourceHandler.FilterStart)];
                }

                if (sourceHandler.CatchType != null)
                {
                    targetHandler.CatchType = InternalImportType(sourceHandler.CatchType, null);
                }

                target.ExceptionHandlers.Add(targetHandler);
            }
        }

        private void CopyInstructions(MethodDefinition source, MethodDefinition target)
        {
            var targetDebugInformation = target.DebugInformation;
            var sourceDebugInformation = source.DebugInformation;

            var sourceBody = source.Body;
            var targetBody = target.Body;

            var sourceInstructions = sourceBody.Instructions;
            var targetInstructions = targetBody.Instructions;

            var instructionMap = new Dictionary<Instruction, Instruction>();

            foreach (var sourceInstruction in sourceInstructions)
            {
                var targetInstruction = CloneInstruction(sourceInstruction, target, instructionMap);

                instructionMap.Add(sourceInstruction, targetInstruction);

                targetInstructions.Add(targetInstruction);

                var sequencePoint = sourceDebugInformation?.GetSequencePoint(sourceInstruction);
                if (sequencePoint != null)
                    targetDebugInformation?.SequencePoints?.Add(CloneSequencePoint(targetInstruction, sequencePoint));
            }

            CopyExceptionHandlers(sourceBody, targetBody);

            if (sourceDebugInformation?.Scope == null || true != targetDebugInformation?.HasSequencePoints)
                return;

            var scope = targetDebugInformation.Scope = new ScopeDebugInformation(targetInstructions.First(), targetInstructions.Last());

            foreach (var variable in sourceDebugInformation.Scope.Variables)
            {
                var targetVariable = targetBody.Variables[variable.Index];

                scope.Variables.Add(new VariableDebugInformation(targetVariable, variable.Name));
            }
        }

        [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
        private Instruction CloneInstruction(Instruction source, MethodDefinition targetMethod, IReadOnlyDictionary<Instruction, Instruction> instructionMap)
        {
            var targetInstruction = (Instruction)_instructionConstructor.Invoke(new[] { source.OpCode, source.Operand });

            switch (targetInstruction.Operand)
            {
                case MethodDefinition sourceMethodDefinition:
                    var targetType = ImportTypeDefinition(sourceMethodDefinition.DeclaringType);
                    ExecuteDeferred(Priority.Operands, () => targetInstruction.Operand = ImportMethodDefinition(sourceMethodDefinition, targetType));
                    break;

                case GenericInstanceMethod genericInstanceMethod:
                    ExecuteDeferred(Priority.Operands, () => targetInstruction.Operand = ImportGenericInstanceMethod(genericInstanceMethod));
                    break;

                case MethodReference sourceMethodReference:
                    ExecuteDeferred(Priority.Operands, () => targetInstruction.Operand = ImportMethodReference(sourceMethodReference));
                    break;

                case TypeReference typeReference:
                    targetInstruction.Operand = InternalImportType(typeReference, targetMethod);
                    break;

                case FieldReference fieldReference:
                    ExecuteDeferred(Priority.Operands, () => targetInstruction.Operand = new FieldReference(fieldReference.Name, InternalImportType(fieldReference.FieldType, targetMethod), InternalImportType(fieldReference.DeclaringType, targetMethod)));
                    break;

                case Instruction instruction:
                    ExecuteDeferred(Priority.Operands, () => targetInstruction.Operand = instructionMap[instruction]);
                    break;

                case Instruction[] instructions:
                    ExecuteDeferred(Priority.Operands, () => targetInstruction.Operand = instructions.Select(instruction => instructionMap[instruction]).ToArray());
                    break;
            }

            return targetInstruction;
        }

        private static SequencePoint CloneSequencePoint(Instruction instruction, SequencePoint sequencePoint)
        {
            return new SequencePoint(instruction, sequencePoint.Document)
            {
                StartLine = sequencePoint.StartLine,
                StartColumn = sequencePoint.StartColumn,
                EndLine = sequencePoint.EndLine,
                EndColumn = sequencePoint.EndColumn
            };
        }

        [return: NotNullIfNotNull("source")]
        private TypeReference? InternalImportType(TypeReference? source, MethodReference? targetMethod)
        {
            switch (source)
            {
                case null:
                    return null;

                case TypeDefinition typeDefinition:
                    return ImportTypeDefinition(typeDefinition);

                case GenericParameter genericParameter:
                    return ImportGenericParameter(genericParameter, targetMethod);

                case GenericInstanceType genericInstanceType:
                    return ImportGenericInstanceType(genericInstanceType, targetMethod);

                case ByReferenceType byReferenceType:
                    return new ByReferenceType(InternalImportType(byReferenceType.ElementType, targetMethod));

                case ArrayType arrayType:
                    return new ArrayType(InternalImportType(arrayType.ElementType, targetMethod), arrayType.Rank);

                case RequiredModifierType requiredModifierType:
                    return new RequiredModifierType(InternalImportType(requiredModifierType.ModifierType, targetMethod), InternalImportType(requiredModifierType.ElementType, targetMethod));

                default:
                    return ImportTypeReference(source, targetMethod);
            }
        }

        private TypeReference ImportTypeReference(TypeReference source, MethodReference? targetMethod)
        {
            Debug.Assert(source.GetType() == typeof(TypeReference));

            if (IsLocalOrExternalReference(source))
            {
                return TargetModule.ImportReference(source);
            }

            var typeDefinition = source.Resolve();

            if (typeDefinition == null)
                throw new InvalidOperationException($"Unable to resolve type {source}");

            return InternalImportType(typeDefinition, targetMethod);
        }

        private TypeReference ImportGenericInstanceType(GenericInstanceType source, MethodReference? targetMethod)
        {
            var target = new GenericInstanceType(InternalImportType(source.ElementType, targetMethod));

            foreach (var genericArgument in source.GenericArguments)
            {
                target.GenericArguments.Add(InternalImportType(genericArgument, targetMethod));
            }

            return target;
        }

        private TypeReference ImportGenericParameter(GenericParameter source, MethodReference? targetMethod)
        {
            var genericParameterProvider = (source.Type == GenericParameterType.Method)
                ? targetMethod ?? throw new InvalidOperationException("Need a method reference for generic method parameter.")
                : (IGenericParameterProvider)InternalImportType(source.DeclaringType, targetMethod)!;

            var index = source.Position;

            if (index < genericParameterProvider?.GenericParameters.Count)
            {
                return genericParameterProvider.GenericParameters[index];
            }

            return source;
        }

        public MethodReference? ImportMethod(MethodReference? source)
        {
            switch (source)
            {
                case MethodDefinition methodDefinition:
                    return ImportMethodDefinition(methodDefinition, ImportTypeDefinition(source.DeclaringType.Resolve()));

                case null:
                    return null;

                default:
                    return ImportMethodReference(source);
            }
        }

        private MethodReference ImportMethodReference(MethodReference source)
        {
            Debug.Assert(source.GetType() == typeof(MethodReference));

            ImportReferencedMethod(source);

            var target = new MethodReference(source.Name, TemporaryPlaceholderType)
            {
                HasThis = source.HasThis,
                ExplicitThis = source.ExplicitThis,
                CallingConvention = source.CallingConvention
            };

            CopyGenericParameters(source, target);
            CopyParameters(source, target);

            target.DeclaringType = InternalImportType(source.DeclaringType, target);

            CopyReturnType(source, target);

            return target;
        }

        public void ImportReferencedMethod(MethodReference reference)
        {
            Debug.Assert(reference.GetType() == typeof(MethodReference));

            if (!IsLocalOrExternalReference(reference.DeclaringType))
            {
                ImportMethod(reference.Resolve());
            }
        }

        private GenericInstanceMethod ImportGenericInstanceMethod(GenericInstanceMethod source)
        {
            var elementMethod = source.ElementMethod;

            switch (source.ElementMethod)
            {
                case MethodDefinition sourceMethodDefinition:
                    elementMethod = ImportMethodDefinition(sourceMethodDefinition, ImportTypeDefinition(sourceMethodDefinition.DeclaringType))!;
                    break;

                case GenericInstanceMethod genericInstanceMethod:
                    elementMethod = ImportGenericInstanceMethod(genericInstanceMethod);
                    break;

                // ReSharper disable once PatternAlwaysOfType
                case MethodReference sourceMethodReference:
                    elementMethod = ImportMethodReference(sourceMethodReference);
                    break;
            }

            var target = new GenericInstanceMethod(elementMethod);

            foreach (var genericArgument in source.GenericArguments)
            {
                target.GenericArguments.Add(InternalImportType(genericArgument, target));
            }

            return target;
        }

        public bool IsLocalOrExternalReference(TypeReference typeReference)
        {
            var scope = typeReference.Scope;

            string assemblyName;

            switch (scope)
            {
                case AssemblyNameReference assemblyNameReference:
                    assemblyName = assemblyNameReference.FullName;
                    break;

                case ModuleDefinition moduleDefinition:
                    assemblyName = moduleDefinition.Assembly.FullName;
                    break;

                default:
                    return false;
            }

            if (string.Equals(TargetModule.Assembly.FullName, assemblyName, StringComparison.OrdinalIgnoreCase))
                return true;

            return !_sourceModuleDefinitions.ContainsKey(assemblyName)
                && !ResolveModule(typeReference, assemblyName);
        }

        private bool ResolveModule(TypeReference typeReference, string assemblyName)
        {
            var module = ModuleResolver?.Resolve(typeReference, assemblyName);

            if (module == null)
                return false;

            RegisterSourceModule(module);
            return true;

        }

        private void ExecuteDeferred(Priority priority, Action action)
        {
            switch (priority)
            {
                case Priority.Instructions:
                    _deferredActions.Insert(0, action);
                    break;

                default:
                    _deferredActions.Add(action);
                    break;
            }
        }

        private TypeReference TemporaryPlaceholderType => new("temporary", "type", TargetModule, TargetModule);
        
        public void ProcessDeferredActions()
        {
            while (true)
            {
                var action = _deferredActions.FirstOrDefault();
                if (action == null)
                    break;

                _deferredActions.RemoveAt(0);
                action();
            }
        }
    }

    internal static class CodeImporterExtensions
    {
        public static bool IsStatic(this TypeDefinition type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        public static bool IsEmbeddedType(this TypeDefinition type)
        {
            if (type.IsPublic)
                return false;

            if (!type.IsStatic() && (type.BaseType?.Name != "Attribute"))
                return false;

            if (type.DeclaringType != null)
            {
                return type.DeclaringType.IsEmbeddedType();
            }

            if (type.Namespace?.Split('.').FirstOrDefault() == "System")
                return true;

            if (!type.HasCustomAttributes)
                return false;

            return type.CustomAttributes.Any(attr => attr.AttributeType.FullName == "Microsoft.CodeAnalysis.EmbeddedAttribute");
        }

        public static bool HasSameNameAndSignature(this MethodDefinition method1, MethodDefinition method2)
        {
            return method1.Name == method2.Name
                   && method1.Parameters.Zip(method2.Parameters, IsSameParameter).All(item => item);
        }

        private static bool IsSameParameter(ParameterDefinition left, ParameterDefinition right)
        {
            return left.ParameterType.FullName == right.ParameterType.FullName && left.Attributes == right.Attributes;
        }
    }

    internal interface IModuleResolver
    {
        ModuleDefinition? Resolve(TypeReference typeReference, string assemblyName);
    }
}
