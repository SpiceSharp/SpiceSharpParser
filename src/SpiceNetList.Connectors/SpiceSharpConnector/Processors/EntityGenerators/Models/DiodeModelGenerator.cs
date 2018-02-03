using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System.Collections.Generic;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors.EntityGenerators.Models
{
    class DiodeModelGenerator : ModelGenerator
    {
        public override List<string> GetGeneratedTypes()
        {
            return new List<string>() { "d" };
        }

        internal override Entity GenerateModel(string name, string type)
        {
            return new DiodeModel(name);
        }
    }
}
