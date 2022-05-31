using System;
using System.Linq;
using Fody;
using MixedIL.Fody.Extensions;
using MixedIL.Fody.Support;
using Mono.Cecil;

namespace MixedIL.Fody.Processing
{
    internal sealed class MethodWeaver
    {
        private const string AnchorAttributeName = "MixedIL.MixedILAttribute";

        private readonly MethodDefinition _ilMethod;
        private readonly MethodDefinition _method;
        private readonly MethodWeaverLogger _log;

        public MethodWeaver(MethodDefinition method, MethodDefinition ilMethod, ILogger log)
        {
            _method = method;
            _ilMethod = ilMethod;
            _log = new MethodWeaverLogger(log, _method);
        }

        public static bool NeedsProcessing(MethodDefinition method)
        {
            return method.CustomAttributes.Any(m => m.AttributeType.FullName == AnchorAttributeName);
        }

        public void Process()
        {
            try
            {
                _method.CustomAttributes.RemoveWhere(m => m.AttributeType.FullName == AnchorAttributeName);
                _method.Body.Instructions.Clear();
                foreach (var instruction in _ilMethod.Body.Instructions)
                {
                    _method.Body.Instructions.Add(instruction);
                }
            }
            catch (InstructionWeavingException ex)
            {
                throw new WeavingException(_log.QualifyMessage(ex.Message, ex.Instruction))
                {
                    SequencePoint = ex.Instruction.GetInputSequencePoint(_method)
                };
            }
            catch (WeavingException ex)
            {
                throw new WeavingException(_log.QualifyMessage(ex.Message))
                {
                    SequencePoint = ex.SequencePoint
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error occured while processing method {_method.FullName}: {ex.Message}", ex);
            }
        }
    }
}
