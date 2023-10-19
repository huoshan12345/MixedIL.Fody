using System.Reflection;
using MixedIL;

namespace System;

public class TestObject
{
    public TestObject(int value)
    {
        Value = value;
    }

    public int Value { get; }
}

public static class ObjectExtensions
{
    /// <summary>
    /// Set the property <see cref="AssemblyKeyNameAttribute.KeyName"/> that doesn't have setter by its BackingField.
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="keyName"></param>
    [MixedIL]
    public static extern void SetKeyName(this AssemblyKeyNameAttribute attribute, string keyName);

    /// <summary>
    /// Set the property <see cref="AssemblyKeyNameAttribute.KeyName"/> that doesn't have setter by its BackingField.
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="keyName"></param>
    [MixedIL]
    public static extern void SetKeyNameNet4(this AssemblyKeyNameAttribute attribute, string keyName);

    /// <summary>
    /// Set the property <see cref="TestObject.Value"/> that doesn't have setter by its BackingField.
    /// </summary>
    /// <param name="testObject"></param>
    /// <param name="value"></param>
    [MixedIL]
    public static extern void SetValue(this TestObject testObject, int value);
}
