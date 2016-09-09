using System;
using System.Collections.Generic;

namespace Typescript.Definitions.Core
{
    public class TypescriptDefinitionOption
    {
        public string DefinitionFileName { get; set; } = "index";

        public string ConstantsFileName { get; set; } = "constants";

        public List<Type> Types { get; set; } = new List<Type>();
    }
}