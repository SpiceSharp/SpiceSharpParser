using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models
{
    public class MosfetModelGenerator : ModelGenerator
    {
        public MosfetModelGenerator()
        {
            // Default MOS levels
            Levels.Add(1, (Identifier name, string type, string version) =>
            {
                var m = new Mosfet1Model(name);
                switch (type)
                {
                    case "nmos": m.ParameterSets.SetProperty("nmos", true); break;
                    case "pmos": m.ParameterSets.SetProperty("pmos", true); break;
                }
                return m;
            });
            Levels.Add(2, (Identifier name, string type, string version) =>
            {
                var m = new Mosfet2Model(name);
                switch (type)
                {
                    case "nmos": m.ParameterSets.SetProperty("nmos", true); break;
                    case "pmos": m.ParameterSets.SetProperty("pmos", true); break;
                }
                return m;
            });
            Levels.Add(3, (Identifier name, string type, string version) =>
            {
                var m = new Mosfet3Model(name);
                switch (type)
                {
                    case "nmos": m.ParameterSets.SetProperty("nmos", true); break;
                    case "pmos": m.ParameterSets.SetProperty("pmos", true); break;
                }
                return m;
            });
        }

        /// <summary>
        /// Available model generators indexed by their LEVEL.
        /// The parameters passed are name, type (nmos or pmos) and the version.
        /// </summary>
        public Dictionary<int, Func<Identifier, string, string, Entity>> Levels { get; } = new Dictionary<int, Func<Identifier, string, string, Entity>>();

        public override List<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "nmos", "pmos" };
        }

        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, ProcessingContext context)
        {
            var clonedParameters = parameters.Clone();
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
            Entity model = null;
            if (Levels.ContainsKey(level))
            {
                model = Levels[level].Invoke(id, type, version);
            }
            else
            {
                throw new Exception($"Unknown mosfet model level {level}");
            }

            // Read all the parameters
            context.SetEntityParameters(model, clonedParameters);

            return model;
        }

        internal override Entity GenerateModel(string name, string type)
        {
            throw new Exception("Shouldn't be called");
        }
    }
}
