using Typescript.Definitions.Tools.Attributes;

namespace Typescript.Definitions.Tools.Tests.TestModels
{
    [TsClass]
    public class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }

        [TsIgnore]
        public double Ignored { get; set; }
    }
}