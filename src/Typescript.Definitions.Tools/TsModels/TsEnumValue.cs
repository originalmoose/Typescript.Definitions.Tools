using System;
using System.Reflection;

namespace Typescript.Definitions.Tools.TsModels
{
    /// <summary>
    /// Represents a value of the enum
    /// </summary>
    public class TsEnumValue
    {
        /// <summary>
        /// Gets or sets name of the enum value
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets value of the enum
        /// </summary>
        public string Value { get; private set; }

        public FieldInfo Field { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TsEnumValue class.
        /// </summary>
        public TsEnumValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the TsEnumValue class with the specific name and value.
        /// </summary>
        public TsEnumValue(FieldInfo field)
        {
            Field = field;
            Name = field.Name;

            var value = field.GetValue(null);

            var valueType = Enum.GetUnderlyingType(value.GetType());
            if (valueType == typeof(byte))
            {
                Value = ((byte)value).ToString();
            }
            if (valueType == typeof(sbyte))
            {
                Value = ((sbyte)value).ToString();
            }
            if (valueType == typeof(short))
            {
                Value = ((short)value).ToString();
            }
            if (valueType == typeof(ushort))
            {
                Value = ((ushort)value).ToString();
            }
            if (valueType == typeof(int))
            {
                Value = ((int)value).ToString();
            }
            if (valueType == typeof(uint))
            {
                Value = ((uint)value).ToString();
            }
            if (valueType == typeof(long))
            {
                Value = ((long)value).ToString();
            }
            if (valueType == typeof(ulong))
            {
                Value = ((ulong)value).ToString();
            }
        }
    }
}