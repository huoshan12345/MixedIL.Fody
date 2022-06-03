.assembly extern CORE_ASSEMBLY {}
.assembly extern mscorlib {}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// =============== CLASS MEMBERS DECLARATION ===================

.class public abstract auto ansi sealed beforefieldinit MixedIL.Unsafe extends [CORE_ASSEMBLY]System.Object
{
	.method public hidebysig static !!T Read<T>(void* source) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldobj !!T
        ret
	} // end of method Unsafe::Read

	.method public hidebysig static !!T ReadUnaligned<T>(void* source) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        unaligned. 0x1
        ldobj !!T
        ret
	} // end of method Unsafe::ReadUnaligned

	.method public hidebysig static !!T ReadUnaligned<T>(uint8& source) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        unaligned. 0x1
        ldobj !!T
        ret
	} // end of method Unsafe::ReadUnaligned

	.method public hidebysig static void Write<T>(void* destination, !!T 'value') cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        stobj !!T
        ret
	} // end of method Unsafe::Write

	.method public hidebysig static void WriteUnaligned<T>(void* destination, !!T 'value') cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        unaligned. 0x01
        stobj !!T
        ret
	} // end of method Unsafe::WriteUnaligned

	.method public hidebysig static void WriteUnaligned<T>(uint8& destination, !!T 'value') cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        unaligned. 0x01
        stobj !!T
        ret
	} // end of method Unsafe::WriteUnaligned

	.method public hidebysig static void Copy<T>(void* destination, !!T& source) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ldobj !!T
        stobj !!T
        ret
	} // end of method Unsafe::Copy

	.method public hidebysig static void Copy<T>(!!T& destination, void* source) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ldobj !!T
        stobj !!T
        ret
	} // end of method Unsafe::Copy

	.method public hidebysig static void* AsPointer<T>(!!T& 'value') cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        conv.u
        ret
	} // end of method Unsafe::AsPointer

	.method public hidebysig static void SkipInit<T> ([out] !!T& 'value') cil managed aggressiveinlining
	{
        .maxstack 8
        ret
	} // end of method Unsafe::SkipInit

	.method public hidebysig static int32 SizeOf<T>() cil managed aggressiveinlining
	{
        .maxstack 8
        sizeof !!T
        ret
	} // end of method Unsafe::SizeOf

	.method public hidebysig static void CopyBlock(void* destination, void* source, uint32 byteCount) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ldarg.2
        cpblk
        ret
	} // end of method Unsafe::CopyBlock

	.method public hidebysig static void CopyBlock(uint8& destination, uint8& source, uint32 byteCount) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ldarg.2
        cpblk
        ret
	} // end of method Unsafe::CopyBlock

	.method public hidebysig static void CopyBlockUnaligned(void* destination, void* source, uint32 byteCount) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        cpblk
        ret
	} // end of method Unsafe::CopyBlockUnaligned

	.method public hidebysig static void CopyBlockUnaligned(uint8& destination, uint8& source, uint32 byteCount) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        cpblk
        ret
	} // end of method Unsafe::CopyBlockUnaligned

	.method public hidebysig static void InitBlock(void* startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ldarg.2
        initblk
        ret
	} // end of method Unsafe::InitBlock

	.method public hidebysig static void InitBlock(uint8& startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ldarg.2
        initblk
        ret
	} // end of method Unsafe::InitBlock

	.method public hidebysig static void InitBlockUnaligned(void* startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        initblk
        ret
	} // end of method Unsafe::InitBlockUnaligned

	.method public hidebysig static void InitBlockUnaligned(uint8& startAddress, uint8 'value', uint32 byteCount) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ldarg.2
        unaligned. 0x1
        initblk
        ret
	} // end of method Unsafe::InitBlockUnaligned

	.method public hidebysig static !!T As<class T>(object o) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ret
	} // end of method Unsafe::As

	.method public hidebysig static !!T& AsRef<T>(void* source) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ret
	} // end of method Unsafe::AsRef

	.method public hidebysig static !!T& AsRef<T>(!!T& source) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ret
	} // end of method Unsafe::AsRef

	.method public hidebysig static !!TTo& As<TFrom, TTo>(!!TFrom& source) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ret
	} // end of method Unsafe::As

	.method public hidebysig static !!T& Unbox<valuetype .ctor ([CORE_ASSEMBLY]System.ValueType) T> (object 'box') cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        unbox !!T
        ret
	} // end of method Unsafe::Unbox

	.method public hidebysig static !!T& Add<T>(!!T& source, int32 elementOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        add
        ret
	} // end of method Unsafe::Add

	.method public hidebysig static void* Add<T>(void* source, int32 elementOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        add
        ret
	} // end of method Unsafe::Add

	.method public hidebysig static !!T& Add<T>(!!T& source, native int elementOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        sizeof !!T
        mul
        add
        ret
	} // end of method Unsafe::Add

	.method public hidebysig static !!T& Add<T>(!!T& source, native uint elementOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        sizeof !!T
        mul
        add
        ret
	} // end of method Unsafe::Add

	.method public hidebysig static !!T& AddByteOffset<T>(!!T& source, native int byteOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        add
        ret
	} // end of method Unsafe::AddByteOffset

	.method public hidebysig static !!T& AddByteOffset<T>(!!T& source, native uint byteOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        add
        ret
	} // end of method Unsafe::AddByteOffset

	.method public hidebysig static !!T& Subtract<T>(!!T& source, int32 elementOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        sub
        ret
	} // end of method Unsafe::Subtract

	.method public hidebysig static void* Subtract<T>(void* source, int32 elementOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        sizeof !!T
        conv.i
        mul
        sub
        ret
	} // end of method Unsafe::Subtract

	.method public hidebysig static !!T& Subtract<T>(!!T& source, native int elementOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        sizeof !!T
        mul
        sub
        ret
	} // end of method Unsafe::Subtract

	.method public hidebysig static !!T& Subtract<T>(!!T& source, native uint elementOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        sizeof !!T
        mul
        sub
        ret
	} // end of method Unsafe::Subtract

	.method public hidebysig static !!T& SubtractByteOffset<T>(!!T& source, native int byteOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        sub
        ret
	} // end of method Unsafe::SubtractByteOffset

	.method public hidebysig static !!T& SubtractByteOffset<T>(!!T& source, native uint byteOffset) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        sub
        ret
	} // end of method Unsafe::SubtractByteOffset

	.method public hidebysig static native int ByteOffset<T>(!!T& origin, !!T& target) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.1
        ldarg.0
        sub
        ret
	} // end of method Unsafe::ByteOffset

	.method public hidebysig static bool AreSame<T>(!!T& left, !!T& right) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        ceq
        ret
	} // end of method Unsafe::AreSame

	.method public hidebysig static bool IsAddressGreaterThan<T>(!!T& left, !!T& right) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        cgt.un
        ret
	} // end of method Unsafe::IsAddressGreaterThan

	.method public hidebysig static bool IsAddressLessThan<T>(!!T& left, !!T& right) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldarg.1
        clt.un
        ret
	} // end of method Unsafe::IsAddressLessThan

	.method public hidebysig static bool IsNullRef<T>(!!T& source) cil managed aggressiveinlining
	{
        .maxstack 8
        ldarg.0
        ldc.i4.0
        conv.u
        ceq
        ret
	} // end of method Unsafe::IsNullRef

	.method public hidebysig static !!T& NullRef<T>() cil managed aggressiveinlining
	{
        .maxstack 8
        ldc.i4.0
        conv.u
        ret
	} // end of method Unsafe::NullRef

} // end of class System.Runtime.CompilerServices.Unsafe
