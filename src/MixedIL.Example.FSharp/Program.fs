open System
open System.Reflection
open System.Runtime.CompilerServices

[<assembly: IgnoresAccessChecksTo("System.Private.CoreLib")>]
do()

module Program = 
    [<EntryPoint>]
    let main args =
        let mutable i = 0;
        let same = ObjectHelper.AreSame(&i, &i);
        printfn "ObjectHelper.AreSame: %b" same;

        let attr = new AssemblyKeyNameAttribute(nameof(args))
        attr.SetKeyName("Program")
        Console.WriteLine attr.KeyName

        0
