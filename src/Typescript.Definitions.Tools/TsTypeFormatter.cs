using Typescript.Definitions.Tools.TsModels;

namespace Typescript.Definitions.Tools
{
    /// <summary>
    /// Defines a method used to format specific TsType to the string representation.
    /// </summary>
    /// <param name="type">The type to format.</param>
    public delegate string TsTypeFormatter(TsType type);
}