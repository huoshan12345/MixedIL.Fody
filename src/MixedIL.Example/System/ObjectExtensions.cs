using System.Reflection;
using MixedIL;

namespace System;

public static class ObjectExtensions
{
    /// <summary>
    /// Set the property <see cref="AssemblyKeyNameAttribute.KeyName"/> that doesn't have setter by its BackingField.
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="keyName"></param>
    [MixedIL]
    public static extern void SetKeyName(this AssemblyKeyNameAttribute attribute, string keyName);
}
