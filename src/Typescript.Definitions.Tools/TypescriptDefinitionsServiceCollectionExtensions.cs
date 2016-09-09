using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Typescript.Definitions.Core
{
    public static class TypescriptDefinitionsServiceCollectionExtensions
    {
        public static IServiceCollection CreateDefinition(this IServiceCollection services, Action<TypescriptDefinitionOptionsBuilder> config)
        {
            var defBuilder = services.BuildServiceProvider().GetService<TypescripDefinitionBuilder>();
            var builder = new TypescriptDefinitionOptionsBuilder();

            config(builder);

            defBuilder.Options.Add(builder.Options);

            return services;
        }
    }
}