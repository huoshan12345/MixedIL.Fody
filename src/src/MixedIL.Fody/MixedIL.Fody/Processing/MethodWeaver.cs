using System;
using System.Linq;
using Fody;
using FodyTools;
using MixedIL.Fody.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MixedIL.Fody.Processing
{
    internal sealed class MethodWeaver
    {
        private const string AnchorAttributeName = "MixedIL.MixedILAttribute";

        private readonly MethodDefinition _ilMethod;
        private readonly MethodDefinition _method;
        private readonly CodeImporter _importer;

        public MethodWeaver(MethodDefinition method, MethodDefinition ilMethod, CodeImporter importer)
        {
            _method = method;
            _ilMethod = ilMethod;
            _importer = importer;
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
                _importer.ImportMethodBody(_ilMethod, _method);
                _importer.ProcessDeferredActions();
            }
            catch (WeavingException ex)
            {
                throw new WeavingException(QualifyMessage(ex.Message))
                {
                    SequencePoint = ex.SequencePoint
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error occured while processing method {_method.FullName}: {ex.Message}", ex);
            }
        }

        private string QualifyMessage(string message)
        {
            if (message.Contains(_method.FullName))
                return message;

            return $"{message} (in {_method.FullName})";
        }
    }
}
