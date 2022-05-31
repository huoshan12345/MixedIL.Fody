using System;

namespace MixedIL.Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var i = 0;
            var same = ObjectHelper.AreSame(ref i, ref i);
            Console.WriteLine(same);
            Console.Read();
        }
    }
}
