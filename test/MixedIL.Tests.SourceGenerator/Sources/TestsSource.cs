﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MixedIL.Tests.SourceGenerator.Sources;

public static class TestsSource
{
    private static readonly string[] ExclusionForStandard = { "UnsafeTests" };

    internal const string RootNamespace = "MixedIL.Tests.Weaving";
    internal const string GeneratedFilesHeader = "// <auto-generated />";

    internal static (string FileName, string Code) Generate(INamedTypeSymbol type)
    {
        var className = type.Name;
        var usings = new List<string> { "Xunit", TestsSourceGenerator.AssemblyName };

        using var builder = new SourceBuilder()
            .WriteLine(GeneratedFilesHeader)
            .WriteLine()
            .WriteUsings(usings)
            .WriteLine();

        // Namespace declaration
        builder.WriteLine($"namespace {RootNamespace}")
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"public class {className} : ClassTestsBase")
            .WriteOpeningBracket();

        builder.WriteLine($"protected override string ClassName => nameof({className});");

        var methods = type.GetMembers().OfType<IMethodSymbol>();
        foreach (var method in methods.Where(m => m.DeclaredAccessibility == Accessibility.Public && m.MethodKind == MethodKind.Ordinary))
        {
            var attrs = method.GetAttributes();
            if (attrs.Any(IsXunitFact))
            {
                builder.WriteLine("[Fact]")
                    .WriteLine($"public void {method.Name}()")
                    .WriteOpeningBracket()
                    .WriteLine($"GetInstance().{method.Name}();")
                    .WriteClosingBracket();
            }
        }

        // End class declaration
        builder.WriteClosingBracket();

        if (ExclusionForStandard.Contains(className) == false)
        {
            // Standard Class declaration
            var classNameOfStandard = className + "Standard";
            builder.WriteLine($"public class {classNameOfStandard} : {className}")
                .WriteOpeningBracket()
                .WriteLine("protected override bool NetStandard => true;")
                // End class declaration
                .WriteClosingBracket();
        }

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }

    private static bool IsXunitFact(AttributeData attr)
    {
        var name = attr.AttributeClass?.Name;
        return name == "FactAttribute";
    }
}
