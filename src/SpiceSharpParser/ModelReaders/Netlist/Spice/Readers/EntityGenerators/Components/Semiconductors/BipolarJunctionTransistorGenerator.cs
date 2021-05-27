using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class BipolarJunctionTransistorGenerator : IComponentGenerator
    {
        public IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 4)
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Wrong parameters count for BJT", parameters.LineInfo);
                return null;
            }

            var modelParameter = parameters.Skip(3).FirstOrDefault(p => p is WordParameter ag && context.ModelsRegistry.FindModel(ag.Value) != null);

            if (modelParameter == null)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Could not find model for bjt {originalName}",
                    parameters.LineInfo);
                return null;
            }

            if (parameters.IndexOf(modelParameter) != 4)
            {
                parameters.Insert(3, parameters[2]);
            }

            BipolarJunctionTransistor bjt = new BipolarJunctionTransistor(componentIdentifier);
            context.CreateNodes(bjt, parameters.Take(BipolarJunctionTransistor.PinCount));
            context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
            {
                context.ModelsRegistry.SetModel(
                    bjt,
                    simulation,
                    parameters.Get(4),
                    $"Could not find model {parameters.Get(4)} for BJT {originalName}",
                    (model) => bjt.Model = model.Name,
                    context);
            });

            bool areaSet = false;

            for (int i = 5; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                if (parameter is SingleParameter s)
                {
                    if (s is WordParameter)
                    {
                        switch (s.Value.ToLower())
                        {
                            case "on": bjt.SetParameter("off", false); break;
                            case "off": bjt.SetParameter("off", true); break;
                            default: throw new System.Exception();
                        }
                    }
                    else
                    {
                        if (!areaSet)
                        {
                            bjt.SetParameter("area", context.Evaluator.EvaluateDouble(s.Value));
                            areaSet = true;
                        }
                        else
                        {
                            bjt.SetParameter("temp", context.Evaluator.EvaluateDouble(s.Value));
                        }
                    }
                }

                if (parameter is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        if (asg.Values.Count == 2)
                        {
                            context.SetParameter(bjt, "icvbe", asg.Values[0]);
                            context.SetParameter(bjt, "icvce", asg.Values[1]);
                        }

                        if (asg.Values.Count == 1)
                        {
                            context.SetParameter(bjt, "icvbe", asg.Values[0]);
                        }
                    }
                    else
                    {
                        context.SetParameter(bjt, asg.Name, asg);
                    }
                }
            }

            return bjt;
        }
    }
}