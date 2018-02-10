using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components;
using SpiceSharp.Circuits;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    class ComponentProcessor : StatementProcessor
    {
        protected List<EntityGenerator> Generators = new List<EntityGenerator>();

        public string NamePrefix { get; internal set; }

        public override void Init()
        {
            Generators.Add(new RLCGenerator());
            Generators.Add(new VoltageSourceGenerator(new EntityGenerators.Components.Waveforms.WaveformsGenerator()));
            Generators.Add(new BipolarJunctionTransistorGenerator());
            Generators.Add(new SubCircuitGenerator(this));
        }

        public override void Process(Statement statement, NetList netlist)
        {
            Component c = statement as Component;
            string name = c.Name.ToLower();
            string type = name[0].ToString();

            foreach (var generator in Generators)
            {
                if (generator.GetGeneratedTypes().Contains(type))
                {
                    //TODO: hack 
                    Entity entity = generator.Generate((NamePrefix ?? "") + name, type, c.Parameters, netlist);
                    if (entity != null)
                    {
                        netlist.Circuit.Objects.Add(entity);
                    }
                }
            }
        }
    }
}
