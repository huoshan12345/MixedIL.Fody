namespace System

open MixedIL
open System.Runtime.CompilerServices
open System.Reflection

[<Extension>]
type ObjectExtensions =
    [<MixedIL>]
    [<Extension>]
    static member SetKeyName(attribute: AssemblyKeyNameAttribute, keyName: string) : unit =
        raise (NotImplementedException())
