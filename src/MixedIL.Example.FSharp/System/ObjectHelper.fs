namespace System

open MixedIL

type ObjectHelper =
    [<MixedIL>]
    static member AreSame<'T>(a: byref<'T>, b: byref<'T>) : bool = raise (NotImplementedException())
