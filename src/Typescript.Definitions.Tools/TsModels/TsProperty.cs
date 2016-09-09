using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Typescript.Definitions.Tools.Attributes;

namespace Typescript.Definitions.Tools.TsModels
{
    /// <summary>
    /// Represents a property of the class in the code model.
    /// </summary>
    [DebuggerDisplay("Name: {Name}")]
    public class TsProperty
    {
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets type of the property.
        /// </summary>
        public TsType PropertyType { get; set; }

        /// <summary>
        /// Gets the CLR property represented by this TsProperty.
        /// </summary>
        public MemberInfo MemberInfo { get; set; }

        /// <summary>
        /// Gets or sets bool value indicating whether this property will be ignored by TsGenerator.
        /// </summary>
        public bool IsIgnored { get; set; }

        /// <summary>
        /// Gets or sets bool value indicating whether this property is optional in TypeScript interface.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Gets the GenericArguments for this property.
        /// </summary>
        public IList<TsType> GenericArguments { get; private set; }

        /// <summary>
        /// Gets or sets the constant value of this property.
        /// </summary>
        public object ConstantValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the TsProperty class with the specific CLR property.
        /// </summary>
        /// <param name="memberInfo">The CLR property represented by this instance of the TsProperty.</param>
        public TsProperty(PropertyInfo memberInfo)
        {
            this.MemberInfo = memberInfo;
            this.Name = memberInfo.Name;

            var propertyType = memberInfo.PropertyType;
            var propertyTypeInfo = propertyType.GetTypeInfo();
            if (propertyType.IsNullable())
            {
                propertyType = propertyType.GetNullableValueType();
            }

            this.GenericArguments = propertyTypeInfo.IsGenericType ? propertyType.GetGenericArguments().Select(o => new TsType(o)).ToArray() : new TsType[0];

            this.PropertyType = propertyTypeInfo.IsEnum ? new TsEnum(propertyType) : new TsType(propertyType);

            var attribute = memberInfo.GetCustomAttribute<TsPropertyAttribute>(false);
            if (attribute != null)
            {
                if (!string.IsNullOrEmpty(attribute.Name))
                {
                    this.Name = attribute.Name;
                }

                this.IsOptional = attribute.IsOptional;
            }

            this.IsIgnored = (memberInfo.GetCustomAttribute<TsIgnoreAttribute>(false) != null);

            // Only fields can be constants.
            this.ConstantValue = null;
        }

        /// <summary>
        /// Initializes a new instance of the TsProperty class with the specific CLR field.
        /// </summary>
        /// <param name="memberInfo">The CLR field represented by this instance of the TsProperty.</param>
        public TsProperty(FieldInfo memberInfo)
        {
            this.MemberInfo = memberInfo;
            this.Name = memberInfo.Name;

            if (memberInfo.DeclaringType.GetTypeInfo().IsGenericType)
            {
                var definitionType = memberInfo.DeclaringType.GetGenericTypeDefinition();
                var definitionTypeProperty = definitionType.GetProperty(memberInfo.Name);
                if (definitionTypeProperty.PropertyType.IsGenericParameter)
                {
                    this.PropertyType = TsType.Any;
                }
                else
                {
                    this.PropertyType = memberInfo.FieldType.GetTypeInfo().IsEnum ? new TsEnum(memberInfo.FieldType) : new TsType(memberInfo.FieldType);
                }
            }
            else
            {
                var propertyType = memberInfo.FieldType;
                if (propertyType.IsNullable())
                {
                    propertyType = propertyType.GetNullableValueType();
                }

                this.PropertyType = propertyType.GetTypeInfo().IsEnum ? new TsEnum(propertyType) : new TsType(propertyType);
            }

            var attribute = memberInfo.GetCustomAttribute<TsPropertyAttribute>(false);
            if (attribute != null)
            {
                if (!string.IsNullOrEmpty(attribute.Name))
                {
                    this.Name = attribute.Name;
                }

                this.IsOptional = attribute.IsOptional;
            }

            this.IsIgnored = (memberInfo.GetCustomAttribute<TsIgnoreAttribute>(false) != null);

            if (memberInfo.IsLiteral && !memberInfo.IsInitOnly)
            {
                // it's a constant
                this.ConstantValue = memberInfo.GetValue(null);
            }
            else
            {
                // not a constant
                this.ConstantValue = null;
            }
        }
    }
}