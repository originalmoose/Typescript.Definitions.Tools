using System;

namespace Typescript.Definitions.Core
{
    public class TypescriptDefinitionOptionsBuilder
    {
        internal readonly TypescriptDefinitionOption Options;
       

        public TypescriptDefinitionOptionsBuilder()
        {
            Options = new TypescriptDefinitionOption();
        }

        public TypescriptDefinitionOptionsBuilder SetDefinitionFileName(string fileName)
        {
            Options.DefinitionFileName = fileName;
            return this;
        }

        public TypescriptDefinitionOptionsBuilder SetConstantsFileName(string fileName)
        {
            Options.ConstantsFileName = fileName;
            return this;
        }

        public TypescriptDefinitionOptionsBuilder AddType<T>()
        {
            Options.Types.Add(typeof(T));
            return this;
        }

        public TypescriptDefinitionOptionsBuilder AddType(Type type)
        {
            Options.Types.Add(type);
            return this;
        }
    }
}