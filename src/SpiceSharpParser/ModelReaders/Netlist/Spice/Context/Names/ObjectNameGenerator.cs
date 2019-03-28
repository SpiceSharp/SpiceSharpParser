using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names
{
    public class ObjectNameGenerator : IObjectNameGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNameGenerator"/> class.
        /// </summary>
        /// <param name="prefix">Naming prefix</param>
        public ObjectNameGenerator(string prefix)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        }

        /// <summary>
        /// Gets the prefix for names.
        /// </summary>
        protected string Prefix { get; }

        /// <summary>
        /// Creates a new child object name generator.
        /// </summary>
        /// <param name="name">Name of generator.</param>
        /// <returns>
        /// A new object name generator.
        /// </returns>
        public IObjectNameGenerator CreateChildGenerator(string name)
        {
            if (!string.IsNullOrEmpty(Prefix))
            {
                return new ObjectNameGenerator(string.Format("{0}.{1}", Prefix, name));
            }

            return new ObjectNameGenerator(name);
        }

        /// <summary>
        /// Generates entity object name.
        /// </summary>
        /// <param name="entityName">Name of entity.</param>
        /// <returns>
        /// A object name for entity.
        /// </returns>
        public string Generate(string entityName)
        {
            if (!string.IsNullOrEmpty(Prefix))
            {
                return string.Format("{0}.{1}", Prefix, entityName);
            }
            else
            {
                return entityName;
            }
        }
    }
}
