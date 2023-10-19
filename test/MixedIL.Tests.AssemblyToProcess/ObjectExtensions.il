.assembly extern MixedIL.Tests.AssemblyToProcess {}

.class public abstract sealed auto ansi beforefieldinit System.ObjectExtensions
{
    .method public hidebysig static void SetKeyName(class [System.Runtime]System.Reflection.AssemblyKeyNameAttribute attribute, string keyName) cil managed
    {
        .maxstack 8
        ldarg.0
        ldarg.1
        stfld string [System.Runtime]System.Reflection.AssemblyKeyNameAttribute::'<KeyName>k__BackingField'
        ret
    }

    .method public hidebysig static void SetValue(class [MixedIL.Tests.AssemblyToProcess]System.TestObject testObject, int32 'value') cil managed
    {
        .maxstack 8
        ldarg.0
        ldarg.1
        stfld int32 [MixedIL.Tests.AssemblyToProcess]System.TestObject::'<Value>k__BackingField'
        ret
    }
}
