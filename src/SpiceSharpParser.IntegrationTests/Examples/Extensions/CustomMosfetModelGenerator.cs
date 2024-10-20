using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;

namespace SpiceSharpParser.IntegrationTests.Examples.Extensions
{
    public class CustomMosfetModelGenerator : MosfetModelGenerator
    {

        public CustomMosfetModelGenerator()
        {
            Levels.Add(4, (name, type, _) =>
            {
                var m = new Mosfet3Model(name);
                switch (type.ToLower())
                {
                    case "nmos": m.SetParameter("nmos", true); break;
                    case "pmos": m.SetParameter("pmos", true); break;
                }

                return new Model(name, m, m.Parameters);
            });
        }


    }
}