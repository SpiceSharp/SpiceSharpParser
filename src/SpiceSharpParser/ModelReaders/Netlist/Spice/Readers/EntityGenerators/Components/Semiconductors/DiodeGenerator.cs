using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Linq;

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
                double? l = GetLengthFromParameters(parameters, context);
                double? w = GetWidthFromParameters(parameters, context);

                context.ModelsRegistry.SetModel(
                    diode,
                    l,
                    w,
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
                    // Skip L and W parameters - they are used for model selection only
                    if (asg.Name.ToLower() == "l" || asg.Name.ToLower() == "w")
                    {
                        continue;
                    }
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

        private double? GetLengthFromParameters(ParameterCollection parameters, IReadingContext context)
        {
            var lParameter = parameters.FirstOrDefault(p => p is AssignmentParameter ap && ap.Name.ToLower() == "l");
            if (lParameter != null && lParameter is AssignmentParameter lap)
            {
                return context.Evaluator.EvaluateDouble(lap.Value);
            }
            return null;
        }

        private double? GetWidthFromParameters(ParameterCollection parameters, IReadingContext context)
        {
            var wParameter = parameters.FirstOrDefault(p => p is AssignmentParameter ap && ap.Name.ToLower() == "w");
            if (wParameter != null && wParameter is AssignmentParameter wap)
            {
                return context.Evaluator.EvaluateDouble(wap.Value);
            }
            return null;
        }
    }
}