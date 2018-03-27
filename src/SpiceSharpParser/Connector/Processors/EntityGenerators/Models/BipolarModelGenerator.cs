using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpParser.Connector.Processors.EntityGenerators.Models
{
    public class BipolarModelGenerator : ModelGenerator
    {
        /// <summary>
        /// Gets the generated types
        /// </summary>
        /// <returns>
        /// A list of generated types
        /// </returns>
        public override IEnumerable<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "npn", "pnp" };
        }

        internal override Entity GenerateModel(string name, string type)
        {
            BipolarJunctionTransistorModel model = new BipolarJunctionTransistorModel(name);

            if (type.ToLower() == "npn")
            {
                model.SetParameter("npn", true);
            }
            else if (type.ToLower() == "pnp")
            {
                model.SetParameter("pnp", true);
            }

            return model;
        }
    }
}
