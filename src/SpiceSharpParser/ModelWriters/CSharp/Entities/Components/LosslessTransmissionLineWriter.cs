using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class LosslessTransmissionLineWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < 5)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(LosslessTransmissionLine.PinCount);
            var parameters = @object.PinsAndParameters.Skip(LosslessTransmissionLine.PinCount);
            var name = @object.Name;

            var lineId = context.GetNewIdentifier(name);

            result.Add(new CSharpNewStatement(lineId, $@"new LosslessTransmissionLine(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", ""{pins[2].Value}"", ""{pins[3].Value}"")"));

            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    var paramName = ap.Name.ToLower();

                    if (paramName == "z0" || paramName == "zo")
                    {
                        result.Add(SetParameter(lineId, "z0", ap.Value, context));
                    }
                    else if (paramName == "f")
                    {
                        result.Add(SetParameter(lineId, "f", ap.Value, context));
                    }
                    else if (paramName == "td")
                    {
                        result.Add(SetParameter(lineId, "td", ap.Value, context));
                    }
                    else if (paramName == "reltol")
                    {
                        result.Add(SetParameter(lineId, "reltol", ap.Value, context));
                    }
                    else if (paramName == "abstol")
                    {
                        result.Add(SetParameter(lineId, "abstol", ap.Value, context));
                    }
                }
            }

            SetParallelParameter(result, lineId, parameters, context);

            return result;
        }
    }
}
