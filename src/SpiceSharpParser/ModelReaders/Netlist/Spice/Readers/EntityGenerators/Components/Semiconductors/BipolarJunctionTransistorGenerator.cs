using SpiceSharp.Components;
using SpiceSharp.Components.BipolarBehaviors;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class BipolarJunctionTransistorGenerator : IComponentGenerator
    {
        public SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            BipolarJunctionTransistor bjt = new BipolarJunctionTransistor(componentIdentifier);

            // If the component is of the format QXXX NC NB NE MNAME off we will insert NE again before the model name
            if (parameters.Count == 5 && parameters[4] is WordParameter w && w.Image == "off")
            {
                parameters.Insert(3, parameters[2]);
            }

            // If the component is of the format QXXX NC NB NE MNAME we will insert NE again before the model name
            if (parameters.Count == 4)
            {
                parameters.Insert(3, parameters[2]);
            }

            context.CreateNodes(bjt, parameters);

            if (parameters.Count < 5)
            {
                throw new WrongParametersCountException();
            }

            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel(
                    bjt,
                    simulation,
                    parameters.Get(4).Image,
                    $"Could not find model {parameters.Get(4)} for BJT {originalName}",
                    (BipolarJunctionTransistorModel model) => bjt.Model = model.Name,
                    context.Result);
            });

            for (int i = 5; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                if (parameter is SingleParameter s)
                {
                    if (s is WordParameter)
                    {
                        switch (s.Image.ToLower())
                        {
                            case "on": bjt.SetParameter("off", false); break;
                            case "off": bjt.SetParameter("on", false); break;
                            default: throw new System.Exception();
                        }
                    }
                    else
                    {
                        // TODO: Fix this please it's broken ...
                        BaseParameters bp = bjt.ParameterSets.Get<BaseParameters>();
                        if (!bp.Area.Given)
                        {
                            bp.Area.Value = context.CircuitEvaluator.EvaluateDouble(s.Image);
                        }

                        if (!bp.Temperature.Given)
                        {
                            bp.Temperature.Value = context.CircuitEvaluator.EvaluateDouble(s.Image);
                        }
                    }
                }

                if (parameter is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        context.SetParameter(bjt, "ic", asg.Value);
                    }
                }
            }

            return bjt;
        }
    }
}
