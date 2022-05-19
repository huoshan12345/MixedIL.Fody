using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixedIL.Tests.AssemblyToProcess
{
    public partial class TestClass
    {
        public static partial bool AreSame<T>(ref T a, ref T b);
    }
}
