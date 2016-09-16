using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Typescript.Definitions.Tools.Attributes;
using Typescript.Definitions.Tools.Extensions;
using Typescript.Definitions.Tools.TsModels;

namespace Typescript.Definitions.Tools
{
    public class TsModelBuilder
    {
        internal Dictionary<Type, TsClass> Classes { get; } = new Dictionary<Type, TsClass>();

        internal Dictionary<Type, TsEnum> Enums { get; } = new Dictionary<Type, TsEnum>();

        /// <summary>
        /// Adds type with all referenced classes to the model.
        /// </summary>
        /// <typeparam name="T">The type to add to the model.</typeparam>
        /// <returns>type added to the model</returns>
        public TsModuleMember Add<T>()
        {
            return Add<T>(true);
        }

        /// <summary>
        /// Adds type and optionally referenced classes to the model.
        /// </summary>
        /// <typeparam name="T">The type to add to the model.</typeparam>
        /// <param name="includeReferences">bool value indicating whether classes referenced by T should be added to the model.</param>
        /// <returns>type added to the model</returns>
        public TsModuleMember Add<T>(bool includeReferences)
        {
            return Add(typeof(T), includeReferences);
        }

        /// <summary>
        /// Adds type with all referenced classes to the model.
        /// </summary>
        /// <param name="clrType">The type to add to the model.</param>
        /// <returns>type added to the model</returns>
        public TsModuleMember Add(Type clrType)
        {
            return Add(clrType, true);
        }

        /// <summary>
        /// Adds type and optionally referenced classes to the model.
        /// </summary>
        /// <param name="clrType">The type to add to the model.</param>
        /// <param name="includeReferences">bool value indicating whether classes referenced by T should be added to the model.</param>
        /// <param name="typeConverters"></param>
        /// <returns>type added to the model</returns>
        public TsModuleMember Add(Type clrType, bool includeReferences, Dictionary<Type, TypeConverter> typeConverters = null)
        {
            var typeFamily = TsType.GetTypeFamily(clrType);
            if (typeFamily != TsTypeFamily.Class && typeFamily != TsTypeFamily.Enum)
            {
                throw new ArgumentException(string.Format("Type '{0}' isn't class or struct. Only classes and structures can be added to the model", clrType.FullName));
            }

            if (clrType.IsNullable())
            {
                return Add(clrType.GetNullableValueType(), includeReferences, typeConverters);
            }

            if (typeFamily == TsTypeFamily.Enum)
            {
                var enumType = new TsEnum(clrType);
                AddEnum(enumType);
                return enumType;
            }

            if (clrType.GetTypeInfo().IsGenericType)
            {
                if (!Classes.ContainsKey(clrType))
                {
                    var openGenericType = clrType.GetGenericTypeDefinition();
                    var added = new TsClass(openGenericType);
                    Classes[openGenericType] = added;
                    if (includeReferences)
                    {
                        AddReferences(added, typeConverters);

                        foreach (var e in added.Properties.Where(p => p.PropertyType.Type.GetTypeInfo().IsEnum))
                            AddEnum(e.PropertyType as TsEnum);
                    }
                }
            }

            if (!Classes.ContainsKey(clrType))
            {
                var added = new TsClass(clrType);
                Classes[clrType] = added;
                if (clrType.IsGenericParameter) added.IsIgnored = true;
                if (clrType.GetTypeInfo().IsGenericType) added.IsIgnored = true;

                if (added.BaseType != null)
                {
                    Add(added.BaseType.Type);
                }
                if (includeReferences)
                {
                    AddReferences(added, typeConverters);

                    foreach (var e in added.Properties.Where(p => p.PropertyType.Type.GetTypeInfo().IsEnum))
                        AddEnum(e.PropertyType as TsEnum);
                }

                foreach (var @interface in added.Interfaces)
                {
                    Add(@interface.Type);
                }

                return added;
            }
            else
            {
                return Classes[clrType];
            }
        }

        /// <summary>
        /// Adds enum to the model
        /// </summary>
        /// <param name="tsEnum">The enum to add</param>
        private void AddEnum(TsEnum tsEnum)
        {
            if (!Enums.ContainsKey(tsEnum.Type))
            {
                Enums[tsEnum.Type] = tsEnum;
            }
        }

        /// <summary>
        /// Adds all classes annotated with the TsClassAttribute from an assembly to the model.
        /// </summary>
        /// <param name="assembly">The assembly with classes to add</param>
        public void Add(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t =>
                    (t.GetTypeInfo().GetCustomAttribute<TsClassAttribute>(false) != null && TsType.GetTypeFamily(t) == TsTypeFamily.Class) ||
                    (t.GetTypeInfo().GetCustomAttribute<TsEnumAttribute>(false) != null && TsType.GetTypeFamily(t) == TsTypeFamily.Enum) ||
                    (t.GetTypeInfo().GetCustomAttribute<TsInterfaceAttribute>(false) != null && TsType.GetTypeFamily(t) == TsTypeFamily.Class)
            ))
            {
                Add(type);
            }
        }
        
        /// <summary>
        /// Build the model.
        /// </summary>
        /// <returns>The script model with the classes.</returns>
        public TsModel Build()
        {
            var model = new TsModel(this.Classes.Values, this.Enums.Values);
            model.RunVisitor(new TypeResolver(model));
            return model;
        }

        /// <summary>
        /// Adds classes referenced by the class to the model
        /// </summary>
        /// <param name="classModel"></param>
        /// <param name="typeConverters"></param>
        private void AddReferences(TsClass classModel, Dictionary<Type, TypeConverter> typeConverters)
        {
            foreach (var property in classModel.Properties.Where(model => !model.IsIgnored))
            {
                var propertyTypeFamily = TsType.GetTypeFamily(property.PropertyType.Type);
                if (propertyTypeFamily == TsTypeFamily.Collection)
                {
                    var collectionItemType = TsType.GetEnumerableType(property.PropertyType.Type);
                    while (collectionItemType != null)
                    {
                        var typeFamily = TsType.GetTypeFamily(collectionItemType);

                        switch (typeFamily)
                        {
                            case TsTypeFamily.Class:
                                Add(collectionItemType);
                                collectionItemType = null;
                                break;
                            case TsTypeFamily.Enum:
                                AddEnum(new TsEnum(collectionItemType));
                                collectionItemType = null;
                                break;
                            case TsTypeFamily.Collection:
                                collectionItemType = TsType.GetEnumerableType(collectionItemType);
                                break;
                            default:
                                collectionItemType = null;
                                break;
                        }
                    }
                }
                else if (propertyTypeFamily == TsTypeFamily.Class)
                {
                    if (typeConverters == null || !typeConverters.ContainsKey(property.PropertyType.Type))
                    {
                        Add(property.PropertyType.Type);
                    }
                    else
                    {
                        Add(property.PropertyType.Type, false, typeConverters);
                    }
                }
            }
            foreach (var genericArgument in classModel.GenericArguments)
            {
                var propertyTypeFamily = TsType.GetTypeFamily(genericArgument.Type);
                if (propertyTypeFamily == TsTypeFamily.Collection)
                {
                    var collectionItemType = TsType.GetEnumerableType(genericArgument.Type);
                    if (collectionItemType != null)
                    {
                        var typeFamily = TsType.GetTypeFamily(collectionItemType);

                        switch (typeFamily)
                        {
                            case TsTypeFamily.Class:
                                Add(collectionItemType);
                                break;
                            case TsTypeFamily.Enum:
                                AddEnum(new TsEnum(collectionItemType));
                                break;
                        }
                    }
                }
                else if (propertyTypeFamily == TsTypeFamily.Class)
                {
                    Add(genericArgument.Type);
                }
            }
        }
    }
}
