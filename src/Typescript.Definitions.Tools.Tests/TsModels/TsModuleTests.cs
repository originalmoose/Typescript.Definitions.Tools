using Typescript.Definitions.Tools.TsModels;
using Xunit;

namespace Typescript.Definitions.Tools.Tests.TsModels
{
    public class TsModuleTests
    {

        [Fact]
        public void WhenInitialized_ClassesCollectionIsEmpty()
        {
            var target = new TsModule("Tests");

            Assert.NotNull(target.Classes);
            Assert.Empty(target.Classes);
        }

        [Fact]
        public void WhenInitialized_NameIsSet()
        {
            var target = new TsModule("Tests");

            Assert.Equal("Tests", target.Name);
        }
    }
}