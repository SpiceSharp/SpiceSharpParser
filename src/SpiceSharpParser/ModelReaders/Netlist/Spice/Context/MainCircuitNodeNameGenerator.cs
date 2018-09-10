using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class MainCircuitNodeNameGenerator : INodeNameGenerator
    {
        private HashSet<string> globalsSet = new HashSet<string>();
        private Dictionary<string, string> pinMap = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainCircuitNodeNameGenerator"/> class.
        /// </summary>
        /// <param name="globals">Global pin names.</param>
        public MainCircuitNodeNameGenerator(IEnumerable<string> globals)
        {
            if (globals == null)
            {
                throw new ArgumentNullException(nameof(globals));
            }

            InitGlobals(globals);
        }

        /// <summary>
        /// Gets the globals.
        /// </summary>
        public IEnumerable<string> Globals => this.globalsSet;

        /// <summary>
        /// Gets or sets the root name.
        /// </summary>
        public string RootName { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<INodeNameGenerator> Children { get; set; } = new List<INodeNameGenerator>();

        /// <summary>
        /// Generates node name.
        /// </summary>
        /// <param name="pinName">Pin name.</param>
        /// <returns>
        /// Node name.
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

            return pinName;
        }

        /// <summary>
        /// Makes a pin name a global pin name.
        /// </summary>
        /// <param name="pinName">Pin name.</param>
        public void SetGlobal(string pinName)
        {
            // ADD thread-safety
            if (!globalsSet.Contains(pinName))
            {
                globalsSet.Add(pinName);
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
            string[] parts = path.Split('.');

            if (parts.Length == 1)
            {
                return path; // path contains only single node identifier
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
