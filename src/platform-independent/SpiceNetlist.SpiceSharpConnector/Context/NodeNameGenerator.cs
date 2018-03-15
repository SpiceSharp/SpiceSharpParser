using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Context
{
    public class NodeNameGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeNameGenerator"/> class.
        /// </summary>
        public NodeNameGenerator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeNameGenerator"/> class.
        /// </summary>
        /// <param name="currentSubCircuit">The current subcircuit</param>
        /// <param name="pinInstanceNames">The names of pins</param>
        public NodeNameGenerator(SubCircuit currentSubCircuit, List<string> pinInstanceNames)
        {
            CurrentSubCircuit = currentSubCircuit ?? throw new ArgumentNullException(nameof(currentSubCircuit));
            PinInstanceNames = pinInstanceNames ?? throw new ArgumentNullException(nameof(pinInstanceNames));
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
        /// Generates node name
        /// </summary>
        /// <param name="pinName">Pin name</param>
        /// <returns>
        /// Node name for current context
        /// </returns>
        public string Generate(string pinName)
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

            return pinName.ToLower();
        }
    }
}
