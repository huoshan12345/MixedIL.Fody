# MixedIL.Fody

[![Build](https://github.com/huoshan12345/MixedIL.Fody/workflows/Build/badge.svg)](https://github.com/huoshan12345/MixedIL.Fody/actions?query=workflow%3ABuild)
[![NuGet package](https://img.shields.io/nuget/v/MixedIL.Fody.svg?logo=NuGet)](https://www.nuget.org/packages/MixedIL.Fody)
[![.net](https://img.shields.io/badge/.net%20standard-2.1-ff69b4.svg?)](https://www.microsoft.com/net/download)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/huoshan12345/MixedIL.Fody/blob/master/LICENSE)  
![Icon](https://github.com/huoshan12345/MixedIL.Fody/raw/master/icon.png)

This is an add-in for [Fody](https://github.com/Fody/Fody) which contains a workaround for `Interface Default Implementation Base Invocation`.  
The feature has not been implemented by C# team yet.  
The status for it can be seen in https://github.com/dotnet/csharplang/issues/2337 

---

 - [Installation](#installation)
 - [Usage](#usage)
 - [Example](#examples) 

---

## Installation
- Install the NuGet packages [`Fody`](https://www.nuget.org/packages/Fody) and [`MixedIL.Fody`](https://www.nuget.org/packages/MixedIL.Fody). Installing `Fody` explicitly is needed to enable weaving.

  ```
  PM> Install-Package Fody
  PM> Install-Package MixedIL.Fody
  ```

- Add the `PrivateAssets="all"` metadata attribute to the `<PackageReference />` items of `Fody` and `MixedIL.Fody` in your project file, so they won't be listed as dependencies.

- If you already have a `FodyWeavers.xml` file in the root directory of your project, add the `<MixedIL />` tag there. This file will be created on the first build if it doesn't exist:

  ```XML
  <?xml version="1.0" encoding="utf-8" ?>
  <Weavers>
    <MixedIL />
  </Weavers>
  ```
See [Fody usage](https://github.com/Fody/Home/blob/master/pages/usage.md) for general guidelines, and [Fody Configuration](https://github.com/Fody/Home/blob/master/pages/configuration.md) for additional options.

## Usage
Call the extension method `Base<T>` to cast an object to one of its interfaces, and then call a method or a property.  
Just like: 
- `var result = this.Base<Interface>().Method(1, "test");`
- `var value = this.Base<Interface>().Property`

## Examples
- [An example project](https://github.com/huoshan12345/MixedIL.Fody/tree/master/src/MixedIL.Example)

- Unit tests can also serve as examples of API usage. See test cases for [valid usage](https://github.com/huoshan12345/MixedIL.Fody/tree/master/src/MixedIL.Tests.AssemblyToProcess) and [invalid usage](https://github.com/huoshan12345/MixedIL.Fody/tree/master/src/MixedIL.Tests.InvalidAssemblyToProcess).

- Basic example:
    ```
    public interface IService
    {
        int Property => 5;
        void Method() => Console.WriteLine("Calling...");
    }

    public class Service : IService
    {
        // call the implementation of interface's property
        public int Property => this.Base<IService>().Property + 1;

        public void Method()
        {
            Console.WriteLine("Before call method");
            // call the implementation of interface's method
            this.Base<IService>().Method();
            Console.WriteLine("After call method");
        }
    }
    ```
