using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Model.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Spice.Context
{
    public class SubcircuitNodeNameGenerator : INodeNameGenerator
    {
        private Dictionary<string, string> pinMap = new Dictionary<string, string>();
        private readonly HashSet<string> globalsSet = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitNodeNameGenerator"/> class.
        /// </summary>
        /// <param name="subcircuitFullName">The fullname of subcircuit</param>
        /// <param name="subCircuitName">The name of subcircuit</param>
        /// <param name="currentSubCircuit">The current subcircuit</param>
        /// <param name="pinInstanceNames">The names of pins</param>
        /// <param name="globals">Global pin names</param>
        public SubcircuitNodeNameGenerator(string subcircuitFullName, string subCircuitName, SubCircuit currentSubCircuit, List<string> pinInstanceNames, IEnumerable<string> globals)
        {
            if (globals == null)
            {
                throw new ArgumentNullException(nameof(globals));
            }

            RootName = subCircuitName;
            FullName = subcircuitFullName;

            SubCircuit = currentSubCircuit ?? throw new ArgumentNullException(nameof(currentSubCircuit));
            PinInstanceNames = pinInstanceNames ?? throw new ArgumentNullException(nameof(pinInstanceNames));

            for (var i = 0; i < SubCircuit.Pins.Count; i++)
            {
                pinMap[SubCircuit.Pins[i]] = PinInstanceNames[i];
            }

            InitGlobals(globals);
        }

        /// <summary>
        /// Gets the subcircuit of this node name generator
        /// </summary>
        public SubCircuit SubCircuit { get; }

        /// <summary>
        /// Gets the names of pins for the current subcircuit
        /// </summary>
        public List<string> PinInstanceNames { get; }

        /// <summary>
        /// Gets the globals
        /// </summary>
        public IEnumerable<string> Globals => this.globalsSet;

       /// <summary>
       /// Gets or sets the root name
       /// </summary>
        public string RootName { get; set; }

        /// <summary>
        /// Gets or sets the full name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets children of node name generator
        /// </summary>
        public List<INodeNameGenerator> Children { get; set; } = new List<INodeNameGenerator>();

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

            if (pinMap.ContainsKey(pinName))
            {
                return pinMap[pinName];
            }
            else
            {
                return string.Format("{0}.{1}", FullName, pinName);
            }
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

        /// <summary>
        /// Parses a path and generate a node name
        /// </summary>
        /// <param name="path">Node path</param>
        /// <returns>
        /// A node name
        /// </returns>
        public string Parse(string path)
        {
            string[] parts = path.Split('.');

            if (parts.Length == 1)
            {
                string pinName = parts[0];

                if (globalsSet.Contains(pinName))
                {
                    return pinName;
                }

                if (pinMap.ContainsKey(pinName))
                {
                    return pinMap[pinName];
                }
                else
                {
                    return FullName + "." + pinName;
                }
            }
            else
            {
                string firstSubcircuit = parts[0];

                foreach (var child in Children)
                {
                    if (child.RootName == firstSubcircuit)
                    {
                        string restOfPath = string.Join(".", parts.Skip(1));
                        return child.Parse(restOfPath);
                    }
                }
            }

            return null;
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
