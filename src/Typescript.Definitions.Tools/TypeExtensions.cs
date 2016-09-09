using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Typescript.Definitions.Core
{
    public static class TypeExtensions
    {

        public static IEnumerable<TypeInfo> GetLoadableDefinedTypes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo);
            }
        }
    }
}