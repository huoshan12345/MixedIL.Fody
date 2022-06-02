using System;
using MixedIL;

internal class Program
{
    private static void Main(string[] args)
    {
        var i = 0;
        var same = ObjectHelper.AreSame(ref i, ref i);
        Console.WriteLine("ObjectHelper.AreSame:" + same);

        var size = Unsafe.SizeOf<int>();
        Console.WriteLine("Unsafe.SizeOf<int>:" + size);

        Console.Read();
    }
}
