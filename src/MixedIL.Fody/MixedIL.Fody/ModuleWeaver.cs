using System.IO;
using Fody;
using FodyTools;
using MixedIL.Fody.Processing;
using MoreFodyHelpers;

namespace MixedIL.Fody;

public class ModuleWeaver : BaseModuleWeaver
{
    public const string TypeNotFoundFormat = "Cannot find type {0}";
    public const string MethodNotFoundFormat = "Cannot find method in type {0} by name {1}";

    private readonly IWeaverLogger _log;

    public ModuleWeaver()
    {
        _log = new WeaverLogger(this);
    }

    public override void Execute()
    {
        var iLFile = GetILFile();
        if (iLFile.Exists == false)
        {
            throw new InvalidOperationException("Cannot find " + iLFile.FullName);
        }

        var readerParameters = new ReaderParameters
        {
            AssemblyResolver = AssemblyResolver,
            ReadWrite = false,
            ReadSymbols = true,
        };
        using var iLModule = ModuleDefinition.ReadModule(iLFile.FullName, readerParameters);

        var codeImporter = new CodeImporter(ModuleDefinition)
        {
            ModuleResolver = new LocalReferenceModuleResolver(_log, ReferenceCopyLocalPaths),
        };
        var typeMethods = iLModule.GetTypes().ToDictionary(m => m.FullName, m => m.Methods.ToDictionary(x => x.FullName, x => x));

        foreach (var type in ModuleDefinition.GetTypes())
        {
            foreach (var method in type.Methods)
            {
                try
                {
                    if (!MethodWeaver.NeedsProcessing(method))
                        continue;

                    if (!typeMethods.TryGetValue(type.FullName, out var methods))
                    {
                        throw new InvalidOperationException(string.Format(TypeNotFoundFormat, type.FullName));
                    }

                    if (!methods.TryGetValue(method.FullName, out var iLMethod))
                    {
                        throw new InvalidOperationException(string.Format(MethodNotFoundFormat, type.FullName, method.FullName));
                    }

                    _log.Debug($"Processing: {method.FullName}");
                    new MethodWeaver(method, iLMethod, codeImporter).Process();
                }
                catch (WeavingException ex)
                {
                    AddError(ex.Message, ex.SequencePoint);
                    break;
                }
                catch (Exception ex)
                {
                    AddError(ex.Message, method.GetSequencePoint());
                    break;
                }
            }
        }
        ModuleDefinition.AddIgnoresAccessCheck();
        RemoveLibReference();
    }

    protected virtual void AddError(string message, SequencePoint? sequencePoint)
        => _log.Error(message, sequencePoint);

    public override IEnumerable<string> GetAssembliesForScanning() => Enumerable.Empty<string>();

    private FileInfo GetILFile()
    {
        var fileInfo = new FileInfo(ModuleDefinition.FileName);
        var name = Path.GetFileNameWithoutExtension(fileInfo.Name);
        var il = name + ".il.dll";
        return new FileInfo(Path.Combine(fileInfo.DirectoryName!, il));
    }

    private void RemoveLibReference()
    {
        var libRef = ModuleDefinition.AssemblyReferences.FirstOrDefault(r => IsMixedILAssembly(r));
        if (libRef == null)
            return;

        var importScopes = new HashSet<ImportDebugInformation>();

        foreach (var method in ModuleDefinition.GetTypes().SelectMany(t => t.Methods))
        {
            foreach (var scope in method.DebugInformation.GetScopes())
                ProcessScope(scope);
        }

        ModuleDefinition.AssemblyReferences.Remove(libRef);

        var copyLocalFilesToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            libRef.Name + ".dll",
            libRef.Name + ".xml",
            libRef.Name + ".pdb" // We don't ship this, but future-proof that ;)
        };

        ReferenceCopyLocalPaths.RemoveAll(i => copyLocalFilesToRemove.Contains(Path.GetFileName(i)));

        WriteDebug("References removed.");

        void ProcessScope(ScopeDebugInformation scope)
        {
            ProcessImportScope(scope.Import);

            if (scope.HasScopes)
            {
                foreach (var childScope in scope.Scopes)
                    ProcessScope(childScope);
            }
        }

        void ProcessImportScope(ImportDebugInformation? importScope)
        {
            if (importScope == null || !importScopes.Add(importScope))
                return;

            importScope.Targets.RemoveWhere(t => IsMixedILAssembly(t.AssemblyReference));
            ProcessImportScope(importScope.Parent);
        }

        static bool IsMixedILAssembly(AssemblyNameReference? assembly)
        {
            return assembly?.Name == "MixedIL";
        }
    }
}
