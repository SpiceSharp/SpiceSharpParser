using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class MosfetGenerator : ComponentGenerator
    {
        public MosfetGenerator()
        {
            // MOS1
            Mosfets.Add(typeof(Mosfet1Model), (string name) =>
            {
                var mosfet = new Mosfet1(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Context.Models.Model model) => mosfet.Model = model.Name };
            });

            // MOS2
            Mosfets.Add(typeof(Mosfet2Model), (string name) =>
            {
                var mosfet = new Mosfet2(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Context.Models.Model model) => mosfet.Model = model.Name };
            });

            // MOS3
            Mosfets.Add(typeof(Mosfet3Model), (string name) =>
            {
                var mosfet = new Mosfet3(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Context.Models.Model model) => mosfet.Model = model.Name };
            });
        }

        protected Dictionary<Type, Func<string, MosfetDetails>> Mosfets { get; } = new Dictionary<Type, Func<string, MosfetDetails>>();

        public override IEntity Generate(string componentIdentifier, string originalName, string type, SpiceSharpParser.Models.Netlist.Spice.Objects.ParameterCollection parameters, IReadingContext context)
        {
            // Errors
            switch (parameters.Count)
            {
                case 0:
                    context.Result.ValidationResult.Add(
                        new ValidationEntry(
                            ValidationEntrySource.Reader,
                            ValidationEntryLevel.Error,
                            $"Node expected for component {componentIdentifier}",
                            parameters.LineInfo));
                    return null;
                case 1:
                case 2:
                case 3:
                    context.Result.ValidationResult.Add(
                        new ValidationEntry(
                            ValidationEntrySource.Reader,
                            ValidationEntryLevel.Error,
                            $"Node expected",
                            parameters.LineInfo));
                    return null;
                case 4:
                    context.Result.ValidationResult.Add(
                        new ValidationEntry(
                            ValidationEntrySource.Reader,
                            ValidationEntryLevel.Error,
                            $"Model name expected",
                            parameters.LineInfo));
                    return null;
            }

            // Get the model and generate a component for it
            SpiceSharp.Components.Component mosfet = null;
            var modelNameParameter = parameters.Get(4);
            var model = context.ModelsRegistry.FindModel(modelNameParameter.Value);
            if (model == null)
            {
                context.Result.ValidationResult.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Error,
                        $"Could not find model {modelNameParameter} for mosfet {originalName}",
                        parameters.LineInfo));

                return null;
            }

            if (Mosfets.ContainsKey(model.GetType()))
            {
                var mosfetDetails = Mosfets[model.GetType()].Invoke(componentIdentifier);
                mosfet = mosfetDetails.Mosfet;

                context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                {
                    context.ModelsRegistry.SetModel(
                        mosfetDetails.Mosfet,
                        simulation,
                        modelNameParameter,
                        $"Could not find model {modelNameParameter} for mosfet {componentIdentifier}",
                        mosfetDetails.SetModelAction,
                        context);
                });
            }
            else
            {
                context.Result.ValidationResult.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Error,
                        $"Invalid model {model.GetType()} for {componentIdentifier}",
                        parameters.LineInfo));

                return null;
            }

            context.CreateNodes(mosfet, parameters);

            foreach (Parameter parameter in parameters.Skip(5))
            {
                if (parameter is AssignmentParameter ap)
                {
                    try
                    {
                        if (ap.Name.ToLower() == "ic")
                        {
                            if (ap.Values.Count == 3)
                            {
                                context.SetParameter(mosfet, "icvds", ap.Values[0], true);
                                context.SetParameter(mosfet, "icvgs", ap.Values[1], true);
                                context.SetParameter(mosfet, "icvbs", ap.Values[2], true);
                            }

                            if (ap.Values.Count == 2)
                            {
                                context.SetParameter(mosfet, "icvds", ap.Values[0], true);
                                context.SetParameter(mosfet, "icvgs", ap.Values[1], true);
                            }

                            if (ap.Values.Count == 1)
                            {
                                context.SetParameter(mosfet, "icvds", ap.Values[0], true);
                            }
                        }
                        else
                        {
                            context.SetParameter(mosfet, ap.Name, ap.Value, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Problem with setting parameter: {parameter}", parameter.LineInfo, ex));
                    }
                }
                else
                {
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Unsupported parameter: {parameter}", parameter.LineInfo));
                }
            }

            return mosfet;
        }

        protected class MosfetDetails
        {
            public SpiceSharp.Components.Component Mosfet { get; set; }

            public Action<Context.Models.Model> SetModelAction { get; set; }
        }
    }
}