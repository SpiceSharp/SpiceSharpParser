using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class BipolarJunctionTransistorGenerator : IComponentGenerator
    {
        public IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
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
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Wrong parameters count for BJT", parameters.LineInfo));
                return null;
            }

            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel(
                    bjt,
                    simulation,
                    parameters.Get(4),
                    $"Could not find model {parameters.Get(4)} for BJT {originalName}",
                    (Context.Models.Model model) => bjt.Model = model.Name,
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
                        // TODO: Verify it
                        var bp = bjt.Parameters;
                        if (bp.Area == 0.0)
                        {
                            bp.Area = context.Evaluator.EvaluateDouble(s.Image);
                        }

                        if (!bp.Temperature.Given)
                        {
                            bp.Temperature = context.Evaluator.EvaluateDouble(s.Image);
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