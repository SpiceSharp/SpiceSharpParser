using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceNetlist.SpiceSharpConnector.Context;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models
{
    public class SwitchModelGenerator : ModelGenerator
    {
        public override List<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "sw", "csw" };
        }

        internal override Entity GenerateModel(string name, string type)
        {
            switch (type)
            {
                case "sw": return new VoltageSwitchModel(name);
                case "csw": return new CurrentSwitchModel(name);
            }

            return null;
        }
    }
}
