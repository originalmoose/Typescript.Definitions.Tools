using System;
using Typescript.Definitions.Tools.Tests.TestModels;
using Typescript.Definitions.Tools.TsModels;
using Xunit;

namespace Typescript.Definitions.Tools.Tests.TsModels
{
    public class TsEnumTests
    {

        [Fact]
        public void WhenInitializedWithNonEnumType_ArgumentExceptionIsThrown()
        {
            Assert.Throws<ArgumentException>(() => new TsEnum(typeof(Address)));
        }

        [Fact]
        public void WhenInitialized_NameIsSet()
        {
            var target = new TsEnum(typeof(ContactType));

            Assert.Equal("ContactType", target.Name);
        }
    }
}