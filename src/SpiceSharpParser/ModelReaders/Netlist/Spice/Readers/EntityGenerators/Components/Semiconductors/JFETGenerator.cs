using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class JFETGenerator : IComponentGenerator
    {
        public IEnumerable<string> GeneratedTypes => new List<string>() { "J" };

        public SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 4)
            {
                throw new System.Exception("Model expected");
            }

            JFET jfet = new JFET(componentIdentifier);
            context.CreateNodes(jfet, parameters);

            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel<JFETModel>(
                    jfet,
                    simulation,
                    parameters.GetString(3),
                    $"Could not find model {parameters.GetString(3)} for JFET {originalName}",
                    (JFETModel model) => jfet.Model = model.Name,
                    context.Result);
            });

            // Read the rest of the parameters
            for (int i = 3; i < parameters.Count; i++)
            {
                if (parameters[i] is WordParameter w)
                {
                    if (w.Image.ToLower() == "off")
                    {
                        jfet.SetParameter("off", true);
                    }
                }

                if (parameters[i] is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        if (asg.Value.Length == 2)
                        {
                            context.SetParameter(jfet, "ic-vds", asg.Values[0]);
                            context.SetParameter(jfet, "ic-vgs", asg.Values[1]);
                        }

                        if (asg.Value.Length == 1)
                        {
                            context.SetParameter(jfet, "ic-vds", asg.Values[0]);
                        }
                    }
                    else if (asg.Name.ToLower() == "temp")
                    {
                        context.SetParameter(jfet, "temp", asg.Value);
                    }
                    else if (asg.Name.ToLower() == "area")
                    {
                        context.SetParameter(jfet, "area", asg.Value);
                    }
                    else
                    {
                        throw new System.Exception("Unknown parameter: " + asg.Name);
                    }
                }

                if (parameters[i] is ValueParameter v1 || parameters[i] is ExpressionParameter v2)
                {
                    context.SetParameter(jfet, "area", parameters[i].Image);
                }
            }

            return jfet;
        }
    }
}
