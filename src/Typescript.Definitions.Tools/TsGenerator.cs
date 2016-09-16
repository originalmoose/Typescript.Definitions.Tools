using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Typescript.Definitions.Tools.TsModels;

namespace Typescript.Definitions.Tools
{
    internal class TsGenerator
    {
        internal DefinitionOptions Options { get; set; }

        public IDocAppender DocAppender = new NullDocAppender();
        public HashSet<TsClass> GeneratedClasses = new HashSet<TsClass>();
        public HashSet<TsEnum> GeneratedEnums = new HashSet<TsEnum>();

        internal TsGenerator()
        {
            Options = new DefinitionOptions();

            Options.TypeFormatters.RegisterTypeFormatter<TsClass>((type) => {
                var tsClass = ((TsClass)type);
                if (!tsClass.GenericArguments.Any()) return tsClass.Name;
                return tsClass.Name + "<" + string.Join(", ", tsClass.GenericArguments.Select(a => a as TsCollection != null ? this.GetFullyQualifiedTypeName(a) + "[]" : this.GetFullyQualifiedTypeName(a))) + ">";
            });
            Options.TypeFormatters.RegisterTypeFormatter<TsSystemType>((type) => ((TsSystemType)type).Kind.ToTypeScriptString());
            Options.TypeFormatters.RegisterTypeFormatter<TsCollection>((type) => {
                var itemType = ((TsCollection)type).ItemsType;
                var itemTypeAsClass = itemType as TsClass;
                if (itemTypeAsClass == null || !itemTypeAsClass.GenericArguments.Any()) return this.GetTypeName(itemType);
                return this.GetTypeName(itemType);
            });
            Options.TypeFormatters.RegisterTypeFormatter<TsEnum>((type) => ((TsEnum)type).Name);

        }

        /// <summary>
        /// Generates TypeScript definitions for properties and enums in the model.
        /// </summary>
        /// <param name="model">The code model with classes to generate definitions for.</param>
        /// <returns>TypeScript definitions for classes in the model.</returns>
        public string Generate(TsModel model)
        {
            return Generate(model, TsGeneratorOutput.Properties | TsGeneratorOutput.Enums);
        }

        /// <summary>
        /// Generates TypeScript definitions for classes and/or enums in the model.
        /// </summary>
        /// <param name="model">The code model with classes to generate definitions for.</param>
        /// <param name="generatorOutput">The type of definitions to generate</param>
        /// <returns>TypeScript definitions for classes and/or enums in the model..</returns>
        public string Generate(TsModel model, TsGeneratorOutput generatorOutput)
        {
            var sb = new IndentedStringBuilder();

            if ((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties
                || (generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields)
            {

                if ((generatorOutput & TsGeneratorOutput.Constants) == TsGeneratorOutput.Constants)
                {
                    // We can't generate constants together with properties or fields, because we can't set values in a .d.ts file.
                    throw new InvalidOperationException("Cannot generate constants together with properties or fields");
                }

                foreach (var reference in Options.References.Concat(model.References))
                {
                    AppendReference(reference, sb);
                }
                sb.AppendLine();
            }

            // We can't just sort by the module name, because a formatter can jump in and change it so
            // format by the desired target name
            foreach (var module in model.Modules.OrderBy(GetModuleName))
            {
                AppendModule(module, sb, generatorOutput);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets name of the type in the TypeScript
        /// </summary>
        /// <param name="type">The type to get name of</param>
        /// <returns>name of the type</returns>
        public string GetTypeName(TsType type)
        {
            return Options.TypeConverters.IsConverterRegistered(type.Type) ?
                Options.TypeConverters.ConvertType(type.Type) :
                Options.TypeFormatters.FormatType(type);
        }

        /// <summary>
        /// Gets property name in the TypeScript
        /// </summary>
        /// <param name="property">The property to get name of</param>
        /// <returns>name of the property</returns>
        public string GetPropertyName(TsProperty property)
        {
            var name = Options.MemberFormatter(property);
            if (property.IsOptional)
            {
                name += "?";
            }

            return name;
        }

        /// <summary>
        /// Gets whether a type should be marked with "Export" keyword in TypeScript
        /// </summary>
        /// <param name="tsClass"></param>
        /// <param name="typeName">The type to get the visibility of</param>
        /// <returns>bool indicating if type should be marked weith keyword "Export"</returns>
        public bool GetTypeVisibility(TsClass tsClass, string typeName)
        {
            return Options.TypeVisibilityFormatter(tsClass, typeName);
        }

        /// <summary>
        /// Formats a module name
        /// </summary>
        /// <param name="module">The module to be formatted</param>
        /// <returns>The module name after formatting.</returns>
        public string GetModuleName(TsModule module)
        {
            return Options.ModuleNameFormatter(module);
        }


        /// <summary>
        /// Gets fully qualified name of the type
        /// </summary>
        /// <param name="type">The type to get name of</param>
        /// <returns>Fully qualified name of the type</returns>
        public string GetFullyQualifiedTypeName(TsType type)
        {
            var moduleName = string.Empty;

            var member = type as TsModuleMember;
            if (member != null && !Options.TypeConverters.IsConverterRegistered(member.Type))
            {
                var memberType = member;
                moduleName = memberType.Module != null ? GetModuleName(memberType.Module) : string.Empty;
            }
            else if (type is TsCollection)
            {
                var collectionType = (TsCollection)type;
                moduleName = GetCollectionModuleName(collectionType, moduleName);
            }

            if (type.Type.IsGenericParameter)
            {
                return GetTypeName(type);
            }

            if (string.IsNullOrEmpty(moduleName))
                return GetTypeName(type);

            var name = moduleName + "." + GetTypeName(type);
            return name;
        }

        /// <summary>
        /// Recursively finds the module name for the underlaying ItemsType of a TsCollection.
        /// </summary>
        /// <param name="collectionType">The TsCollection object.</param>
        /// <param name="moduleName">The module name.</param>
        /// <returns></returns>
        public string GetCollectionModuleName(TsCollection collectionType, string moduleName)
        {
            var type = collectionType.ItemsType as TsModuleMember;
            if (type != null && !Options.TypeConverters.IsConverterRegistered(type.Type))
            {
                if (!type.Type.IsGenericParameter)
                    moduleName = type.Module != null ? GetModuleName(type.Module) : string.Empty;
            }

            var collection = collectionType.ItemsType as TsCollection;
            if (collection != null)
            {
                moduleName = GetCollectionModuleName(collection, moduleName);
            }
            return moduleName;
        }


        /// <summary>
        /// Gets property type in the TypeScript
        /// </summary>
        /// <param name="property">The property to get type of</param>
        /// <returns>type of the property</returns>
        public string GetPropertyType(TsProperty property)
        {
            var fullyQualifiedTypeName = GetFullyQualifiedTypeName(property.PropertyType);
            return Options.MemberTypeFormatter(property, fullyQualifiedTypeName);
        }


        /// <summary>
        /// Gets property constant value in TypeScript format
        /// </summary>
        /// <param name="property">The property to get constant value of</param>
        /// <returns>constant value of the property</returns>
        public string GetPropertyConstantValue(TsProperty property)
        {
            var quote = property.PropertyType.Type == typeof(string) ? "\"" : "";
            return quote + property.ConstantValue + quote;
        }


        /// <summary>
        /// Generates reference to other d.ts file and appends it to the output.
        /// </summary>
        /// <param name="reference">The reference file to generate reference for.</param>
        /// <param name="sb">The output</param>
        protected virtual void AppendReference(string reference, IndentedStringBuilder sb)
        {
            sb.Append($"/// <reference path=\"{reference}\" />");
            sb.AppendLine();
        }

        protected virtual void AppendModule(TsModule module, IndentedStringBuilder sb, TsGeneratorOutput generatorOutput)
        {
            var classes = module.Classes.Where(c => !Options.TypeConverters.IsConverterRegistered(c.Type) && !c.IsIgnored).OrderBy(GetTypeName).ToList();
            var enums = module.Enums.Where(e => !Options.TypeConverters.IsConverterRegistered(e.Type) && !e.IsIgnored).OrderBy(GetTypeName).ToList();
            if ((generatorOutput == TsGeneratorOutput.Enums && enums.Count == 0) ||
                (generatorOutput == TsGeneratorOutput.Properties && classes.Count == 0) ||
                (enums.Count == 0 && classes.Count == 0))
            {
                return;
            }

            var moduleName = GetModuleName(module);
            var generateModuleHeader = moduleName != string.Empty;

            if (generateModuleHeader)
            {
                if (generatorOutput != TsGeneratorOutput.Enums &&
                    (generatorOutput & TsGeneratorOutput.Constants) != TsGeneratorOutput.Constants)
                {
                    sb.Append("declare ");
                }

                sb.AppendLine(string.Format("module {0} {{", moduleName));
            }

            using (sb.Indent())
            {
                if ((generatorOutput & TsGeneratorOutput.Enums) == TsGeneratorOutput.Enums)
                {
                    foreach (var enumModel in enums)
                    {
                        AppendEnumDefinition(enumModel, sb, generatorOutput);
                    }
                }

                if (((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties)
                    || (generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields)
                {
                    foreach (var classModel in classes)
                    {

                        AppendClassDefinition(classModel, sb, generatorOutput);
                    }
                }

                if ((generatorOutput & TsGeneratorOutput.Constants) == TsGeneratorOutput.Constants)
                {
                    foreach (var classModel in classes)
                    {
                        if (classModel.IsIgnored)
                        {
                            continue;
                        }

                        AppendConstantModule(classModel, sb);
                    }
                }
            }
            if (generateModuleHeader)
            {
                sb.AppendLine("}");
            }
        }

        /// <summary>
        /// Generates class definition and appends it to the output.
        /// </summary>
        /// <param name="classModel">The class to generate definition for.</param>
        /// <param name="sb">The output.</param>
        /// <param name="generatorOutput"></param>
        protected virtual void AppendClassDefinition(TsClass classModel, IndentedStringBuilder sb, TsGeneratorOutput generatorOutput)
        {
            var typeName = GetTypeName(classModel);
            var visibility = GetTypeVisibility(classModel, typeName) ? "export " : "";
            DocAppender.AppendClassDoc(sb, classModel, typeName);
            sb.Append(string.Format("{0}interface {1}", visibility, typeName));
            if (classModel.BaseType != null)
            {
                sb.Append(string.Format(" extends {0}", GetFullyQualifiedTypeName(classModel.BaseType)));
            }

            if (classModel.Interfaces.Count > 0)
            {
                var implementations = classModel.Interfaces.Select(GetFullyQualifiedTypeName).ToArray();

                var prefixFormat = classModel.Type.GetTypeInfo().IsInterface ? " extends {0}"
                    : classModel.BaseType != null ? " , {0}"
                        : " extends {0}";

                sb.Append(string.Format(prefixFormat, string.Join(" ,", implementations)));
            }

            sb.AppendLine(" {");

            var members = new List<TsProperty>();
            if ((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties)
            {
                members.AddRange(classModel.Properties);
            }
            if ((generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields)
            {
                members.AddRange(classModel.Fields);
            }
            using (sb.Indent())
            {
                foreach (var property in members.Where(p => !p.IsIgnored).OrderBy(GetPropertyName))
                {
                    DocAppender.AppendPropertyDoc(sb, property, GetPropertyName(property), GetPropertyType(property));
                    sb.AppendLine(string.Format("{0}: {1};", GetPropertyName(property), GetPropertyType(property)));
                }
            }

            sb.AppendLine("}");

            GeneratedClasses.Add(classModel);
        }

        protected virtual void AppendEnumDefinition(TsEnum enumModel, IndentedStringBuilder sb, TsGeneratorOutput output)
        {
            var typeName = GetTypeName(enumModel);
            var visibility = (output & TsGeneratorOutput.Enums) == TsGeneratorOutput.Enums || (output & TsGeneratorOutput.Constants) == TsGeneratorOutput.Constants ? "export " : "";

            DocAppender.AppendEnumDoc(sb, enumModel, typeName);

            var constSpecifier = Options.GenerateConstEnums ? "const " : string.Empty;
            sb.AppendLine(string.Format("{0}{2}enum {1} {{", visibility, typeName, constSpecifier));

            using (sb.Indent())
            {
                var i = 1;
                foreach (var v in enumModel.Values)
                {
                    DocAppender.AppendEnumValueDoc(sb, v);
                    sb.AppendLine(string.Format(i < enumModel.Values.Count ? "{0} = {1}," : "{0} = {1}", v.Name, v.Value));
                    i++;
                }
            }

            sb.AppendLine("}");

            GeneratedEnums.Add(enumModel);
        }

        /// <summary>
        /// Generates class definition and appends it to the output.
        /// </summary>
        /// <param name="classModel">The class to generate definition for.</param>
        /// <param name="sb">The output.</param>
        protected virtual void AppendConstantModule(TsClass classModel, IndentedStringBuilder sb)
        {
            if (!classModel.Constants.Any())
            {
                return;
            }

            var typeName = GetTypeName(classModel);
            sb.AppendLine(string.Format("export module {0} {{", typeName));

            using (sb.Indent())
            {
                foreach (var property in classModel.Constants)
                {
                    if (property.IsIgnored)
                    {
                        continue;
                    }

                    DocAppender.AppendConstantModuleDoc(sb, property, GetPropertyName(property), GetPropertyType(property));
                    sb.Append(string.Format("export var {0}: {1} = {2};", GetPropertyName(property), GetPropertyType(property), GetPropertyConstantValue(property)));
                    sb.AppendLine();
                }

            }
            sb.AppendLine("}");

            GeneratedClasses.Add(classModel);
        }

    }
}
