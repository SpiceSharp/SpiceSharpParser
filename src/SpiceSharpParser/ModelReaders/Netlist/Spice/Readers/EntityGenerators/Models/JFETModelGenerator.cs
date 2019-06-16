using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class JFETModelGenerator : ModelGenerator
    {
        public override IEnumerable<string> GeneratedTypes => new List<string>() { "pjf", "njf" };

        public override SpiceSharp.Components.Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context)
        {
            var model = new JFETModel(id);
            switch (type.ToLower())
            {
                case "pjf": model.SetParameter("pjf", true); break;
                case "njf": model.SetParameter("njf", true); break;
            }

            SetParameters(context, model, parameters);
            return model;
        }
    }
}
