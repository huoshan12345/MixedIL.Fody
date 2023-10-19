using MixedIL.Tests.AssemblyToProcess;
using MixedIL.Tests.StandardAssemblyToProcess;

namespace MixedIL.Tests.Support;

public static class AssemblyToProcessFixture
{
    public static readonly AssemblyFixture Fixture = FixtureHelper.ProcessAssembly<AssemblyToProcessReference>();
    public static TestResult TestResult => Fixture.TestResult;
    public static ModuleDefinition ResultModule => Fixture.ResultModule;
}


public static class StandardAssemblyToProcessFixture
{
    public static readonly AssemblyFixture Fixture = FixtureHelper.ProcessAssembly<StandardAssemblyToProcessReference>();
    public static TestResult TestResult => Fixture.TestResult;
    public static ModuleDefinition ResultModule => Fixture.ResultModule;
}
