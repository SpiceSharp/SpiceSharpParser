using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Extensions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class MosfetModelGenerator : IModelGenerator
    {
        public MosfetModelGenerator()
        {
            // Default MOS levels
            Levels.Add(1, (Identifier name, string type, string version) =>
            {
                var m = new Mosfet1Model(name);
                switch (type)
                {
                    case "nmos": m.SetParameter("nmos", true); break;
                    case "pmos": m.SetParameter("pmos", true); break;
                }

                return m;
            });

            Levels.Add(2, (Identifier name, string type, string version) =>
            {
                var m = new Mosfet2Model(name);
                switch (type)
                {
                    case "nmos": m.SetParameter("nmos", true); break;
                    case "pmos": m.SetParameter("pmos", true); break;
                }

                return m;
            });

            Levels.Add(3, (Identifier name, string type, string version) =>
            {
                var m = new Mosfet3Model(name);
                switch (type)
                {
                    case "nmos": m.SetParameter("nmos", true); break;
                    case "pmos": m.SetParameter("pmos", true); break;
                }

                return m;
            });
        }

        /// <summary>
        /// Gets available model generators indexed by their LEVEL.
        /// The parameters passed are name, type (nmos or pmos) and the version.
        /// </summary>
        protected Dictionary<int, Func<Identifier, string, string, SpiceSharp.Components.Model>> Levels { get; } = new Dictionary<int, Func<Identifier, string, string, SpiceSharp.Components.Model>>();

        /// <summary>
        /// Gets generated Spice types by generator.
        /// </summary>
        /// <returns>
        /// Generated Spice types.
        /// </returns>
        public IEnumerable<string> GeneratedTypes
        {
            get
            {
                return new List<string>() { "nmos", "pmos" };
            }
        }

        public SpiceSharp.Components.Model Generate(string name, string type, ParameterCollection parameters, IReadingContext context)
        {
            var clonedParameters = (ParameterCollection)parameters.Clone();
            switch (clonedParameters.Count)
            {
                case 0: throw new Exception("Model name and type expected");
            }

            int level = 0;
            string version = null;
            int lindex = -1, vindex = -1;
            for (int i = 0; i < clonedParameters.Count; i++)
            {
                if (clonedParameters[i] is AssignmentParameter ap)
                {
                    if (ap.Name.ToLower() == "level")
                    {
                        lindex = i;
                        level = (int)Math.Round(context.ParseDouble(ap.Value));
                    }

                    if (ap.Name.ToLower() == "version")
                    {
                        vindex = i;
                        version = ap.Value.ToLower();
                    }

                    if (vindex >= 0 && lindex >= 0)
                    {
                        break;
                    }
                }
            }

            if (lindex >= 0)
            {
                clonedParameters.Remove(lindex);
            }

            if (vindex >= 0)
            {
                clonedParameters.Remove(vindex < lindex ? vindex : vindex - 1);
            }

            // Generate the model
            SpiceSharp.Components.Model model = null;
            if (Levels.ContainsKey(level))
            {
                model = Levels[level].Invoke(name, type, version);
            }
            else
            {
                throw new Exception($"Unknown mosfet model level {level}");
            }

            // Read all the parameters
            context.SetParameters(model, clonedParameters, true);

            return model;
        }
    }
}
