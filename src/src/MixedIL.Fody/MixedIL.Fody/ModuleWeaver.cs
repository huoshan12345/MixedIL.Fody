using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fody;
using FodyTools;
using MixedIL.Fody.Extensions;
using MixedIL.Fody.Processing;
using MixedIL.Fody.Support;
using Mono.Cecil;

namespace MixedIL.Fody
{
    public class ModuleWeaver : AbstractModuleWeaver
    {
        public const string TypeNotFoundFormat = "Cannot find type {0}";
        public const string MethodNotFoundFormat = "Cannot find method in type {0} by name {1}";

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
                ModuleResolver = new LocalReferenceModuleResolver(this, ReferenceCopyLocalPaths),
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

                        WriteDebug($"Processing: {method.FullName}");
                        new MethodWeaver(method, iLMethod, codeImporter).Process();
                    }
                    catch (WeavingException ex)
                    {
                        WriteError(ex.Message, ex.SequencePoint);
                        break;
                    }
                    catch (Exception ex)
                    {
                        WriteError(ex.Message, method.GetSequencePoint());
                        break;
                    }
                }
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning() => Enumerable.Empty<string>();

        private FileInfo GetILFile()
        {
            var fileInfo = new FileInfo(ModuleDefinition.FileName);
            var (name, _) = PathHelper.ExtractFileName(fileInfo.Name);
            var il = name + ".il.dll";
            return new FileInfo(Path.Combine(fileInfo.DirectoryName!, il));
        }
    }
}
