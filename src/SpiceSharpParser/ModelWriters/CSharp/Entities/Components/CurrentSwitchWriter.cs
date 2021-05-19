using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class CurrentSwitchWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component component, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            var pins = component.PinsAndParameters.Take(CurrentSwitch.CurrentSwitchPinCount);
            var parameters = component.PinsAndParameters.Skip(CurrentSwitch.CurrentSwitchPinCount);
            var name = component.Name;

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
