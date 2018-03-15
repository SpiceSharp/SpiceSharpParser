using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Context
{
    public class ObjectNameGenerator
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
        /// Gets the prefix for names
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Generates entity object name
        /// </summary>
        /// <param name="entityName">Name of entity</param>
        /// <returns>
        /// A object name for entity
        /// </returns>
        public string Generate(string entityName)
        {
            return Prefix + entityName;
        }
    }
}
