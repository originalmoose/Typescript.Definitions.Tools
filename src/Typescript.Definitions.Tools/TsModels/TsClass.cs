using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Typescript.Definitions.Tools.Attributes;

namespace Typescript.Definitions.Tools.TsModels
{
    /// <summary>
    /// Represents a class in the code model.
    /// </summary>
    [DebuggerDisplay("TsClass - Name: {Name}")]
    public class TsClass : TsModuleMember
    {
        /// <summary>
        /// Gets collection of properties of the class.
        /// </summary>
        public ICollection<TsProperty> Properties { get; private set; }

        /// <summary>
        /// Gets collection of fields of the class.
        /// </summary>
        public ICollection<TsProperty> Fields { get; private set; }

        /// <summary>
        /// Gets collection of GenericArguments for this class
        /// </summary>
        public IList<TsType> GenericArguments { get; private set; }

        /// <summary>
        /// Gets collection of constants of the class.
        /// </summary>
        public ICollection<TsProperty> Constants { get; private set; }

        /// <summary>
        /// Gets base type of the class
        /// </summary>
        /// <remarks>
        /// If the class derives from the object, the BaseType property is null.
        /// </remarks>
        public TsType BaseType { get; internal set; }

        // TODO document
        public IList<TsType> Interfaces { get; internal set; }

        /// <summary>
        /// Gets or sets bool value indicating whether this class will be ignored by TsGenerator.
        /// </summary>
        public bool IsIgnored { get; set; }

        /// <summary>
        /// Initializes a new instance of the TsClass class with the specific CLR type.
        /// </summary>
        /// <param name="type">The CLR type represented by this instance of the TsClass</param>
        public TsClass(Type type)
            : base(type)
        {

            Properties = Type
                .GetProperties()
                .Where(pi => pi.DeclaringType == Type)
                .Select(pi => new TsProperty(pi))
                .ToList();

            Fields = Type
                .GetFields()
                .Where(fi => fi.DeclaringType == Type
                             && !(fi.IsLiteral && !fi.IsInitOnly)) // skip constants
                .Select(fi => new TsProperty(fi))
                .ToList();

            Constants = Type
                .GetFields()
                .Where(fi => fi.DeclaringType == Type
                             && fi.IsLiteral && !fi.IsInitOnly) // constants only
                .Select(fi => new TsProperty(fi))
                .ToList();

            if (TypeInfo.IsGenericType)
            {
                Name = type.Name.Remove(type.Name.IndexOf('`'));
                GenericArguments = type
                    .GetGenericArguments()
                    .Select(Create)
                    .ToList();
            }
            else
            {
                Name = type.Name;
                GenericArguments = new TsType[0];
            }

            if (TypeInfo.BaseType != null && TypeInfo.BaseType != typeof(object) && TypeInfo.BaseType != typeof(ValueType))
            {
                BaseType = new TsType(TypeInfo.BaseType);
            }

            var interfaces = Type.GetInterfaces();
            Interfaces = interfaces
                .Where(@interface => @interface.GetTypeInfo().GetCustomAttribute<TsInterfaceAttribute>(false) != null)
                .Except(interfaces.SelectMany(@interface => @interface.GetInterfaces()))
                .Select(Create).ToList();

            var attribute = TypeInfo.GetCustomAttribute<TsClassAttribute>(false);
            if (attribute != null)
            {
                if (!string.IsNullOrEmpty(attribute.Name))
                {
                    Name = attribute.Name;
                }

                if (attribute.Module != null)
                {
                    Module.Name = attribute.Module;
                }
            }

            var ignoreAttribute = TypeInfo.GetCustomAttribute<TsIgnoreAttribute>(false);
            if (ignoreAttribute != null)
            {
                IsIgnored = true;
            }
        }
    }
}