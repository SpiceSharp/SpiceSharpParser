using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    public class DiodeGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier name, string originalName, string type, ParameterCollection parameters, IProcessingContext context)
        {
            if (parameters.Count < 3)
            {
                throw new System.Exception("Model expected");
            }

            Diode diode = new Diode(name);
            context.CreateNodes(diode, parameters);
            diode.SetModel(context.FindModel<DiodeModel>(parameters.GetString(2)));

            // Read the rest of the parameters
            for (int i = 3; i < parameters.Count; i++)
            {
                if (parameters[i] is WordParameter w)
                {
                    if (w.Image.ToLower() == "on")
                    {
                        diode.SetParameter("off", false);
                    }
                    else if (w.Image.ToLower() == "off")
                    {
                        diode.SetParameter("off", true);
                    }
                    else
                    {
                        throw new System.Exception("Expected on/off for diode");
                    }
                }

                if (parameters[i] is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        diode.SetParameter("ic", context.ParseDouble(asg.Value));
                    }
                }

                if (parameters[i] is ValueParameter v1 || parameters[i] is ExpressionParameter v2)
                {
                    var bp = diode.ParameterSets[typeof(SpiceSharp.Components.DiodeBehaviors.BaseParameters)] as SpiceSharp.Components.DiodeBehaviors.BaseParameters;
                    if (!bp.Area.Given)
                    {
                        bp.Area.Value = context.ParseDouble(parameters.GetString(i));
                    }
                    else
                    {
                        if (!bp.Temperature.Given)
                        {
                            bp.Temperature.Value = context.ParseDouble(parameters.GetString(i));
                        }
                    }

                    throw new System.Exception("Unsupported yet..");
                }
            }

            return diode;
        }

        public override List<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "d" };
        }
    }
}
