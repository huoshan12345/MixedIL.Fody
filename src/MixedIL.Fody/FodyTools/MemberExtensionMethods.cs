namespace FodyTools
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    internal static class MemberExtensionMethods
    {
        public static SequencePoint? GetEntryPoint(this MethodReference? method)
        {
            return method?.Resolve()?.GetSequencePoints()?.FirstOrDefault();
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        public static IList<SequencePoint>? GetSequencePoints(this MethodDefinition? method)
        {
            return (method?.DebugInformation?.HasSequencePoints == true)
                ? method.DebugInformation.SequencePoints
                : null;
        }
    }
}
