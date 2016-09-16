using Typescript.Definitions.Tools.TsModels;

namespace Typescript.Definitions.Tools
{
    /// <summary>
    /// Formats a module name
    /// </summary>
    /// <param name="module">The module to be formatted</param>
    /// <returns>The module name after formatting.</returns>
    public delegate string TsModuleNameFormatter(TsModule module);
}