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
                var entity = new BehavioralVoltageSource(componentIdentifier);
                context.CreateNodes(entity, parameters);

                var expressionParameter = (AssignmentParameter)parameters.First(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "v");
                entity.Parameters.Expression = expressionParameter.Value;
                entity.Parameters.ParseAction = (expression) =>
                {
                    var parser = context.CreateExpressionParser(null);
                    return parser.Resolve(expression);
                };
                return entity;
            }

            if (parameters.Any(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "i"))
            {
                var entity = new BehavioralCurrentSource(componentIdentifier);
                context.CreateNodes(entity, parameters);

                var expressionParameter = (AssignmentParameter)parameters.First(p =>
                    p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "i");

                var mParameter = (AssignmentParameter)parameters.FirstOrDefault(p =>
                    p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "m");

                if (mParameter != null)
                {
                    entity.Parameters.Expression = $"({expressionParameter.Value}) * ({mParameter.Value})";
                }
                else
                {
                    entity.Parameters.Expression = expressionParameter.Value;
                }

                entity.Parameters.ParseAction = (expression) =>
                {
                    var parser = context.CreateExpressionParser(null);
                    return parser.Resolve(expression);
                };
                return entity;
            }

            return null;
        }
    }
}