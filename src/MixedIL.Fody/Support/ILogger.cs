﻿using Mono.Cecil.Cil;

namespace MixedIL.Fody.Support
{
    internal interface ILogger
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message, SequencePoint? sequencePoint);
        void Error(string message, SequencePoint? sequencePoint);
    }
}
