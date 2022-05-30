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
                _log.Info("There is no il dll.");
                return;
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
                            throw new InvalidOperationException($"Cannot find type in {iLFile.Name} by name {type.FullName}");
                        }
                        if (!methods.TryGetValue(method.FullName, out var iLMethod))
                        {
                            throw new InvalidOperationException($"Cannot find method of type {type.FullName} in {iLFile.Name} by name {method.FullName}");
                        }

                        _log.Debug($"Processing: {method.FullName}");
                        new MethodWeaver(method, iLMethod, _log).Process();
                    }
                    catch (WeavingException ex)
                    {
                        AddError(ex.Message, ex.SequencePoint);
                    }
                }
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning() => Enumerable.Empty<string>();

        protected virtual void AddError(string message, SequencePoint? sequencePoint)
            => _log.Error(message, sequencePoint);

        private FileInfo GetILFile()
        {
            var fileInfo = new FileInfo(ModuleDefinition.FileName);
            var (name, _) = PathHelper.ExtractFileName(fileInfo.Name);
            var il = name + ".il.dll";
            return new FileInfo(Path.Combine(fileInfo.DirectoryName!, il));
        }
    }
}
