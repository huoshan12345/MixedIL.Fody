using System.Reflection;
using Xunit;

namespace MixedIL.Tests.AssemblyToProcess;

public class ObjectExtensionsTests
{
    [Fact]
    public void PropertyWithoutSetter_InReferencedAssembly_SetKeyName()
    {
        var attr = new AssemblyKeyNameAttribute("");
        const string name = nameof(AssemblyKeyNameAttribute);
        attr.SetKeyName(name);
        Assert.Equal(name, attr.KeyName);
    }

    [Fact]
    public void PropertyWithoutSetter_InSelfAssembly_SetValue()
    {
        var obj = new TestObject(0);
        obj.SetValue(2);
        Assert.Equal(2, obj.Value);
    }
}
