using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names
{
    public class SubcircuitNodeNameGenerator : INodeNameGenerator
    {
        private readonly Dictionary<string, string> _pinMap;
        private HashSet<string> _globals;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitNodeNameGenerator"/> class.
        /// </summary>
        /// <param name="subcircuitFullName">The fullname of subcircuit.</param>
        /// <param name="subCircuitName">The name of subcircuit.</param>
        /// <param name="currentSubCircuit">The current subcircuit.</param>
        /// <param name="pinInstanceNames">The names of pins.</param>
        /// <param name="globals">Global pin names.</param>
        /// <param name="isNodeNameCaseSensitive">Is node name case sensitive.</param>
        public SubcircuitNodeNameGenerator(string subcircuitFullName, string subCircuitName, SubCircuit currentSubCircuit, List<string> pinInstanceNames, IEnumerable<string> globals, bool isNodeNameCaseSensitive)
        {
            RootName = subCircuitName ?? throw new ArgumentNullException(nameof(subCircuitName));
            FullName = subcircuitFullName ?? throw new ArgumentNullException(nameof(subcircuitFullName));

            SubCircuit = currentSubCircuit ?? throw new ArgumentNullException(nameof(currentSubCircuit));
            PinInstanceNames = pinInstanceNames ?? throw new ArgumentNullException(nameof(pinInstanceNames));
            if (globals == null)
            {
                throw new ArgumentNullException(nameof(globals));
            }

            _pinMap = new Dictionary<string, string>(StringComparerProvider.Get(isNodeNameCaseSensitive));

            if (SubCircuit.Pins.Count != PinInstanceNames.Count)
            {
                throw new SpiceSharpParserException($"Subcircuit: {subcircuitFullName} has wrong number of nodes");
            }

            for (var i = 0; i < SubCircuit.Pins.Count; i++)
            {
                var pinIdentifier = SubCircuit.Pins[i].Image;
                var pinInstanceIdentifier = PinInstanceNames[i];
                _pinMap[pinIdentifier] = pinInstanceIdentifier;
            }

            IsNodeNameCaseSensitive = isNodeNameCaseSensitive;
            InitGlobals(globals);
        }

        public Dictionary<string, string> PinMap
        {
            get
            {
                return _pinMap;
            }
        }

        public bool IsNodeNameCaseSensitive { get; }

        /// <summary>
        /// Gets the subcircuit of this node name generator.
        /// </summary>
        public SubCircuit SubCircuit { get; }

        /// <summary>
        /// Gets the names of pins for the current subcircuit.
        /// </summary>
        public List<string> PinInstanceNames { get; }

        /// <summary>
        /// Gets the globals.
        /// </summary>
        public IEnumerable<string> Globals => _globals;

        /// <summary>
        /// Gets or sets the root name.
        /// </summary>
        public string RootName { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets children of node name generator.
        /// </summary>
        public List<INodeNameGenerator> Children { get; set; } = new List<INodeNameGenerator>();

        /// <summary>
        /// Generates node name.
        /// </summary>
        /// <param name="nodeName">Pin name.</param>
        /// <returns>
        /// Node name.
        /// </returns>
        public string Generate(string nodeName)
        {
            if (nodeName is null)
            {
                throw new ArgumentNullException(nameof(nodeName));
            }

            if (nodeName.ToUpper() == "GND")
            {
                return nodeName;
            }

            if (_globals.Contains(nodeName))
            {
                return nodeName;
            }

            var pinIdentifier = nodeName;

            if (_pinMap.ContainsKey(pinIdentifier))
            {
                return _pinMap[pinIdentifier];
            }
            else
            {
                return $"{FullName}.{nodeName}";
            }
        }

        /// <summary>
        /// Makes a pin name a global pin name.
        /// </summary>
        /// <param name="pinName">Pin name.</param>
        public void SetGlobal(string pinName)
        {
            if (pinName == null)
            {
                throw new ArgumentNullException(nameof(pinName));
            }

            if (!_globals.Contains(pinName))
            {
                _globals.Add(pinName);
            }
        }

        /// <summary>
        /// Parses a path and generate a node name.
        /// </summary>
        /// <param name="path">Node path.</param>
        /// <returns>
        /// A node name.
        /// </returns>
        public string Parse(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            string[] parts = path.Split('.');

            if (parts.Length == 1)
            {
                string pinName = parts[0];

                if (_globals.Contains(pinName))
                {
                    return pinName;
                }

                if (_pinMap.ContainsKey(pinName))
                {
                    return _pinMap[pinName];
                }
                else
                {
                    return $"{FullName}.{pinName}";
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

            if (parts[0] == RootName)
            {
                string restOfPath = string.Join(".", parts.Skip(1));
                return restOfPath;
            }

            return null;
        }

        private void InitGlobals(IEnumerable<string> globals)
        {
            _globals = new HashSet<string>(StringComparerProvider.Get(IsNodeNameCaseSensitive));

            foreach (var global in globals)
            {
                _globals.Add(global);
            }
        }
    }
}