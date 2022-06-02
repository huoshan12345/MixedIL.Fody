// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace MixedIL;

/// <summary>
/// Contains generic, low-level functionality for manipulating pointers.
/// </summary>
public static unsafe class Unsafe
{
    /// <summary>
    /// Returns a pointer to the given by-ref parameter.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void* AsPointer<T>(ref T value);

    /// <summary>
    /// Returns the size of an object of the given type parameter.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern int SizeOf<T>();

    /// <summary>
    /// Casts the given object to the specified type, performs no dynamic type checking.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("o")]
    public static extern T As<T>(object? o) where T : class?;

    /// <summary>
    /// Reinterprets the given reference as a reference to a value of type <typeparamref name="TTo"/>.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref TTo As<TFrom, TTo>(ref TFrom source);

    /// <summary>
    /// Adds an element offset to the given reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T Add<T>(ref T source, int elementOffset);

    /// <summary>
    /// Adds an element offset to the given reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T Add<T>(ref T source, IntPtr elementOffset);

    /// <summary>
    /// Adds an element offset to the given pointer.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void* Add<T>(void* source, int elementOffset);

    /// <summary>
    /// Adds an element offset to the given reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T Add<T>(ref T source, nuint elementOffset);

    /// <summary>
    /// Adds an byte offset to the given reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T AddByteOffset<T>(ref T source, nuint byteOffset);

    /// <summary>
    /// Determines whether the specified references point to the same location.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern bool AreSame<T>([AllowNull] ref T left, [AllowNull] ref T right);

    /// <summary>
    /// Copies a value of type T to the given location.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void Copy<T>(void* destination, ref T source);

    /// <summary>
    /// Copies a value of type T to the given location.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void Copy<T>(ref T destination, void* source);

    /// <summary>
    /// Copies bytes from the source address to the destination address.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void CopyBlock(void* destination, void* source, uint byteCount);

    /// <summary>
    /// Copies bytes from the source address to the destination address.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void CopyBlock(ref byte destination, ref byte source, uint byteCount);

    /// <summary>
    /// Copies bytes from the source address to the destination address without assuming architecture dependent alignment of the addresses.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void CopyBlockUnaligned(void* destination, void* source, uint byteCount);

    /// <summary>
    /// Copies bytes from the source address to the destination address without assuming architecture dependent alignment of the addresses.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void CopyBlockUnaligned(ref byte destination, ref byte source, uint byteCount);

    /// <summary>
    /// Determines whether the memory address referenced by <paramref name="left"/> is greater than
    /// the memory address referenced by <paramref name="right"/>.
    /// </summary>
    /// <remarks>
    /// This check is conceptually similar to "(void*)(&amp;left) &gt; (void*)(&amp;right)".
    /// </remarks>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern bool IsAddressGreaterThan<T>([AllowNull] ref T left, [AllowNull] ref T right);

    /// <summary>
    /// Determines whether the memory address referenced by <paramref name="left"/> is less than
    /// the memory address referenced by <paramref name="right"/>.
    /// </summary>
    /// <remarks>
    /// This check is conceptually similar to "(void*)(&amp;left) &lt; (void*)(&amp;right)".
    /// </remarks>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern bool IsAddressLessThan<T>([AllowNull] ref T left, [AllowNull] ref T right);

    /// <summary>
    /// Initializes a block of memory at the given location with a given initial value.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void InitBlock(void* startAddress, byte value, uint byteCount);

    /// <summary>
    /// Initializes a block of memory at the given location with a given initial value.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void InitBlock(ref byte startAddress, byte value, uint byteCount);

    /// <summary>
    /// Initializes a block of memory at the given location with a given initial value
    /// without assuming architecture dependent alignment of the address.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void InitBlockUnaligned(void* startAddress, byte value, uint byteCount);

    /// <summary>
    /// Initializes a block of memory at the given location with a given initial value
    /// without assuming architecture dependent alignment of the address.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount);

    /// <summary>
    /// Reads a value of type <typeparamref name="T"/> from the given location.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern T ReadUnaligned<T>(void* source);

    /// <summary>
    /// Reads a value of type <typeparamref name="T"/> from the given location.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern T ReadUnaligned<T>(ref byte source);

    /// <summary>
    /// Writes a value of type <typeparamref name="T"/> to the given location.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void WriteUnaligned<T>(void* destination, T value);

    /// <summary>
    /// Writes a value of type <typeparamref name="T"/> to the given location.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void WriteUnaligned<T>(ref byte destination, T value);

    /// <summary>
    /// Adds an byte offset to the given reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T AddByteOffset<T>(ref T source, IntPtr byteOffset);

    /// <summary>
    /// Reads a value of type <typeparamref name="T"/> from the given location.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern T Read<T>(void* source);

    /// <summary>
    /// Writes a value of type <typeparamref name="T"/> to the given location.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void Write<T>(void* destination, T value);

    /// <summary>
    /// Reinterprets the given location as a reference to a value of type <typeparamref name="T"/>.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T AsRef<T>(void* source);

    /// <summary>
    /// Reinterprets the given location as a reference to a value of type <typeparamref name="T"/>.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T AsRef<T>(in T source);

    /// <summary>
    /// Determines the byte offset from origin to target from the given references.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern IntPtr ByteOffset<T>([AllowNull] ref T origin, [AllowNull] ref T target);

    /// <summary>
    /// Returns a by-ref to type <typeparamref name="T"/> that is a null reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T NullRef<T>();

    /// <summary>
    /// Returns if a given by-ref to type <typeparamref name="T"/> is a null reference.
    /// </summary>
    /// <remarks>
    /// This check is conceptually similar to "(void*)(&amp;source) == nullptr".
    /// </remarks>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern bool IsNullRef<T>(ref T source);

    /// <summary>
    /// Bypasses definite assignment rules by taking advantage of <c>out</c> semantics.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void SkipInit<T>(out T value);

    /// <summary>
    /// Subtracts an element offset from the given reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T Subtract<T>(ref T source, int elementOffset);

    /// <summary>
    /// Subtracts an element offset from the given void pointer.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern void* Subtract<T>(void* source, int elementOffset);

    /// <summary>
    /// Subtracts an element offset from the given reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T Subtract<T>(ref T source, IntPtr elementOffset);

    /// <summary>
    /// Subtracts an element offset from the given reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T Subtract<T>(ref T source, nuint elementOffset);

    /// <summary>
    /// Subtracts a byte offset from the given reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T SubtractByteOffset<T>(ref T source, IntPtr byteOffset);

    /// <summary>
    /// Subtracts a byte offset from the given reference.
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T SubtractByteOffset<T>(ref T source, nuint byteOffset);

    /// <summary>
    /// Returns a mutable ref to a boxed value
    /// </summary>
    [MixedIL, Intrinsic, NonVersionable]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static extern ref T Unbox<T>(object box);

}
