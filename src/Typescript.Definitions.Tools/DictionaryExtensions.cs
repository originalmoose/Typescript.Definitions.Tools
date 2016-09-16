using System;
using System.Collections.Generic;
using Typescript.Definitions.Tools.TsModels;

namespace Typescript.Definitions.Tools
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Converts the specific type to it's string representation using a formatter registered for the type
        /// </summary>
        /// <param name="formatters"></param>
        /// <param name="type">The type to format.</param>
        /// <returns>The string representation of the type.</returns>
        public static string FormatType(this Dictionary<Type, TsTypeFormatter> formatters, TsType type)
        {
            return formatters.ContainsKey(type.GetType()) ? formatters[type.GetType()](type) : "any";
        }

        /// <summary>
        /// Registers the formatter for the specific TsType
        /// </summary>
        /// <typeparam name="TFor">The type to register the formatter for. TFor is restricted to TsType and derived classes.</typeparam>
        /// <param name="formatters"></param>
        /// <param name="formatter">The formatter to register</param>
        public static void RegisterTypeFormatter<TFor>(this Dictionary<Type, TsTypeFormatter> formatters, TsTypeFormatter formatter)
        {
            formatters[typeof(TFor)] = formatter;
        }


        /// <summary>
        /// Registers the converter for the specific Type
        /// </summary>
        /// <typeparam name="TFor">The type to register the converter for.</typeparam>
        /// <param name="converters"></param>
        /// <param name="converter">The converter to register</param>
        public static void RegisterTypeConverter<TFor>(this Dictionary<Type, TypeConverter> converters, TypeConverter converter)
        {
            converters[typeof(TFor)] = converter;
        }

        /// <summary>
        /// Checks whether any converter is registered for the specific Type
        /// </summary>
        /// <param name="converters"></param>
        /// <param name="type">The type to check</param>
        /// <returns>true if a converter is registered for the specific Type otherwise return false</returns>
        public static bool IsConverterRegistered(this Dictionary<Type, TypeConverter> converters, Type type)
        {
            return converters.ContainsKey(type);
        }


        /// <summary>
        /// Converts specific type to its string representation.
        /// </summary>
        /// <param name="converters"></param>
        /// <param name="type">The type to convert</param>
        /// <returns>the string representation of the type if a converter of the type is registered otherwise return null</returns>
        public static string ConvertType(this Dictionary<Type, TypeConverter> converters, Type type)
        {
            return converters.ContainsKey(type) ? converters[type](type) : null;
        }
    }
}