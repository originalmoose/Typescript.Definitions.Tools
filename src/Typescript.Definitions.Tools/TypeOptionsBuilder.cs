namespace Typescript.Definitions.Tools
{
    public class TypeOptionsBuilder
    {
        internal TypeOptions Options = new TypeOptions();


        /// <summary>
        /// Changes the name of the type being configured .
        /// </summary>
        /// <param name="name">The new name of the type</param>
        /// <returns>Instance of the <see cref="TypeOptionsBuilder"/> that enables fluent configuration.</returns>
        public TypeOptionsBuilder Named(string name)
        {
            Options.Name = name;
            return this;
        }

        /// <summary>
        /// Maps the type being configured to the specific module
        /// </summary>
        /// <param name="moduleName">The name of the module</param>
        /// <returns>Instance of the <see cref="TypeOptionsBuilder"/> that enables fluent configuration.</returns>
        public TypeOptionsBuilder ToModule(string moduleName)
        {
            Options.Module = moduleName;
            return this;
        }

        /// <summary>
        /// Ignores this member when generating typescript
        /// </summary>
        /// <returns>Instance of the <see cref="TypeOptionsBuilder"/> that enables fluent configuration.</returns>
        public TypeOptionsBuilder Ignore()
        {
            Options.IsIgnored = true;
            return this;
        }
    }
}