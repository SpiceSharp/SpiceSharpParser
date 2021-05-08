using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names
{
    public class ObjectNameGenerator : IObjectNameGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNameGenerator"/> class.
        /// </summary>
        /// <param name="prefix">Naming prefix.</param>
        /// <param name="separator">Separator.</param>
        public ObjectNameGenerator(string prefix, string separator)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            Separator = separator;
        }

        /// <summary>
        /// Gets the separator.
        /// </summary>
        public string Separator { get; }

        /// <summary>
        /// Gets the prefix for names.
        /// </summary>
        protected string Prefix { get; }

        /// <summary>
        /// Creates a new child object name generator.
        /// </summary>
        /// <param name="childGeneratorName">Name of generator.</param>
        /// <returns>
        /// A new object name generator.
        /// </returns>
        public IObjectNameGenerator CreateChildGenerator(string childGeneratorName)
        {
            if (!string.IsNullOrEmpty(Prefix))
            {
                return new ObjectNameGenerator($"{Prefix}{Separator}{childGeneratorName}", Separator);
            }

            return new ObjectNameGenerator(childGeneratorName, Separator);
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
                return $"{Prefix}{Separator}{entityName}";
            }
            else
            {
                return entityName;
            }
        }
    }
}