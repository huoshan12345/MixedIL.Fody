using System;
using Xunit;

namespace MixedIL.Tests.AssemblyToProcess
{
    public class ObjectHelperTests
    {
        [Fact]
        public unsafe void AreSame()
        {
            var a = 4;
            var b = 4;
            var c = 5;
            var aa = &a;

            object o = a;
            object p = a;

            Assert.True(ObjectHelper.AreSame(ref a, ref a));
            Assert.True(ObjectHelper.AreSame(ref o, ref o));
            Assert.False(ObjectHelper.AreSame(ref o, ref p));
            Assert.False(ObjectHelper.AreSame(ref a, ref b));
            Assert.False(ObjectHelper.AreSame(ref a, ref c));
            Assert.True(ObjectHelper.AreSame(ref a, ref *aa));
        }
    }
}
