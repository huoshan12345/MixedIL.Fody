// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

// Calls to methods or references to fields marked with this attribute may be replaced at
// some call sites with jit intrinsic expansions.
// Types marked with this attribute may be specially treated by the runtime/compiler.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field, Inherited = false)]
internal sealed class IntrinsicAttribute : Attribute
{
}
