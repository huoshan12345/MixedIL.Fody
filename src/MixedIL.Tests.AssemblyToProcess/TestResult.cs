// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace MixedIL.Tests.AssemblyToProcess
{
    public readonly record struct TestResult<T>(T Expected, T Actual)
    {
        public static implicit operator TestResult<T>((T, T) tuple)
        {
            return new(tuple.Item1, tuple.Item2);
        }

        public static implicit operator (T, T)(TestResult<T> result)
        {
            return (result.Expected, result.Actual);
        }
    }
}
