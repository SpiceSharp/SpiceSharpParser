using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class VoltageSwitchWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < 5)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(VoltageSwitch.VoltageSwitchPinCount);
            var parameters = @object.PinsAndParameters.Skip(VoltageSwitch.VoltageSwitchPinCount);
            var name = @object.Name;

            var voltageSwitchId = context.GetNewIdentifier(name);
            result.Add(new CSharpNewStatement(voltageSwitchId, $@"new VoltageSwitch(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", ""{pins[2].Value}"", ""{pins[3].Value}"", ""{parameters[0].Value}"")"));

            for (var i = 1; i < parameters.Count; i++)
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
