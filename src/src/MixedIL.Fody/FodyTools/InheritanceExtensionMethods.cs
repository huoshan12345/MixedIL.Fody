namespace FodyTools
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;

    internal static class InheritanceExtensionMethods
    {
        public static IEnumerable<TypeReference> EnumerateInterfaces(this TypeDefinition typeDefinition, TypeReference typeReference)
        {
            foreach (var implementation in typeDefinition.Interfaces)
            {
                var interfaceType = implementation.InterfaceType;

                yield return interfaceType.ResolveGenericArguments(typeReference);
            }
        }

        public static IEnumerable<MethodDefinition> EnumerateOverridesAndImplementations(this MethodDefinition method)
        {
            if (!method.HasThis)
            {
                yield break;
            }

            if (method.IsPrivate)
            {
                if (method.HasOverrides)
                {
                    foreach (var methodOverride in method.Overrides)
                    {
                        yield return methodOverride.Resolve();
                    }
                }

                yield break;
            }

            var declaringType = method.DeclaringType;

            foreach (var interfaceType in declaringType.EnumerateInterfaces(declaringType))
            {
                var interfaceMethod = interfaceType.Find(method);
                if (interfaceMethod == null)
                {
                    continue;
                }

                if (declaringType.HasExplicitInterfaceImplementation(method))
                {
                    continue;
                }

                yield return interfaceMethod;
            }

            var baseMethod = method.FindBase()?.Resolve();
            if (baseMethod != null)
            {
                yield return baseMethod;

                foreach (var baseImplementation in baseMethod.EnumerateOverridesAndImplementations())
                {
                    yield return baseImplementation;
                }
            }
        }

        public static IEnumerable<PropertyDefinition> EnumerateOverridesAndImplementations(this PropertyDefinition property)
        {
            if (!property.HasThis)
            {
                yield break;
            }

            var propertyOverrides = property.EnumerateOverrides().ToArray();
            if (propertyOverrides.Any())
            {
                foreach (var propertyOverride in propertyOverrides)
                {
                    yield return propertyOverride;
                }

                yield break;
            }

            var declaringType = property.GetMethod?.DeclaringType;
            if (declaringType != null)
            {
                foreach (var interfaceType in declaringType.EnumerateInterfaces(declaringType))
                {
                    var interfaceProperty = interfaceType.Find(property);
                    if (interfaceProperty == null)
                        continue;

                    if (declaringType.HasExplicitInterfaceImplementation(property))
                        continue;

                    yield return interfaceProperty;
                }
            }

            var baseProperty = property.GetBaseProperty();
            if (baseProperty != null)
            {
                yield return baseProperty;

                foreach (var baseImplementation in baseProperty.EnumerateOverridesAndImplementations())
                {
                    yield return baseImplementation;
                }
            }
        }

        public static bool IsInterfaceImplementation(this MethodDefinition method)
        {
            if (!method.HasThis)
            {
                return false;
            }

            var declaringType = method.DeclaringType;

            foreach (var interfaceType in declaringType.EnumerateInterfaces(declaringType))
            {
                var interfaceMethod = interfaceType.Find(method);
                if (interfaceMethod == null)
                {
                    continue;
                }

                if (declaringType.HasExplicitInterfaceImplementation(method))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public static bool IsInterfaceImplementation(this PropertyDefinition property)
        {
            if (!property.HasThis)
            {
                return false;
            }

            var declaringType = property.DeclaringType;

            foreach (var interfaceType in declaringType.EnumerateInterfaces(declaringType))
            {
                var interfaceMethod = interfaceType.Find(property);
                if (interfaceMethod == null)
                {
                    continue;
                }

                if (declaringType.HasExplicitInterfaceImplementation(property))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public static MethodReference? FindBase(this MethodDefinition method)
        {
            if (!method.IsVirtual || method.IsNewSlot)
                return null;

            TypeReference? type = method.DeclaringType;

            for (type = type.Resolve().BaseType?.ResolveGenericArguments(type); type != null; type = type.Resolve().BaseType?.ResolveGenericArguments(type))
            {
                var matchingMethod = type.Find(method);
                if (matchingMethod != null)
                    return matchingMethod;
            }

            return null;
        }

        public static MethodDefinition? Find(this TypeReference declaringType, MethodReference reference)
        {
            return declaringType.Resolve().Methods
                .FirstOrDefault(method => HasSameSignature(declaringType, method, reference.DeclaringType, reference.Resolve()));
        }

        public static PropertyDefinition? Find(this TypeReference declaringType, PropertyReference reference)
        {
            return declaringType.Resolve().Properties
                .FirstOrDefault(property => HasSameSignature(declaringType, property, reference.DeclaringType, reference.Resolve()));
        }

        public static bool HasSameSignature(TypeReference declaringType1, MethodDefinition method1, TypeReference declaringType2, MethodDefinition method2)
        {
            var resolvedGenericParameter1 = method1.ReturnType.ResolveGenericParameter(declaringType1);
            var resolvedGenericParameter2 = method2.ReturnType.ResolveGenericParameter(declaringType2);
            var areaAllParametersOfSameType = AreaAllParametersOfSameType(declaringType1, method1, declaringType2, method2);
            var referenceEquals = resolvedGenericParameter1.FullName == resolvedGenericParameter2.FullName;
            return method1.Name == method2.Name
                   && referenceEquals
                   && method1.GenericParameters.Count == method2.GenericParameters.Count
                   && areaAllParametersOfSameType;
        }

        public static bool HasSameSignature(TypeReference declaringType1, PropertyDefinition property1, TypeReference declaringType2, PropertyDefinition property2)
        {
            var resolvedGenericParameter1 = property1.PropertyType.ResolveGenericParameter(declaringType1);
            var resolvedGenericParameter2 = property2.PropertyType.ResolveGenericParameter(declaringType2);
            return property1.Name == property2.Name
                   && resolvedGenericParameter1.FullName == resolvedGenericParameter2.FullName
                   && AreaAllParametersOfSameType(declaringType1, property1, declaringType2, property2);
        }

        public static bool AreaAllParametersOfSameType(TypeReference declaringType1, IMethodSignature method1, TypeReference declaringType2, IMethodSignature method2)
        {
            if (!method2.HasParameters)
                return !method1.HasParameters;

            if (!method1.HasParameters)
                return false;

            if (method1.Parameters.Count != method2.Parameters.Count)
                return false;

            for (var i = 0; i < method1.Parameters.Count; i++)
            {
                var p1 = method1.Parameters[i].ParameterType.ResolveGenericParameter(declaringType1);

                var p2 = method2.Parameters[i].ParameterType.ResolveGenericParameter(declaringType2);

                if (p1.FullName != p2.FullName)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AreaAllParametersOfSameType(TypeReference declaringType1, PropertyDefinition property1, TypeReference declaringType2, PropertyDefinition property2)
        {
            if (!property2.HasParameters)
                return !property1.HasParameters;

            if (!property1.HasParameters)
                return false;

            if (property1.Parameters.Count != property2.Parameters.Count)
                return false;

            for (var i = 0; i < property1.Parameters.Count; i++)
            {
                var p1 = property1.Parameters[i].ParameterType.ResolveGenericParameter(declaringType1);

                var p2 = property2.Parameters[i].ParameterType.ResolveGenericParameter(declaringType2);

                if (p1.FullName != p2.FullName)
                { return false; }
            }

            return true;
        }

        public static bool HasExplicitInterfaceImplementation(this TypeDefinition type, MethodDefinition? method)
        {
            if (method == null)
                return false;

            return method.DeclaringType.Methods
                .Where(m => m != method && m.HasOverrides)
                .SelectMany(m => m.Overrides)
                .Any(methodReference => HasSameSignature(type, method, methodReference.DeclaringType, methodReference.Resolve()));
        }

        public static bool HasExplicitInterfaceImplementation(this TypeDefinition type, PropertyDefinition property)
        {
            return type.HasExplicitInterfaceImplementation(property.GetMethod);
        }

        public static PropertyDefinition? GetBaseProperty(this PropertyDefinition property)
        {
            var getMethod = property.GetMethod;
            var getMethodBase = getMethod?.FindBase();

            if (getMethodBase != null)
            {
                var baseProperty = getMethodBase.DeclaringType.Resolve().Properties.FirstOrDefault(p => p.GetMethod == getMethodBase);
                if (baseProperty != null)
                    return baseProperty;
            }

            return null;
        }

        public static IEnumerable<MethodReference> EnumerateOverrides(this MethodDefinition? method)
        {
            if (method == null)
                yield break;

            if (method.HasOverrides)
            {
                // Explicit interface implementations...
                foreach (var reference in method.Overrides)
                {
                    yield return reference;
                }
            }
        }

        public static IEnumerable<PropertyDefinition> EnumerateOverrides(this PropertyDefinition property)
        {
            var getMethod = property.GetMethod;
            foreach (var getOverride in getMethod.EnumerateOverrides())
            {
                var typeDefinition = getOverride.DeclaringType.Resolve();
                var ovr = typeDefinition.Properties.FirstOrDefault(p => p.GetMethod == getOverride.Resolve());
                if (ovr != null)
                {
                    yield return ovr;
                }
            }
        }

        public static TypeReference ResolveGenericParameter(this TypeReference parameterType, TypeReference declaringType)
        {
            if (parameterType.IsGenericParameter && declaringType.IsGenericInstance)
            {
                var parameterIndex = ((GenericParameter)parameterType).Position;
                parameterType = ((GenericInstanceType)declaringType).GenericArguments[parameterIndex];
            }

            return parameterType;
        }

        public static TypeReference ResolveGenericArguments(this TypeReference baseType, TypeReference derivedType)
        {
            if (!baseType.IsGenericInstance)
                return baseType;

            if (!derivedType.IsGenericInstance)
                return baseType;

            var genericBase = (GenericInstanceType)baseType;
            if (!genericBase.HasGenericArguments)
                return baseType;

            var result = new GenericInstanceType(baseType);

            result.GenericArguments.AddRange(genericBase.GenericArguments.Select(arg => ResolveGenericParameter(arg, derivedType)));

            return result;
        }
    }
}
