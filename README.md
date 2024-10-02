# MixedIL.Fody

[![Build](https://github.com/huoshan12345/MixedIL.Fody/workflows/Build/badge.svg)](https://github.com/huoshan12345/MixedIL.Fody/actions?query=workflow%3ABuild)
[![NuGet package](https://img.shields.io/nuget/v/MixedIL.Fody.svg?logo=NuGet)](https://www.nuget.org/packages/MixedIL.Fody)
[![.net](https://img.shields.io/badge/.net%20standard-2.0-ff69b4.svg?)](https://www.microsoft.com/net/download)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/huoshan12345/MixedIL.Fody/blob/master/LICENSE)  
![Icon](https://raw.githubusercontent.com/huoshan12345/MixedIL.Fody/master/icon.png)

## This is an add-in for [Fody](https://github.com/Fody/Fody) which allows you to impliment C# method by IL code.

- [Installation](#installation)
- [Usage](#usage)
- [Example](#examples)
- [Comparison with InlineIL.Fody](#comparison-with-inlineilfody)

---

## Installation
- Include the [`Fody`](https://www.nuget.org/packages/Fody) and [`MixedIL.Fody`](https://www.nuget.org/packages/MixedIL.Fody) NuGet packages with a `PrivateAssets="all"` attribute on their `<PackageReference />` items. Installing `Fody` explicitly is needed to enable weaving.

  ```XML
  <PackageReference Include="Fody" Version="..." PrivateAssets="all" />
  <PackageReference Include="MixedIL.Fody" Version="..." PrivateAssets="all" />
  ```


- If you already have a `FodyWeavers.xml` file in the root directory of your project, add the `<MixedIL />` tag there. This file will be created on the first build if it doesn't exist:

  ```XML
  <?xml version="1.0" encoding="utf-8" ?>
  <Weavers>
    <MixedIL />
  </Weavers>
  ```

  See [Fody usage](https://github.com/Fody/Home/blob/master/pages/usage.md) for general guidelines, and [Fody Configuration](https://github.com/Fody/Home/blob/master/pages/configuration.md) for additional options.

---

## Usage

1. Write a method stub in a C# project with keyword `extern` and an attribute provided in `MixedIL` called `MixedILAttribute`

```
using MixedIL;

namespace System;

public class ObjectHelper
{
    [MixedIL]
    public static extern bool AreSame<T>(ref T a, ref T b);
}
```

2. Then create a .il file in the the C# project and implement the method in il code. C# dll

```
.class public abstract auto ansi sealed beforefieldinit System.ObjectHelper
{
    .method public hidebysig static bool AreSame<T>(!!T& a, !!T& b) cil managed aggressiveinlining
    {
        .maxstack 2
        ldarg.0
        ldarg.1
        ceq
        ret
    }
}
```

3. Compile the project and that's it.

#### NOTE:

- The method signature and declaring type should be the same.
- Only il method body will be injected into the assembly file, other parts like attributes in il code won't. You can just write all the other parts using C# code.

---

## Options

Options that can be configured through some MSBuild properties to set in consuming projects.

- `MixedILExcludeIgnoresAccessChecksToAttribute`: Exclude **IgnoresAccessChecksToAttribute** from source files to comiple.

#### NOTE:

- **IgnoresAccessChecksToAttribute** is used to access non-public types and members across assemblies through IL code. It is helpful when you are using `MixedIL` or `System.Reflection.Emit`.

---

## Examples

- [An example project](https://github.com/huoshan12345/MixedIL.Fody/tree/master/src/MixedIL.Example)
- A reimplementation of the `System.Runtime.CompilerServices.Unsafe` called [MixedIL.Unsafe](https://github.com/huoshan12345/MixedIL.Fody/tree/master/src/MixedIL.Unsafe) is provided as an example.

---

## Comparison with InlineIL.Fody

[InlineIL.Fody](https://github.com/ltrzesniewski/InlineIL.Fody) is a remarkable fody add-in which lets you inject IL instructions by calling normal static C# methods. While there are some differences, so you can choose based on your requirement.

- Basically, in most cases you can use `InlineIL`, which is more friendly to C# developers.
- `MixedIL` is more flexible and less restrictive, since pure IL code will be written.  
  For instance, you can invoke backing field of a property without setter by pure IL, like this:  
  `stfld string [System.Runtime]System.Reflection.AssemblyKeyNameAttribute::'<KeyName>k__BackingField'`  
  see [example code](https://github.com/huoshan12345/MixedIL.Fody/blob/master/test/MixedIL.Tests.AssemblyToProcess/ObjectExtensions.il#L11).  
  However, up to now, you will get compile error if you would like to implement it using `InlineIL`.
