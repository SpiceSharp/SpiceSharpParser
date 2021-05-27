using SpiceSharp.Components;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class CurrentControlledVoltageSourceWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();
            var sourceId = context.GetNewIdentifier(@object.Name);

            if (@object.PinsAndParameters.Count == 4
                && @object.PinsAndParameters.IsValueString(0)
                && @object.PinsAndParameters.IsValueString(1)
                && @object.PinsAndParameters.IsValueString(2) && @object.PinsAndParameters[2].Value.ToLower() != "value"
                && @object.PinsAndParameters.IsValueString(3))
            {
                var pins = @object.PinsAndParameters.Take(CurrentControlledVoltageSource.PinCount);
                var parameters = @object.PinsAndParameters.Skip(CurrentControlledVoltageSource.PinCount);

                double gain = context.EvaluationContext.Evaluate(parameters.Get(1));

                result.Add(new CSharpNewStatement(
                    sourceId,
                    $@"new CurrentControlledVoltageSource(""{@object.Name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", ""{parameters[0].Value}"", {gain})"));
            }
            else
            {
                SourceWriterHelper.CreateCustomVoltageSource(result, @object.Name, @object.PinsAndParameters, context, false);
            }

            return result;
        }
    }
}
