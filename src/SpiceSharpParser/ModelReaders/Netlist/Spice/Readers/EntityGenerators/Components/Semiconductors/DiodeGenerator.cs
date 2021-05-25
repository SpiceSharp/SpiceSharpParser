using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class DiodeGenerator : IComponentGenerator
    {
        public IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 3)
            {
                throw new System.Exception("Model expected");
            }

            Diode diode = new Diode(componentIdentifier);
            context.CreateNodes(diode, parameters.Take(Diode.PinCount));

            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel(
                    diode,
                    simulation,
                    parameters.Get(2),
                    $"Could not find model {parameters.Get(2)} for diode {originalName}",
                    (model) => diode.Model = model.Name,
                    context);
            });

            bool areaSet = false;
            // Read the rest of the parameters
            for (int i = 3; i < parameters.Count; i++)
            {
                if (parameters[i] is WordParameter w)
                {
                    if (w.Value.ToLower() == "on")
                    {
                        diode.SetParameter("off", false);
                    }
                    else if (w.Value.ToLower() == "off")
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
                    context.SetParameter(diode, asg.Name, asg);
                }

                if (parameters[i] is ValueParameter || parameters[i] is ExpressionParameter)
                {
                    if (!areaSet)
                    {
                        context.SetParameter(diode, "area", parameters.Get(i));
                        areaSet = true;
                    }
                    else
                    {
                        context.SetParameter(diode, "temp", parameters.Get(i));
                    }
                }
            }

            return diode;
        }
    }
}