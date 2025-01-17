using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using MixedIL.Tests.SourceGenerator.Sources;
using Microsoft.CodeAnalysis;

namespace MixedIL.Tests.SourceGenerator;

[Generator]
public class TestsSourceGenerator : IIncrementalGenerator
{
    public static readonly string[] Namespaces =
    [
        "MixedIL",
        "Tests",
        "AssemblyToProcess",
    ];

    public static readonly string AssemblyName = string.Join(".", Namespaces);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (Debugger.IsAttached == false)
        //    Debugger.Launch();

        context.RegisterImplementationSourceOutput(context.CompilationProvider, (ctx, provider) =>
        {
            var assemblySymbol = provider.SourceModule.ReferencedAssemblySymbols.FirstOrDefault(q => q.Name == AssemblyName);
            if (assemblySymbol == null)
                throw new InvalidOperationException("Cannot find assembly symbol: " + AssemblyName);

            var cur = Namespaces.Aggregate(assemblySymbol.GlobalNamespace, (current, ns) => current.GetNamespaceMembers().First(m => m.Name == ns));
            var types = cur.GetTypeMembers();

            foreach (var type in types)
            {
                var idx = type.Name.IndexOf("Tests", StringComparison.Ordinal);
                if (idx < 0)
                    continue;

                var (name, code) = TestsSource.Generate(type);
                ctx.AddSource(name, code);
            }
        });
    }
}
