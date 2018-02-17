using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ComponentProcessor : StatementProcessor<Component>
    {
        private readonly ModelProcessor modelProcessor;

        // TODO: Refactor
        protected List<EntityGenerator> Generators = new List<EntityGenerator>();
        protected Dictionary<string, EntityGenerator> GeneratorsByType = new Dictionary<string, EntityGenerator>();

        public ComponentProcessor(ModelProcessor modelProcessor)
        {
            this.modelProcessor = modelProcessor;

            var waveFormsGenerators = new EntityGenerators.Components.Waveforms.WaveformsGenerator();

            Generators.Add(new RLCGenerator());
            Generators.Add(new VoltageSourceGenerator(waveFormsGenerators));
            Generators.Add(new BipolarJunctionTransistorGenerator());
            Generators.Add(new DiodeGenerator());
            Generators.Add(new SubCircuitGenerator(this, modelProcessor));
            Generators.Add(new CurrentSourceGenerator(waveFormsGenerators));
            Generators.Add(new SwitchGenerator());

            foreach (var generator in Generators)
            {
                foreach (var type in generator.GetGeneratedTypes())
                {
                    GeneratorsByType[type] = generator;
                }
            }
        }

        public override void Process(Component statement, ProcessingContext context)
        {
            string name = statement.Name.ToLower();
            string type = name[0].ToString();

            if (!GeneratorsByType.ContainsKey(type))
            {
                throw new System.Exception("Unsupported component type");
            }

            var generator = GeneratorsByType[type];

            Entity entity = generator.Generate(
                new Identifier(context.GenerateObjectName(name)),
                name,
                type,
                statement.PinsAndParameters,
                context);

            if (entity != null)
            {
                context.AddEntity(entity);
            }
        }
    }
}
