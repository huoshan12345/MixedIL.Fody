using System;
using System.Collections.Generic;
using System.Text;
using MixedIL.Fody;
using Mono.Cecil.Cil;

namespace MixedIL.Tests.Support;

internal class GuardedWeaver : ModuleWeaver
{
    private readonly List<string> _errors = new();

    public override void Execute()
    {
        try
        {
            base.Execute();
        }
        catch (Exception ex)
        {
            var str = new StringBuilder();
            foreach (var error in _errors)
                str.AppendLine(error);

            str.AppendLine(ex.Message);
            throw new InvalidOperationException(str.ToString());
        }
    }

    public override void WriteError(string message, SequencePoint? sequencePoint)
    {
        _errors.Add(message);
        base.WriteError(message, sequencePoint);
    }
}
