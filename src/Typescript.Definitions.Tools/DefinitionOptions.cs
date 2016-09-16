using System;
using System.Collections.Generic;
using System.Linq;
using Typescript.Definitions.Tools.TsModels;

namespace Typescript.Definitions.Tools
{
    public class DefinitionOptions
    {
        internal Dictionary<Type, TsTypeFormatter> TypeFormatters { get; } = new Dictionary<Type, TsTypeFormatter>();

        internal Dictionary<Type, TypeConverter> TypeConverters { get; } = new Dictionary<Type, TypeConverter>();

        internal char IndentationChar { get; set; } = ' ';

        internal TsMemberIdentifierFormatter MemberFormatter { get; set; }

        internal TsMemberTypeFormatter MemberTypeFormatter { get; set; }

        internal TsModuleNameFormatter ModuleNameFormatter { get; set; }

        internal TsTypeVisibilityFormatter TypeVisibilityFormatter { get; set; }

        internal List<string> References { get; } = new List<string>();

        internal bool GenerateConstEnums { get; set; } = true;

        internal string DefinitionFileName { get; set; } = "index";

        internal string ConstFileName { get; set; } = "constants";

        public string OutDir { get; set; }

        internal DefinitionOptions()
        {
            MemberFormatter = DefaultMemberFormatter;
            MemberTypeFormatter = DefaultMemberTypeFormatter;
            ModuleNameFormatter = DefaultModuleNameFormatter;
            TypeVisibilityFormatter = DefaultTypeVisibilityFormatter;
        }

        internal bool DefaultTypeVisibilityFormatter(TsClass tsClass, string typeName)
        {
            return false;
        }

        internal string DefaultModuleNameFormatter(TsModule module)
        {
            return module.Name;
        }

        internal string DefaultMemberFormatter(TsProperty identifier)
        {
            return identifier.Name;
        }

        internal string DefaultMemberTypeFormatter(TsProperty tsProperty, string memberTypeName)
        {
            var asCollection = tsProperty.PropertyType as TsCollection;
            var isCollection = asCollection != null;

            return memberTypeName + (isCollection ? string.Concat(Enumerable.Repeat("[]", asCollection.Dimension)) : "");
        }

    }
}