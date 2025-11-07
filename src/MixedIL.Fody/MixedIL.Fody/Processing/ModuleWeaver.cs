using System.IO;
using Fody;
using FodyTools;
using MoreFodyHelpers;
using MoreFodyHelpers.Processing;

namespace MixedIL.Fody.Processing;

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

        using var context = new ModuleWeavingContext(ModuleDefinition, WeaverAnchors.AssemblyName, ProjectDirectoryPath);

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

        context.AddIgnoresAccessCheck();
        context.AddIgnoresAccessCheck(AssemblyNames.MsCorLib);
        context.AddIgnoresAccessCheck(AssemblyNames.SystemPrivateCoreLib);
        context.RemoveReference(WeaverAnchors.AssemblyName, this);
    }

    protected virtual void AddError(string message, SequencePoint? sequencePoint)
        => _log.Error(message, sequencePoint);

    public override IEnumerable<string> GetAssembliesForScanning() => [];

    private FileInfo GetILFile()
    {
        var fileInfo = new FileInfo(ModuleDefinition.FileName);
        var name = Path.GetFileNameWithoutExtension(fileInfo.Name);
        var il = name + ".il.dll";
        return new FileInfo(Path.Combine(fileInfo.DirectoryName!, il));
    }
}
