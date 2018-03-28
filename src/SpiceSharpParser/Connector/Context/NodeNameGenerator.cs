using SpiceSharpParser.Model.SpiceObjects;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Connector.Context
{
    public class NodeNameGenerator : INodeNameGenerator
    {
        private Dictionary<string, string> pinMap = new Dictionary<string, string>();
        private readonly HashSet<string> globalsSet = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeNameGenerator"/> class.
        /// </summary>
        /// <param name="globals">Global pin names</param>
        public NodeNameGenerator(IEnumerable<string> globals)
        {
            if (globals == null)
            {
                throw new ArgumentNullException(nameof(globals));
            }

            InitGlobals(globals);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeNameGenerator"/> class.
        /// </summary>
        /// <param name="subCircuitName">The name of subcircuit</param>
        /// <param name="currentSubCircuit">The current subcircuit</param>
        /// <param name="pinInstanceNames">The names of pins</param>
        /// <param name="globals">Global pin names</param>
        public NodeNameGenerator(string subCircuitName, SubCircuit currentSubCircuit, List<string> pinInstanceNames, IEnumerable<string> globals)
        {
            if (globals == null)
            {
                throw new ArgumentNullException(nameof(globals));
            }
            SubCircuitName = subCircuitName;
            SubCircuit = currentSubCircuit ?? throw new ArgumentNullException(nameof(currentSubCircuit));
            PinInstanceNames = pinInstanceNames ?? throw new ArgumentNullException(nameof(pinInstanceNames));

            for (var i = 0; i < SubCircuit.Pins.Count; i++)
            {
                pinMap[SubCircuit.Pins[i]] = PinInstanceNames[i];
            }

            InitGlobals(globals);
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
        /// Gets the globals
        /// </summary>
        public IEnumerable<string> Globals => this.globalsSet;

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

            if (pinName.ToLower() == "gnd")
            {
                return pinName.ToUpper();
            }

            if (globalsSet.Contains(pinName))
            {
                return pinName;
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

        /// <summary>
        /// Makes a pin name a global pin name
        /// </summary>
        /// <param name="pinName">Pin name</param>
        public void SetGlobal(string pinName)
        {
            // ADD thread-safety
            if (!globalsSet.Contains(pinName))
            {
                globalsSet.Add(pinName);
            }
        }

        private void InitGlobals(IEnumerable<string> globals)
        {
            foreach (var global in globals)
            {
                globalsSet.Add(global);
            }
        }
    }
}
