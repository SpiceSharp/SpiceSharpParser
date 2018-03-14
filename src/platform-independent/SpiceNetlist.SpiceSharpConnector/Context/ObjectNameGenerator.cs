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
        public ObjectNameGenerator(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; }

        /// <summary>
        /// Generates object name
        /// </summary>
        /// <param name="name">Name of object</param>
        /// <returns>
        /// A object name for current context
        /// </returns>
        public string GenerateObjectName(string name)
        {
            return Prefix + name;
        }
    }
}
