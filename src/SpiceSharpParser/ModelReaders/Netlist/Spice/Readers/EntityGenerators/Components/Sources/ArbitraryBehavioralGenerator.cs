using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    public class ArbitraryBehavioralGenerator : SourceGenerator
    {
        public override SpiceSharp.Components.Component Generate(
            string componentIdentifier,
            string originalName,
            string type,
            ParameterCollection parameters,
            IReadingContext context)
        {
            if (parameters.Any(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "v"))
            {
                var entity = new BehavioralVoltageSource(componentIdentifier);
                context.CreateNodes(entity, parameters);

                var expressionParameter = (AssignmentParameter)parameters.First(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "v");
                var baseParameters = entity.ParameterSets.Get<SpiceSharpBehavioral.Components.BehavioralBehaviors.BaseParameters>();
                baseParameters.Expression = expressionParameter.Value;
                baseParameters.Parser = (sim) => CreateParser(context, sim);
                return entity;
            }

            if (parameters.Any(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "i"))
            {
                var entity = new BehavioralCurrentSource(componentIdentifier);
                context.CreateNodes(entity, parameters);

                var expressionParameter = (AssignmentParameter)parameters.First(p =>
                    p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "i");

                var baseParameters = entity.ParameterSets.Get<SpiceSharpBehavioral.Components.BehavioralBehaviors.BaseParameters>();
                baseParameters.Expression = expressionParameter.Value;
                baseParameters.Parser = (sim) => CreateParser(context, sim);

                return entity;
            }

            return null;
        }
    }
}