using System;

namespace Typescript.Definitions.Tools.TsModels
{
    /// <summary>
    /// Represents a type that can be places inside module
    /// </summary>
    public abstract class TsModuleMember : TsType
    {
        private TsModule _module;

        /// <summary>
        /// Gets or sets module, that contains this class.
        /// </summary>
        public TsModule Module
        {
            get
            {
                return _module;
            }
            set
            {
                _module?.Remove(this);
                _module = value;
                _module?.Add(this);
            }
        }

        /// <summary>
        /// Gets or sets the name of the module member.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes TsModuleMember class with the specific CLR type.
        /// </summary>
        /// <param name="type">The CLR type represented by this instance of the ModuleMember</param>
        protected TsModuleMember(Type type)
            : base(type)
        {

            var moduleName = Type.Namespace;
            if (type.DeclaringType != null)
            {
                moduleName += "." + type.DeclaringType.Name;
            }

            Module = new TsModule(moduleName);
            Name = Type.Name;
        }
    }
}