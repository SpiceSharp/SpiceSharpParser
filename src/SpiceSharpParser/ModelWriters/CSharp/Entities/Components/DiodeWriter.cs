using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class DiodeWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < Diode.PinCount + 1)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(Diode.PinCount);
            var parameters = @object.PinsAndParameters.Skip(Diode.PinCount);
            var name = @object.Name;

            var diodeId = context.GetNewIdentifier(name);
            var modelName = parameters[0].Value;
            result.Add(new CSharpNewStatement(diodeId, $@"new Diode(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", ""{modelName}"")"));

            bool areaSet = false;

            // Read the rest of the parameters
            for (int i = 1; i < parameters.Count; i++)
            {
                if (parameters[i] is WordParameter w)
                {
                    if (w.Value.ToLower() == "on")
                    {
                        result.Add(SetParameter(diodeId, "off", false, context));
                    }
                    else if (w.Value.ToLower() == "off")
                    {
                        result.Add(SetParameter(diodeId, "off", true, context));
                    }
                    else
                    {
                        result.Add(new CSharpComment("Wrong parameter for diode:" + w.Value));
                    }
                }

                if (parameters[i] is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        result.Add(SetParameter(diodeId, "ic", asg.Value, context));
                    }
                }

                if (parameters[i] is ValueParameter || parameters[i] is ExpressionParameter)
                {
                    if (!areaSet)
                    {
                        result.Add(SetParameter(diodeId, "area", parameters[i].Value, context));
                        areaSet = true;
                    }
                    else
                    {
                        result.Add(SetParameter(diodeId, "temp", parameters[i].Value, context));
                    }
                }

                if (parameters[i] is AssignmentParameter assignmentParameter)
                {
                    if (assignmentParameter.Name.ToLower() != "m")
                    {
                        result.Add(SetParameter(diodeId, assignmentParameter.Name, assignmentParameter.Value, context));
                    }
                }
            }

            SetParallelParameter(result, diodeId, parameters, context);

            return result;
        }
    }
}
