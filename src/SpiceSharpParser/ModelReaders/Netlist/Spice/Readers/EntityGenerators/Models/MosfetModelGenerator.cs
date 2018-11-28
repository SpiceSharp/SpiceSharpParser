using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class MosfetModelGenerator : ModelGenerator
    {
        public MosfetModelGenerator()
        {
            // Default MOS levels
            Levels.Add(1, (string name, string type, string version) =>
            {
                var m = new Mosfet1Model(name);
                switch (type.ToLower())
                {
                    case "nmos": m.SetParameter("nmos", true); break;
                    case "pmos": m.SetParameter("pmos", true); break;
                }

                return m;
            });

            Levels.Add(2, (string name, string type, string version) =>
            {
                var m = new Mosfet2Model(name);
                switch (type.ToLower())
                {
                    case "nmos": m.SetParameter("nmos", true); break;
                    case "pmos": m.SetParameter("pmos", true); break;
                }

                return m;
            });

            Levels.Add(3, (string name, string type, string version) =>
            {
                var m = new Mosfet3Model(name);
                switch (type.ToLower())
                {
                    case "nmos": m.SetParameter("nmos", true); break;
                    case "pmos": m.SetParameter("pmos", true); break;
                }

                return m;
            });
        }

        /// <summary>
        /// Gets generated Spice types by generator.
        /// </summary>
        /// <returns>
        /// Generated Spice types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes => new List<string>() { "nmos", "pmos" };

        /// <summary>
        /// Gets available model generators indexed by their LEVEL.
        /// The parameters passed are name, type (nmos or pmos) and the version.
        /// </summary>
        protected Dictionary<int, Func<string, string, string, SpiceSharp.Components.Model>> Levels { get; } = new Dictionary<int, Func<string, string, string, SpiceSharp.Components.Model>>();

        public override SpiceSharp.Components.Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context)
        {
            var clonedParameters = (ParameterCollection)parameters.Clone();

            int level = 1;
            string version = null;
            int lindex = -1, vindex = -1;
            for (int i = 0; i < clonedParameters.Count; i++)
            {
                if (clonedParameters[i] is AssignmentParameter ap)
                {
                    if (ap.Name.ToLower() == "level")
                    {
                        lindex = i;
                        level = (int)Math.Round(context.EvaluateDouble(ap.Value));
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
                clonedParameters.RemoveAt(lindex);
            }

            if (vindex >= 0)
            {
                clonedParameters.RemoveAt(vindex < lindex ? vindex : vindex - 1);
            }

            // Generate the model
            SpiceSharp.Components.Model model = null;
            if (Levels.ContainsKey(level))
            {
                model = Levels[level].Invoke(id, type, version);
            }
            else
            {
                throw new Exception($"Unknown mosfet model level {level}");
            }

            // Read all the parameters
            SetParameters(context, model, clonedParameters);

            return model;
        }
    }
}
