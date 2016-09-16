using Typescript.Definitions.Tools.TsModels;

namespace Typescript.Definitions.Tools
{
    /// <summary>
    /// Defines a method used to format class member identifiers.
    /// </summary>
    /// <param name="identifier">The identifier to format</param>
    /// <returns>The formatted identifier.</returns>
    public delegate string TsMemberIdentifierFormatter(TsProperty identifier);
}