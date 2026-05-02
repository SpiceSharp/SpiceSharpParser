using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    public class ArbitraryBehavioralGenerator : SourceGenerator
    {
        public override IEntity Generate(
            string componentIdentifier,
            string originalName,
            string type,
            ParameterCollection parameters,
            IReadingContext context)
        {
            if (parameters.Any(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "v"))
            {
                var expressionParameter = (AssignmentParameter)parameters.First(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "v");

                if (TryCreateLaplaceFunctionSource(
                    componentIdentifier,
                    originalName,
                    parameters,
                    context,
                    LaplaceOutputKind.Voltage,
                    expressionParameter.Value,
                    expressionParameter,
                    out var laplaceEntity))
                {
                    return laplaceEntity;
                }

                return CreateBehavioralVoltageSource(
                    componentIdentifier,
                    parameters,
                    context,
                    context.EvaluationContext,
                    expressionParameter.Value);
            }

            if (parameters.Any(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "i"))
            {
                var expressionParameter = (AssignmentParameter)parameters.First(p =>
                    p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "i");

                if (TryCreateLaplaceFunctionSource(
                    componentIdentifier,
                    originalName,
                    parameters,
                    context,
                    LaplaceOutputKind.Current,
                    expressionParameter.Value,
                    expressionParameter,
                    out var laplaceEntity))
                {
                    return laplaceEntity;
                }

                return CreateBehavioralCurrentSource(
                    componentIdentifier,
                    parameters,
                    context,
                    context.EvaluationContext,
                    expressionParameter.Value);
            }

            return null;
        }
    }
}
