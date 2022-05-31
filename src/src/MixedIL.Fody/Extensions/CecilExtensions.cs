using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MixedIL.Fody.Extensions
{
    internal static class CecilExtensions
    {
        public static SequencePoint? GetInputSequencePoint(this Instruction? instruction, MethodDefinition method)
        {
            if (instruction == null)
                return null;

            var sequencePoints = method.DebugInformation.HasSequencePoints
                ? method.DebugInformation.SequencePoints
                : Enumerable.Empty<SequencePoint>();

            return sequencePoints.LastOrDefault(sp => sp.Offset <= instruction.Offset);
        }
    }
}
