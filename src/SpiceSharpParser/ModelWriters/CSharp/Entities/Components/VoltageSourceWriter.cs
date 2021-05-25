using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Linq;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class VoltageSourceWriter : BaseWriter, IWriter<Component>
    {
        public VoltageSourceWriter(WaveformWriter waveformWriter)
        {
            WaveformWriter = waveformWriter;
        }

        public WaveformWriter WaveformWriter { get; }

        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < 3)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(VoltageSource.PinCount);
            var parameters = @object.PinsAndParameters.Skip(VoltageSource.PinCount);
            var name = @object.Name;

            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value"))
            {
                var valueParameter = (AssignmentParameter)parameters.Single(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");
                string expression = valueParameter.Value;
                if (context.EvaluationContext.HaveSpiceProperties(expression) || context.EvaluationContext.HaveFunctions(expression))
                {
                    SourceWriterHelper.CreateBehavioralVoltageSource(result, name, pins, expression, context);
                    return result;
                }
            }

            if (parameters.Any(p => p is ExpressionParameter))
            {
                var expressionParameter = (ExpressionParameter)parameters.Single(p => p is ExpressionParameter);
                string expression = expressionParameter.Value;

                if (context.EvaluationContext.HaveSpiceProperties(expressionParameter.Value) || context.EvaluationContext.HaveFunctions(expressionParameter.Value))
                {
                    SourceWriterHelper.CreateBehavioralVoltageSource(result, name, pins, expression, context);
                    return result;
                }
            }

            var voltageId = context.GetNewIdentifier(name);
            result.Add(new CSharpNewStatement(voltageId, $@"new VoltageSource(""{name}"")"));
            result.Add(new CSharpCallStatement(voltageId, $@"Connect(""{pins[0].Value}"", ""{pins[1].Value}"")"));

            SourceWriterHelper.SetSourceParameters(this, WaveformWriter, result, voltageId, parameters, context, isCurrentSource: false);

            return result;
        }
    }
}
