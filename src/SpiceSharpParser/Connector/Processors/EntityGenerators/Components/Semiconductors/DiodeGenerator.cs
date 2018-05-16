using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;


namespace SpiceSharpParser.Connector.Processors.EntityGenerators.Components.Semiconductors
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

            var model = context.FindModel<DiodeModel>(parameters.GetString(2));
            if (model == null)
            {
                throw new ModelNotFoundException($"Could not find model {parameters.GetString(2)} for diode {name}");
            }

            diode.SetModel(model);

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
                    //TODO: Fix this please it's broken ...
                    var bp = diode.ParameterSets.Get<SpiceSharp.Components.DiodeBehaviors.BaseParameters>();
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
                }
            }

            return diode;
        }

        /// <summary>
        /// Gets generated Spice types by generator
        /// </summary>
        /// <returns>
        /// Generated Spice types
        /// </returns>
        public override IEnumerable<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "d" };
        }
    }
}
