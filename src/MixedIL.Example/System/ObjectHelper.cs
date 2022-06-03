using MixedIL;

// ReSharper disable once CheckNamespace
namespace System;

public static class ObjectHelper
{
    [MixedIL]
    public static extern bool AreSame<T>(ref T a, ref T b);
}
