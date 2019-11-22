using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class DiodeGenerator : IComponentGenerator
    {
        public SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            if (parameters.Count < 3)
            {
                throw new System.Exception("Model expected");
            }

            Diode diode = new Diode(componentIdentifier);
            context.CreateNodes(diode, parameters);

            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel(
                    diode,
                    simulation,
                    parameters.Get(2).Image,
                    $"Could not find model {parameters.Get(2)} for diode {originalName}",
                    (DiodeModel model) => diode.Model = model.Name,
                    context.Result);
            });

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
                        context.SetParameter(diode, "ic", asg.Value);
                    }
                }

                if (parameters[i] is ValueParameter || parameters[i] is ExpressionParameter)
                {
                    // TODO: Fix this please it's broken ...
                    var bp = diode.ParameterSets.Get<SpiceSharp.Components.DiodeBehaviors.BaseParameters>();
                    if (!bp.Area.Given)
                    {
                        bp.Area.Value = context.CircuitEvaluator.EvaluateDouble(parameters.Get(i));
                    }
                    else
                    {
                        if (!bp.Temperature.Given)
                        {
                            bp.Temperature.Value = context.CircuitEvaluator.EvaluateDouble(parameters.Get(i));
                        }
                    }
                }
            }

            return diode;
        }
    }
}