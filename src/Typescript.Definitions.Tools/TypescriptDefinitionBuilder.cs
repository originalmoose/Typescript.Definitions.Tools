using System;
using System.Collections.Generic;

namespace Typescript.Definitions.Tools
{
    public class TypescriptDefinitionBuilder : IDefinitionBuilder
    {
        internal List<DefinitionOptionsBuilder> Builders { get; } = new List<DefinitionOptionsBuilder>();
        public IDefinitionBuilder AddDefinition(Action<DefinitionOptionsBuilder> setup)
        {
            var builder = new DefinitionOptionsBuilder();

            setup(builder);

            Builders.Add(builder);
                
            return this;
        }
    }
}