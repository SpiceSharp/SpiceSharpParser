using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using Model = SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models.Model;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Creates custom ideal diode instances for ideal diode models.
    /// </summary>
    public class IdealDiodeGenerator : IComponentGenerator
    {
        private readonly DiodeGenerator _fallback = new DiodeGenerator();

        /// <inheritdoc />
        public IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 3)
            {
                throw new Exception("Model expected");
            }

            var rangePredicate = CreateRangePredicate(parameters, context, originalName, false);
            var contextModel = context.ModelsRegistry.FindModel(
                parameters.Get(2).Value,
                selectedModel => IsIdealDiodeModel(selectedModel, rangePredicate));
            if (!(contextModel?.Entity is IdealDiodeModel))
            {
                return _fallback.Generate(componentIdentifier, originalName, type, parameters, context);
            }

            var diode = new IdealDiode(componentIdentifier);
            context.CreateNodes(diode, parameters.Take(IdealDiode.PinCount));
            diode.Model = contextModel.Name;
            diode.SetModelParameters(null, ((IdealDiodeModel)contextModel.Entity).Parameters);

            context.SimulationPreparations.ExecuteActionBeforeSetup(simulation =>
            {
                var currentRangePredicate = CreateRangePredicate(parameters, context, originalName, true);

                context.ModelsRegistry.SetModel(
                    diode,
                    selectedModel => IsIdealDiodeModel(selectedModel, currentRangePredicate),
                    simulation,
                    parameters.Get(2),
                    $"Could not find ideal diode model {parameters.Get(2)} for diode {originalName}",
                    selectedModel => SetSelectedModel(diode, selectedModel, parameters.Get(2), simulation, context),
                    context);
            });

            bool areaSet = false;
            for (int i = 3; i < parameters.Count; i++)
            {
                if (parameters[i] is WordParameter word)
                {
                    if (word.Value.Equals("on", StringComparison.OrdinalIgnoreCase))
                    {
                        diode.SetParameter("off", false);
                    }
                    else if (word.Value.Equals("off", StringComparison.OrdinalIgnoreCase))
                    {
                        diode.SetParameter("off", true);
                    }
                    else
                    {
                        throw new Exception("Expected on/off for diode");
                    }
                }

                if (parameters[i] is AssignmentParameter assignment)
                {
                    if (assignment.Name.Equals("l", StringComparison.OrdinalIgnoreCase)
                        || assignment.Name.Equals("w", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (IdealDiodeParserSupport.SetInstanceParameter(context, diode, originalName, assignment))
                    {
                        diode.Parameters.MarkInstanceOverride(assignment.Name);
                    }
                }

                if (parameters[i] is ValueParameter || parameters[i] is ExpressionParameter)
                {
                    if (!areaSet)
                    {
                        if (IdealDiodeParserSupport.SetPositionalAreaParameter(context, diode, parameters.Get(i)))
                        {
                            diode.Parameters.MarkInstanceOverride("area");
                        }

                        areaSet = true;
                    }
                }
            }

            return diode;
        }

        private static bool IsIdealDiodeModel(Model model, Func<Model, bool> rangePredicate)
        {
            return model.Entity is IdealDiodeModel && (rangePredicate == null || rangePredicate(model));
        }

        private static Func<Model, bool> CreateRangePredicate(
            ParameterCollection parameters,
            IReadingContext context,
            string diodeName,
            bool reportErrors)
        {
            try
            {
                double? l = ComponentGenerator.GetAssignmentParameterValue("l", parameters, context);
                double? w = ComponentGenerator.GetAssignmentParameterValue("w", parameters, context);
                return ComponentGenerator.CreateRangePredicate(("l", l), ("w", w));
            }
            catch (Exception ex)
            {
                if (reportErrors)
                {
                    var parameter = parameters
                        .OfType<AssignmentParameter>()
                        .FirstOrDefault(assignment => assignment.Name.Equals("l", StringComparison.OrdinalIgnoreCase)
                            || assignment.Name.Equals("w", StringComparison.OrdinalIgnoreCase))
                        ?? parameters.Get(2);

                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Could not evaluate ideal diode model selection parameter L/W for diode {diodeName}.",
                        parameter.LineInfo,
                        ex);
                }

                return null;
            }
        }

        private static void SetSelectedModel(
            IdealDiode diode,
            Model selectedModel,
            Parameter modelParameter,
            ISimulationWithEvents simulation,
            IReadingContext context)
        {
            if (selectedModel.Entity is IdealDiodeModel idealDiodeModel)
            {
                diode.Model = selectedModel.Name;
                diode.SetModelParameters(simulation, idealDiodeModel.Parameters);
                ApplyModelParameterSweepOverrides(diode, selectedModel, idealDiodeModel, modelParameter, simulation, context);
                return;
            }

            context.Result.ValidationResult.AddError(
                ValidationEntrySource.Reader,
                $"Selected model '{selectedModel.Name}' is not an ideal diode model for '{diode.Name}'.",
                modelParameter.LineInfo);
        }

        private static void ApplyModelParameterSweepOverrides(
            IdealDiode diode,
            Model selectedModel,
            IdealDiodeModel model,
            Parameter modelParameter,
            ISimulationWithEvents simulation,
            IReadingContext context)
        {
            var comparison = context.ReaderSettings.CaseSensitivity.IsEntityNamesCaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            var originalValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var sweep in context.SimulationConfiguration.ParameterSweeps)
            {
                if (!(sweep.Parameter is BracketParameter bracketParameter)
                    || bracketParameter.Parameters.Count == 0
                    || !MatchesModelName(bracketParameter.Name, modelParameter.Value, selectedModel.Name, comparison))
                {
                    continue;
                }

                var parameterName = bracketParameter.Parameters[0].Value;
                if (!IdealDiodeParserSupport.IsModelParameter(parameterName))
                {
                    continue;
                }

                if (TryGetSweepValue(context, simulation, sweep.Parameter, out double value))
                {
                    if (!originalValues.ContainsKey(parameterName)
                        && model.Parameters.TryGetProperty<double>(parameterName, out double originalValue))
                    {
                        originalValues.Add(parameterName, originalValue);
                    }

                    diode.SetModelParameterOverride(simulation, parameterName, value);
                }
            }

            if (originalValues.Count > 0)
            {
                simulation.EventBeforeUnSetup += (_, _) =>
                {
                    foreach (var originalValue in originalValues)
                    {
                        model.Parameters.SetParameter(originalValue.Key, originalValue.Value);
                    }
                };
            }
        }

        private static bool MatchesModelName(
            string sweepModelName,
            string requestedModelName,
            string selectedModelName,
            StringComparison comparison)
        {
            return string.Equals(sweepModelName, requestedModelName, comparison)
                || string.Equals(sweepModelName, selectedModelName, comparison)
                || string.Equals(sweepModelName, GetBaseModelName(selectedModelName), comparison);
        }

        private static string GetBaseModelName(string modelName)
        {
            var separatorIndex = modelName.IndexOf('#');
            return separatorIndex >= 0 ? modelName.Substring(0, separatorIndex) : modelName;
        }

        private static bool TryGetSweepValue(
            IReadingContext context,
            ISimulationWithEvents simulation,
            Parameter sweepParameter,
            out double value)
        {
            value = 0.0;
            var simulationContext = context.EvaluationContext.GetSimulationContext(simulation);
            if (!simulationContext.Parameters.TryGetValue(sweepParameter.Value, out var expression))
            {
                return false;
            }

            value = simulationContext.Evaluator.EvaluateDouble(expression);
            return true;
        }
    }
}
