using System;
using System.IO;
using Fody;
using MixedIL.Fody.Extensions;
using Mono.Cecil;

namespace MixedIL.Tests.Support
{
    public static class FixtureHelper
    {
        public static string IsolateAssembly<T>()
        {
            var assembly = typeof(T).Assembly;
            var assemblyPath = assembly.Location;
            var assemblyDir = Path.GetDirectoryName(assemblyPath)!;
            var rootTestDir = Path.Combine(assemblyDir, "WeavingTest");
            var asmTestDir = Path.Combine(rootTestDir, Path.GetFileNameWithoutExtension(assemblyPath)!);

            EmptyDirectory(asmTestDir);
            Directory.CreateDirectory(asmTestDir);

            var destFile = CopyFile(assemblyPath, asmTestDir);
            CopyFile(Path.ChangeExtension(assemblyPath, ".pdb"), asmTestDir);
            CopyFile(Path.ChangeExtension(assemblyPath, ".il.dll"), asmTestDir, true);
            CopyFile(Path.ChangeExtension(assemblyPath, ".il.pdb"), asmTestDir, true);
            CopyFile(Path.Combine(assemblyDir, "MixedIL.dll"), asmTestDir);

            return destFile;
        }

        private static string CopyFile(string fileName, string targetDir, bool ignoreError = false)
        {
            if (!File.Exists(fileName))
            {
                if (ignoreError)
                {
                    return fileName;
                }
                else
                {
                    throw new InvalidOperationException($"File not found: {fileName}");
                }
            }

            var dest = Path.Combine(targetDir, Path.GetFileName(fileName)!);
            File.Copy(fileName, dest);
            return dest;
        }

        private static void EmptyDirectory(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
                return;

            foreach (var file in directoryInfo.GetFiles())
                file.Delete();

            foreach (var dir in directoryInfo.GetDirectories())
                dir.Delete(true);
        }

        public static AssemblyFixture ProcessAssembly<T>()
        {
            var assemblyPath = IsolateAssembly<T>();

            var weavingTask = new GuardedWeaver();

            var testResult = weavingTask.ExecuteTestRun(
                assemblyPath,
                ignoreCodes: new[]
                {
                    "0x801312da" // VLDTR_E_MR_VARARGCALLINGCONV
                },
                writeSymbols: true,
                beforeExecuteCallback: BeforeExecuteCallback,
                runPeVerify: false
            );

            using var assemblyResolver = new TestAssemblyResolver();

            var readerParams = new ReaderParameters(ReadingMode.Immediate)
            {
                ReadSymbols = true,
                AssemblyResolver = assemblyResolver
            };

            var originalModule = ModuleDefinition.ReadModule(assemblyPath, readerParams);
            var resultModule = ModuleDefinition.ReadModule(testResult.AssemblyPath, readerParams);

            return new(testResult, originalModule, resultModule);
        }

        internal static void BeforeExecuteCallback(ModuleDefinition module)
        {
            // This reference is added by Fody, it's not supposed to be there
            module.AssemblyReferences.RemoveWhere(i => string.Equals(i.Name, "System.Private.CoreLib", StringComparison.OrdinalIgnoreCase));
        }
    }

    public record AssemblyFixture(
        TestResult TestResult,
        ModuleDefinition OriginalModule,
        ModuleDefinition ResultModule);
}
