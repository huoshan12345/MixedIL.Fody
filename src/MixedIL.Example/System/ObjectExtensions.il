.assembly extern System.Runtime {}
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
}
