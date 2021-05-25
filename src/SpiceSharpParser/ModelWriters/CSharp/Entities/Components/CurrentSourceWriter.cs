using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Linq;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class CurrentSourceWriter : BaseWriter, IWriter<Component>
    {
        public CurrentSourceWriter(WaveformWriter waveformWriter)
        {
            WaveformWriter = waveformWriter;
        }

        public WaveformWriter WaveformWriter { get; }

        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < CurrentSource.PinCount + 1)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(CurrentSource.PinCount);
            var parameters = @object.PinsAndParameters.Skip(CurrentSource.PinCount);
            var name = @object.Name;

            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value"))
            {
                var valueParameter = (AssignmentParameter)parameters.Single(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");
                string expression = valueParameter.Value;
                if (context.EvaluationContext.HaveSpiceProperties(expression) || context.EvaluationContext.HaveFunctions(expression))
                {
                    SourceWriterHelper.CreateBehavioralCurrentSource(result, name, pins, expression, context);
                    return result;
                }
            }

            if (parameters.Any(p => p is ExpressionParameter))
            {
                var expressionParameter = (ExpressionParameter)parameters.Single(p => p is ExpressionParameter);
                string expression = expressionParameter.Value;

                if (context.EvaluationContext.HaveSpiceProperties(expressionParameter.Value) || context.EvaluationContext.HaveFunctions(expressionParameter.Value))
                {
                    SourceWriterHelper.CreateBehavioralCurrentSource(result, name, pins, expression, context);
                    return result;
                }
            }

            var currentSourceId = context.GetNewIdentifier(name);
            result.Add(new CSharpNewStatement(currentSourceId, $@"new CurrentSource(""{name}"")"));
            result.Add(new CSharpCallStatement(currentSourceId, $@"Connect(""{pins[0].Value}"", ""{pins[1].Value}"")"));

            SourceWriterHelper.SetSourceParameters(this, WaveformWriter, result, currentSourceId, parameters, context, isCurrentSource: true);

            return result;
        }
    }
}
