using Xunit;

namespace MixedIL.Tests.Weaving
{
    public class MissingTypeTests : ClassTestsBase
    {
        protected override string ClassName => "System.ObjectHelper";

        [Fact]
        public void MissingType_Test()
        {
            CannotFindType();
        }
    }
}
