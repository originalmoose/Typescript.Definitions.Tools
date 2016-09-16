using Typescript.Definitions.Tools.TsModels;

namespace Typescript.Definitions.Tools
{
    /// <summary>
    /// Defines a method used to format class member types.
    /// </summary>
    /// <param name="tsProperty">The typescript property</param>
    /// <returns>The formatted type.</returns>
    public delegate string TsMemberTypeFormatter(TsProperty tsProperty, string memberTypeName);
}