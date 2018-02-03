using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models
{
    class BipolarModelGenerator : ModelGenerator
    {
        public override List<string> GetGeneratedTypes()
        {
            return new List<string>() { "npm", "pnp" };
        }

        internal override Entity GenerateModel(string name, string type)
        {
            BipolarJunctionTransistorModel model = new BipolarJunctionTransistorModel(name);
            if (type == "npn")
                model.ParameterSets.SetProperty("npm", true);
            else if (type == "pnp")
                model.ParameterSets.SetProperty("pnp", true);
            return model;
        }
    }
}
