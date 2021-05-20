using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Linq;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class VoltageControlledVoltageSourceWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();
            var sourceId = context.GetNewIdentifier(@object.Name);

            if (@object.PinsAndParameters.Count == 5
                && @object.PinsAndParameters.IsValueString(4))
            {
                var pins = @object.PinsAndParameters.Take(VoltageControlledCurrentSource.PinCount);
                var parameters = @object.PinsAndParameters.Skip(VoltageControlledCurrentSource.PinCount);

                double gain = context.EvaluationContext.Evaluate(parameters.Get(0));

                result.Add(new CSharpNewStatement(
                    sourceId,
                    $@"new VoltageControlledVoltageSource(""{@object.Name}"", ""{pins[0].Value}"", ""{pins[1].Value}"",""{pins[2].Value}"", ""{pins[3].Value}"", {gain})"));
            }
            else
            {
                if (@object.PinsAndParameters.Count == 3
                    && @object.PinsAndParameters[0] is PointParameter pp1 && pp1.Values.Count() == 2
                    && @object.PinsAndParameters[1] is PointParameter pp2 && pp2.Values.Count() == 2)
                {
                    var vccsNodes = new ParameterCollection(new List<Parameter>());
                    vccsNodes.Add(pp1.Values.Items[0]);
                    vccsNodes.Add(pp1.Values.Items[1]);
                    vccsNodes.Add(pp2.Values.Items[0]);
                    vccsNodes.Add(pp2.Values.Items[1]);

                    double gain = context.EvaluationContext.Evaluate(@object.PinsAndParameters.Get(4));

                    result.Add(new CSharpNewStatement(
                        sourceId,
                        $@"new VoltageControlledVoltageSource(""{@object.Name}"", ""{vccsNodes[0].Value}"", ""{vccsNodes[1].Value}"",""{vccsNodes[2].Value}"",""{vccsNodes[3].Value}"", {gain})"));
                }
                else
                {
                    SourceWriterHelper.CreateCustomVoltageSource(result, @object.Name, @object.PinsAndParameters, context, true);
                }
            }

            return result;
        }
    }
}
