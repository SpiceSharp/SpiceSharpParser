using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class JFETWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < JFET.PinCount + 1)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(JFET.PinCount);
            var parameters = @object.PinsAndParameters.Skip(JFET.PinCount);
            var name = @object.Name;

            var jfetId = context.GetNewIdentifier(name);
            var modelName = parameters[0].Value;

            result.Add(new CSharpNewStatement(jfetId, $@"new JFET(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", ""{pins[2].Value}"", ""{modelName}"")"));

            for (var i = 1; i < parameters.Count; i++)
            {
                if (parameters[i] is WordParameter w)
                {
                    if (w.Value.ToLower() == "off")
                    {
                        result.Add(SetParameter(jfetId, "off", true, context));
                    }
                }

                if (parameters[i] is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        if (asg.Value.Length == 2)
                        {
                            result.Add(SetParameter(jfetId, "ic-vds", asg.Values[0], context));
                            result.Add(SetParameter(jfetId, "ic-vgs", asg.Values[1], context));
                        }

                        if (asg.Value.Length == 1)
                        {
                            result.Add(SetParameter(jfetId, "ic-vds", asg.Values[0], context));
                        }
                    }
                    else if (asg.Name.ToLower() == "temp")
                    {
                        result.Add(SetParameter(jfetId, "temp", asg.Value, context));
                    }
                    else if (asg.Name.ToLower() == "area")
                    {
                        result.Add(SetParameter(jfetId, "area", asg.Value, context));
                    }
                    else
                    {
                        result.Add(SetParameter(jfetId, asg.Name, asg.Value, context));
                    }
                }

                if (parameters[i] is ValueParameter || parameters[i] is ExpressionParameter)
                {
                    result.Add(SetParameter(jfetId, "area", parameters[i].Value, context));
                }

                if (parameters[i] is AssignmentParameter assignmentParameter)
                {
                    if (assignmentParameter.Name.ToLower() != "m")
                    {
                        result.Add(SetParameter(jfetId, assignmentParameter.Name, assignmentParameter.Value, context));
                    }
                }
            }

            SetParallelParameter(result, jfetId, parameters, context);

            return result;
        }
    }
}
