using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class BipolarJunctionTransistorWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < 4)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(BipolarJunctionTransistor.PinCount);
            var parameters = @object.PinsAndParameters.Skip(BipolarJunctionTransistor.PinCount);
            var name = @object.Name;

            var bjtId = context.GetNewIdentifier(name);
            var modelName = parameters[0].Value;

            result.Add(new CSharpNewStatement(
                bjtId,
                $@"new BipolarJunctionTransistor(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", ""{pins[2].Value}"", ""{pins[3].Value}"", ""{modelName}"")"));

            bool areaSet = false;
            for (int i = 1; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                if (parameter is SingleParameter s)
                {
                    if (s is WordParameter)
                    {
                        switch (s.Value.ToLower())
                        {
                            case "on": result.Add(SetParameter(bjtId, "off", false, context)); break;
                            case "off": result.Add(SetParameter(bjtId, "off", true, context)); break;
                            default:
                                result.Add(new CSharpComment("Unsupported parameter" + s.Value));
                                break;
                        }
                    }
                    else
                    {
                        if (!areaSet)
                        {
                            result.Add(SetParameter(bjtId, "area", s.Value, context));
                            areaSet = true;
                        }
                        else
                        {
                            result.Add(SetParameter(bjtId, "temp", s.Value, context));
                        }
                    }
                }

                if (parameter is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        if (asg.Value.Length == 2)
                        {
                            result.Add(SetParameter(bjtId, "icvbe", asg.Values[0], context));
                            result.Add(SetParameter(bjtId, "icvce", asg.Values[1], context));
                        }

                        if (asg.Value.Length == 1)
                        {
                            result.Add(SetParameter(bjtId, "icvbe", asg.Values[0], context));
                        }
                    }
                    else
                    {
                        if (asg.Name.ToLower() != "m")
                        {
                            result.Add(SetParameter(bjtId, asg.Name, asg.Value, context));
                        }
                    }
                }
            }

            SetParallelParameter(result, bjtId, parameters, context);

            return result;
        }
    }
}