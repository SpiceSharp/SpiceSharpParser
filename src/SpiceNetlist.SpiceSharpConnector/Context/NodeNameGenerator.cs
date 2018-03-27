using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Context
{
    public class NodeNameGenerator : INodeNameGenerator
    {
        private Dictionary<string, string> pinMap = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeNameGenerator"/> class.
        /// </summary>
        public NodeNameGenerator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeNameGenerator"/> class.
        /// </summary>
        /// <param name="subCircuitName">The name of subcircuit</param>
        /// <param name="currentSubCircuit">The current subcircuit</param>
        /// <param name="pinInstanceNames">The names of pins</param>
        public NodeNameGenerator(string subCircuitName, SubCircuit currentSubCircuit, List<string> pinInstanceNames)
        {
            SubCircuitName = subCircuitName;
            SubCircuit = currentSubCircuit ?? throw new ArgumentNullException(nameof(currentSubCircuit));
            PinInstanceNames = pinInstanceNames ?? throw new ArgumentNullException(nameof(pinInstanceNames));

            for (var i = 0; i < SubCircuit.Pins.Count; i++)
            {
                pinMap[SubCircuit.Pins[i]] = PinInstanceNames[i];
            }
        }

        /// <summary>
        /// Gets the subcircuit name
        /// </summary>
        public string SubCircuitName { get; }

        /// <summary>
        /// Gets the subcircuit of this node name generator
        /// </summary>
        public SubCircuit SubCircuit { get; }

        /// <summary>
        /// Gets the names of pinds for the current subcircuit
        /// </summary>
        public List<string> PinInstanceNames { get; }

        /// <summary>
        /// Generates node name
        /// </summary>
        /// <param name="pinName">Pin name</param>
        /// <returns>
        /// Node name
        /// </returns>
        public string Generate(string pinName)
        {
            if (pinName is null)
            {
                throw new ArgumentNullException(nameof(pinName));
            }

            if (pinName == "0")
            {
                return "0";
            }

            if (pinName.ToLower() == "gnd")
            {
                return pinName.ToUpper();
            }

            if (SubCircuit != null)
            {
                if (pinMap.ContainsKey(pinName))
                {
                    return pinMap[pinName];
                }
                else
                {
                    return string.Format("{0}.{1}", SubCircuitName, pinName);
                }
            }

            return pinName;
        }
    }
}
