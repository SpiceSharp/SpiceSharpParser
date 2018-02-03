using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.Connectors.SpiceSharpConnector.Processors.Generators.Models;
using SpiceSharp.Circuits;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors
{
    class ModelProcessor : StatementProcessor
    {
        public override void Init()
        {
            Generators.Add(new RLCModelGenerator());
        }

        public override void Process(Statement statement, NetList netlist)
        {
            Model model = statement as Model;
            string name = model.Name.ToLower();
            var complex = model.Parameters.Values[0] as ComplexParameter;
            var type = complex.Name;

            foreach (var generator in Generators)
            {
                if (generator.GetGeneratedTypes().Contains(type))
                {
                    Entity spiceSharpModel = generator.Generate(name, type, complex.Parameters, netlist);

                    if (model != null)
                    {
                        netlist.Circuit.Objects.Add(spiceSharpModel);
                    }
                }
            }
        }
    }
}
