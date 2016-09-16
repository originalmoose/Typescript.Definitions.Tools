using System;

namespace Typescript.Definitions.Tools
{
    public interface IDefinitionBuilder
    {
        IDefinitionBuilder AddDefinition(Action<DefinitionOptionsBuilder> setup);
    }
}