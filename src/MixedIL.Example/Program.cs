using System;
using System.Reflection;
using MixedIL;

internal class Program
{
    private static void Main(string[] args)
    {
        var i = 0;
        var same = ObjectHelper.AreSame(ref i, ref i);
        Console.WriteLine("ObjectHelper.AreSame:" + same);


        var attr = new AssemblyKeyNameAttribute(nameof(Main));
        attr.SetKeyName(typeof(Program).Assembly.GetName().Name!);
        Console.WriteLine(attr.KeyName);

        Console.Read();
    }
}
