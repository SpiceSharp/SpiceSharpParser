using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class JFETGenerator : IComponentGenerator
    {
        public IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 4)
            {
                throw new System.Exception("Model expected");
            }

            JFET jfet = new JFET(componentIdentifier);
            context.CreateNodes(jfet, parameters);

            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel(
                    jfet,
                    simulation,
                    parameters.Get(3),
                    $"Could not find model {parameters.Get(3)} for JFET {originalName}",
                    (model) => jfet.Model = model.Name,
                    context);
            });

            // Read the rest of the parameters
            for (int i = 3; i < parameters.Count; i++)
            {
                if (parameters[i] is WordParameter w)
                {
                    if (w.Value.ToLower() == "off")
                    {
                        jfet.SetParameter("off", true);
                    }
                }

                if (parameters[i] is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        if (asg.Values.Count == 2)
                        {
                            context.SetParameter(jfet, "ic-vds", asg.Values[0]);
                            context.SetParameter(jfet, "ic-vgs", asg.Values[1]);
                        }

                        if (asg.Values.Count == 1)
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
                        context.SetParameter(jfet, asg.Name, asg.Value);
                    }
                }

                if (parameters[i] is ValueParameter || parameters[i] is ExpressionParameter)
                {
                    context.SetParameter(jfet, "area", parameters[i].Value);
                }
            }

            return jfet;
        }
    }
}