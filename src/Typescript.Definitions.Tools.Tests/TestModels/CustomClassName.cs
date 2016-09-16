using Typescript.Definitions.Tools.Attributes;

namespace Typescript.Definitions.Tools.Tests.TestModels
{
    [TsClass(Name = "MyClass", Module = "MyModule")]
    public class CustomClassName
    {
        [TsProperty(Name = "MyProperty")]
        public int CustomPorperty { get; set; }
    }
}