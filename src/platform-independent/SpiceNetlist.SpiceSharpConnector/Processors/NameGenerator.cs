using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class NameGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameGenerator"/> class.
        /// </summary>
        /// <param name="path">The path of context</param>
        public NameGenerator(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameGenerator"/> class.
        /// </summary>
        /// <param name="path">The path of context </param>
        /// <param name="currentSubCircuit">The current subcircuit</param>
        /// <param name="pinInstanceNames">The names of pins</param>
        public NameGenerator(string path, SubCircuit currentSubCircuit, List<string> pinInstanceNames)
        {
            Path = path;
            CurrentSubCircuit = currentSubCircuit;
            PinInstanceNames = pinInstanceNames;
        }

        /// <summary>
        /// Gets the current subcircuit
        /// </summary>
        protected SubCircuit CurrentSubCircuit { get; }

        /// <summary>
        /// Gets the names of pinds for the current subcircuit
        /// </summary>
        protected List<string> PinInstanceNames { get; }

        /// <summary>
        /// Gets the path of circuit context
        /// </summary>
        protected string Path { get; }

        /// <summary>
        /// Generates  object name for current context
        /// </summary>
        /// <param name="objectName">Name of object</param>
        /// <returns>
        /// A object name for current context
        /// </returns>
        public string GenerateObjectName(string objectName)
        {
            return Path + objectName;
        }

        /// <summary>
        /// Generates object name for given context
        /// </summary>
        /// <param name="path">Path of context</param>
        /// <param name="objectName">Name of object</param>
        /// <returns>
        /// A object name for given context
        /// </returns>
        public string GenerateObjectName(string path, string objectName)
        {
            return path + objectName;
        }

        /// <summary>
        /// Generates node name for current context
        /// </summary>
        /// <param name="pinName">Pin name</param>
        /// <returns>
        /// Node name for current context
        /// </returns>
        public string GenerateNodeName(string pinName)
        {
            if (pinName == "0" || pinName == "gnd" || pinName == "GND")
            {
                return pinName.ToUpper();
            }

            if (CurrentSubCircuit != null)
            {
                Dictionary<string, string> map = new Dictionary<string, string>();

                for (var i = 0; i < CurrentSubCircuit.Pins.Count; i++)
                {
                    map[CurrentSubCircuit.Pins[i]] = PinInstanceNames[i];
                }

                if (map.ContainsKey(pinName))
                {
                    return map[pinName].ToLower();
                }
            }

            return (Path + pinName).ToLower();
        }
    }
}
