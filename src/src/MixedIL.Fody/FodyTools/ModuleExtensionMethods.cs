﻿namespace FodyTools
{
    using System;
    using System.Linq;
    using System.Runtime.Versioning;

    using Mono.Cecil;

    internal static class ModuleExtensionMethods
    {
        public static FrameworkName? GetTargetFrameworkName(this ModuleDefinition moduleDefinition)
        {
            return moduleDefinition.Assembly
                .CustomAttributes
                .Where(attr => attr.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName)
                .Select(attr => attr.ConstructorArguments.Select(arg => arg.Value as string).FirstOrDefault())
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => new FrameworkName(name))
                .FirstOrDefault();
        }

        public static FrameworkName? GetTargetFrameworkName(this Type typeInTargetAssembly)
        {
            return typeInTargetAssembly.Assembly
                .CustomAttributes
                .Where(attr => attr.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName)
                .Select(attr => attr.ConstructorArguments.Select(arg => arg.Value as string).FirstOrDefault())
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => new FrameworkName(name))
                .FirstOrDefault();
        }
    }
}
