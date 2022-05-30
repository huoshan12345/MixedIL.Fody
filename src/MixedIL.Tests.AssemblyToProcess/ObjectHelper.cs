namespace MixedIL.Tests.AssemblyToProcess
{
    public class ObjectHelper
    {
        [MixedIL]
        public static extern bool AreSame<T>(ref T a, ref T b);
    }
}
