﻿using Fody;
using MixedIL.Fody.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MixedIL.Fody.Support;
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
            foreach (var type in ModuleDefinition.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    try
                    {
                        if (!MethodWeaver.NeedsProcessing(ModuleDefinition, method))
                            continue;

                        _log.Debug($"Processing: {method.FullName}");
                        new MethodWeaver(ModuleDefinition, method, _log).Process();
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
    }
}
