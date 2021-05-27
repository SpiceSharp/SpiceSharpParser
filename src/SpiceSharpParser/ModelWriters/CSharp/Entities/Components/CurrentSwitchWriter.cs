using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class CurrentSwitchWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < CurrentSwitch.CurrentSwitchPinCount + 1)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(CurrentSwitch.CurrentSwitchPinCount);
            var parameters = @object.PinsAndParameters.Skip(CurrentSwitch.CurrentSwitchPinCount);
            var name = @object.Name;

            var voltageSwitchId = context.GetNewIdentifier(name);
            result.Add(new CSharpNewStatement(voltageSwitchId, $@"new CurrentSwitch(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", ""{parameters[0].Value}"")"));

            var model = parameters[1].Value;
            result.Add(new CSharpAssignmentStatement(voltageSwitchId + ".Model", $@"""{model}"""));

            for (var i = 2; i < parameters.Count; i++)
            {
                if (parameters[i] is AssignmentParameter assignmentParameter)
                {
                    result.Add(SetParameter(voltageSwitchId, assignmentParameter.Name, assignmentParameter.Value, context));
                }

                if (parameters[i] is WordParameter wordParameter)
                {
                    switch (wordParameter.Value.ToLower())
                    {
                        case "on": result.Add(SetParameter(voltageSwitchId, "on", true, context)); break;
                        case "off": result.Add(SetParameter(voltageSwitchId, "off", true, context)); break;
                    }
                }
            }

            return result;
        }
    }
}
