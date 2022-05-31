using Fody;
using MixedIL.Fody.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MixedIL.Fody.Support;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MixedIL.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        internal const string TypeNotFoundFormat = "Cannot find type {0}";
        internal const string MethodNotFoundFormat = "Cannot find method of {0} by name {1}";

        private readonly Logger _log;

        public ModuleWeaver()
        {
            _log = new Logger(this);
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
                        new MethodWeaver(method, iLMethod, _log).Process();
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
        }

        public override IEnumerable<string> GetAssembliesForScanning() => Enumerable.Empty<string>();

        protected virtual void AddError(string message, SequencePoint? sequencePoint)
        {
            _log.Error(message, sequencePoint);
        }

        private FileInfo GetILFile()
        {
            var fileInfo = new FileInfo(ModuleDefinition.FileName);
            var (name, _) = PathHelper.ExtractFileName(fileInfo.Name);
            var il = name + ".il.dll";
            return new FileInfo(Path.Combine(fileInfo.DirectoryName!, il));
        }
    }
}
