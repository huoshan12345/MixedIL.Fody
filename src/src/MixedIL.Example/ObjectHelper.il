.assembly extern mscorlib {}

.class public abstract auto ansi sealed beforefieldinit System.ObjectHelper
{
    .method public hidebysig static bool AreSame<T>(!!T& a, !!T& b) cil managed aggressiveinlining
    {
        .maxstack 2
        ldarg.0
        ldarg.1
        ceq
        ret
    }
}
