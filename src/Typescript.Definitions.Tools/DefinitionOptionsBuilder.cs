using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Typescript.Definitions.Tools.Attributes;
using Typescript.Definitions.Tools.TsModels;

namespace Typescript.Definitions.Tools
{
    public class DefinitionOptionsBuilder
    {
        internal TsModelBuilder ModelBuilder;
        internal TsGenerator Generator;

        internal DefinitionOptionsBuilder()
        {
            ModelBuilder = new TsModelBuilder();
            Generator = new TsGenerator();
        }
        internal IList<string> Generate(string projectDir)
        {
            var model = ModelBuilder.Build();
            //output the definition and the 
            var definitionCode = Generator.Generate(model, TsGeneratorOutput.Properties | TsGeneratorOutput.Fields);
            var constantCode = Generator.Generate(model, TsGeneratorOutput.Constants | TsGeneratorOutput.Enums);

            var definitionDirectory = string.IsNullOrEmpty(Generator.Options.OutDir) ? projectDir : Generator.Options.OutDir;
            var definitionFile = System.IO.Path.Combine(definitionDirectory, $"{Generator.Options.DefinitionFileName}.d.ts");
            var constFile = System.IO.Path.Combine(definitionDirectory, $"{Generator.Options.ConstFileName}.ts");

            Directory.CreateDirectory(definitionDirectory);

            File.WriteAllText(definitionFile, definitionCode, Encoding.UTF8);
            File.WriteAllText(constFile, constantCode, Encoding.UTF8);

            return new List<string> {definitionFile, constFile};
        }

        public DefinitionOptionsBuilder OutDir(string outDir)
        {
            if (!string.IsNullOrWhiteSpace(outDir))
            {
                Generator.Options.OutDir = outDir;
            }
            return this;
        }

        public DefinitionOptionsBuilder Filename(string definitionFilename = null, string constantsFilename = null)
        {
            if (!string.IsNullOrWhiteSpace(definitionFilename))
            {
                Generator.Options.DefinitionFileName = definitionFilename;
            }
            if (!string.IsNullOrWhiteSpace(constantsFilename))
            {
                Generator.Options.ConstFileName = constantsFilename;
            }
            return this;
        }

        /// <summary>
        /// Adds specific class with all referenced classes to the model.
        /// </summary>
        /// <typeparam name="TFor">The class type to add.</typeparam>
        /// <param name="options">Action to configure options for this type.</param>
        /// <returns>Instance of the <see cref="DefinitionOptionsBuilder"/> that enables fluent configuration.</returns>
        public DefinitionOptionsBuilder For<TFor>(Action<TypeOptionsBuilder> options = null)
        {
            return For(typeof(TFor), options);
        }

        /// <summary>
        /// Adds specific class with all referenced classes to the model.
        /// </summary>
        /// <param name="type">The type to add to the model.</param>
        /// <param name="options">Action to configure options for this type.</param>
        /// <returns>Instance of the <see cref="DefinitionOptionsBuilder"/> that enables fluent configuration.</returns>
        public DefinitionOptionsBuilder For(Type type, Action<TypeOptionsBuilder> options = null)
        {
            var model = ModelBuilder.Add(type);
            if (options != null)
            {
                var builder = new TypeOptionsBuilder();
                options(builder);
                if (!string.IsNullOrWhiteSpace(builder.Options.Name))
                {
                    model.Name = builder.Options.Name;
                }

                if (!string.IsNullOrWhiteSpace(builder.Options.Module))
                {
                    model.Module = new TsModule(builder.Options.Module);
                }

                if (builder.Options.IsIgnored && model is TsClass)
                {
                    ((TsClass)model).IsIgnored = builder.Options.IsIgnored;
                }
            }
            return this;
        }

        /// <summary>
        /// Adds all classes annotated with the TsClassAttribute from an assembly to the model.
        /// </summary>
        /// <param name="assembly">The assembly with classes to add.</param>
        /// <returns>Instance of the <see cref="DefinitionOptionsBuilder"/> that enables fluent configuration.</returns>
        public DefinitionOptionsBuilder For(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t =>
                    (t.GetTypeInfo().GetCustomAttribute<TsClassAttribute>(false) != null && TsType.GetTypeFamily(t) == TsTypeFamily.Class) ||
                    (t.GetTypeInfo().GetCustomAttribute<TsEnumAttribute>(false) != null && TsType.GetTypeFamily(t) == TsTypeFamily.Enum) ||
                    (t.GetTypeInfo().GetCustomAttribute<TsInterfaceAttribute>(false) != null && TsType.GetTypeFamily(t) == TsTypeFamily.Class)
            ))
            {
                For(type);
            }

            return this;
        }


        /// <summary>
        /// Registers a formatter for the specific type
        /// </summary>
        /// <typeparam name="TFor">The type to register the formatter for. TFor is restricted to TsType and derived classes.</typeparam>
        /// <param name="formatter">The formatter to register</param>
        public DefinitionOptionsBuilder WithTypeFormatter<TFor>(TsTypeFormatter formatter) where TFor : TsType
        {
            Generator.Options.TypeFormatters.RegisterTypeFormatter<TFor>(formatter);
            return this;
        }
        
        /// <summary>
        /// Registers a formatter for the the <see cref="TsClass"/> type.
        /// </summary>
        /// <param name="formatter">The formatter to register</param>
        /// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
        public DefinitionOptionsBuilder WithTypeFormatter(TsTypeFormatter formatter)
        {
            Generator.Options.TypeFormatters.RegisterTypeFormatter<TsClass>(formatter);
            return this;
        }

        /// <summary>
        /// Registers a formatter for member identifiers
        /// </summary>
        /// <param name="formatter">The formatter to register</param>
        /// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
        public DefinitionOptionsBuilder WithMemberFormatter(TsMemberIdentifierFormatter formatter)
        {
            Generator.Options.MemberFormatter = formatter;
            return this;
        }

        /// <summary>
        /// Registers a formatter for member types
        /// </summary>
        /// <param name="formatter">The formatter to register</param>
        /// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
        public DefinitionOptionsBuilder WithMemberTypeFormatter(TsMemberTypeFormatter formatter)
        {
            Generator.Options.MemberTypeFormatter = formatter;
            return this;
        }

        /// <summary>
        /// Registers a formatter for module names
        /// </summary>
        /// <param name="formatter">The formatter to register</param>
        /// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
        public DefinitionOptionsBuilder WithModuleNameFormatter(TsModuleNameFormatter formatter)
        {
            Generator.Options.ModuleNameFormatter = formatter;
            return this;
        }

        /// <summary>
        /// Registers a formatter for type visibility
        /// </summary>
        /// <param name="formatter">The formatter to register</param>
        /// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
        public DefinitionOptionsBuilder WithVisibility(TsTypeVisibilityFormatter formatter)
        {
            Generator.Options.TypeVisibilityFormatter = (formatter);
            return this;
        }

        /// <summary>
        /// Registers a converter for the specific type
        /// </summary>
        /// <typeparam name="TFor">The type to register the converter for.</typeparam>
        /// <param name="converter">The converter to register</param>
        /// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
        public DefinitionOptionsBuilder WithConverter<TFor>(TypeConverter converter)
        {
            Generator.Options.TypeConverters.RegisterTypeConverter<TFor>(converter);
            return this;
        }

        /// <summary>
        /// Registers a typescript reference file
        /// </summary>
        /// <param name="reference">Name of the d.ts typescript reference file</param>
        /// <returns></returns>
        public DefinitionOptionsBuilder WithReference(string reference)
        {
            Generator.Options.References.Add(reference);
            return this;
        }

        /// <summary>
        /// Sets a string for single indentation level in the output
        /// </summary>
        /// <param name="indentationChar">The character used for the single indentation level.</param>
        /// <returns></returns>
        public DefinitionOptionsBuilder WithIndentation(char indentationChar)
        {
            Generator.Options.IndentationChar = indentationChar;
            return this;
        }

        /// <summary>
        /// Sets format of generated enums.
        /// </summary>
        /// <param name="value">Boolean value indicating whether the enums should be generated as 'const enum'</param>
        /// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
        public DefinitionOptionsBuilder AsConstEnums(bool value = true)
        {
            Generator.Options.GenerateConstEnums = value;
            return this;
        }

    }
}