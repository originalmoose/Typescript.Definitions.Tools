using System;
using System.Collections.Generic;
using Xunit;

namespace Typescript.Definitions.Tools.Tests
{
    public class TypeConvertorCollectionTests
    {
        [Fact]
        public void WhenConvertTypeAndNoConverterRegistered_NullIsReturned()
        {
            var target = new Dictionary<Type,TypeConverter>();

            var result = target.ConvertType(typeof(string));

            Assert.Null(result);
        }

        [Fact]
        public void WhenConvertType_ConvertedValueIsReturned()
        {
            var target = new Dictionary<Type, TypeConverter>();

            target.RegisterTypeConverter<string>(type => "KnockoutObservable<string>");

            var result = target.ConvertType(typeof(string));

            Assert.Equal("KnockoutObservable<string>", result);
        }
    }
}