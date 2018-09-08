﻿using System;
using System.Collections.Generic;
using System.Threading;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators
{
    public class CreateSimulationWithStochasticModelsDecorator
    {
        private static readonly Dictionary<Entity, Dictionary<string, double>> LotValues = new Dictionary<Entity, Dictionary<string, double>>();

        public static Func<string, Control, IReadingContext, BaseSimulation> Decorate(IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation)
        {
            return (string name, Control control, IReadingContext context2) =>
            {
                var sim = createSimulation(name, control, context2);

                sim.BeforeExecute += (object s, BeforeExecuteEventArgs arg) =>
                {
                    var evaluator = context.SimulationContexts.GetSimulationEvaluator(sim);

                    foreach (var stochasticModels in context.StochasticModelsRegistry.GetStochasticModels())
                    {
                        var baseModel = stochasticModels.Key;
                        var componentModels = stochasticModels.Value;

                        foreach (var componentModel in componentModels)
                        {
                            var stochasticDevParameters = context.StochasticModelsRegistry.GetStochasticModelDevParameters(baseModel);

                            if (stochasticDevParameters != null)
                            {
                                SetModelDevModelParameters(context, sim, evaluator, componentModel, stochasticDevParameters);
                            }

                            var stochasticLotParameters = context.StochasticModelsRegistry.GetStochasticModelLotParameters(baseModel);
                            if (stochasticLotParameters != null)
                            {
                                SetModelLotModelParameters(context, sim, evaluator, baseModel, componentModel, stochasticLotParameters);
                            }
                        }
                    }
                };

                return sim;
            };
        }

        private static void SetModelLotModelParameters(IReadingContext context, BaseSimulation sim, IEvaluator evaluator, Entity baseModel, Entity componentModel, Dictionary<Parameter, Parameter> stochasticLotParameters)
        {
            foreach (var stochasticParameter in stochasticLotParameters)
            {
                var parameter = stochasticParameter.Key;
                var parameterPercent = stochasticParameter.Value;

                if (parameter is AssignmentParameter asg)
                {
                    var parameterName = asg.Name.ToLower();

                    var currentValueParameter = sim.EntityParameters[componentModel.Name].GetParameter<double>(parameterName);
                    var currentValue = currentValueParameter.Value;
                    var percentValue = evaluator.EvaluateDouble(parameterPercent.Image);
                    double newValue = GetValueForLotProperty(evaluator, baseModel, parameterName, currentValue, percentValue);
                    context.SimulationContexts.SetModelParameter(parameterName, componentModel, newValue.ToString(), sim);
                }
            }
        }

        private static void SetModelDevModelParameters(IReadingContext context, BaseSimulation sim, Common.IEvaluator evaluator, Entity componentModel, System.Collections.Generic.Dictionary<Parameter, Parameter> stochasticDevParameters)
        {
            foreach (var stochasticParameter in stochasticDevParameters)
            {
                var parameter = stochasticParameter.Key;
                var parameterPercent = stochasticParameter.Value;

                if (parameter is AssignmentParameter asg)
                {
                    var asgparamName = asg.Name.ToLower();
                    var currentValueParameter = sim.EntityParameters[componentModel.Name].GetParameter<double>(asgparamName);
                    var currentValue = currentValueParameter.Value;
                    var percentValue = evaluator.EvaluateDouble(parameterPercent.Image);

                    double newValue = GetValueForDevProperty(evaluator, currentValue, percentValue);
                    context.SimulationContexts.SetModelParameter(asgparamName, componentModel, newValue.ToString(), sim);
                }
            }
        }

        private static double GetValueForDevProperty(IEvaluator evaluator, double currentValue, double percentValue)
        {
            var random = Randomizer.GetRandom(evaluator.RandomSeed);

            double newValue = 0;
            if (random.Next() % 2 == 0)
            {
                newValue = currentValue + ((percentValue / 100.0) * currentValue * random.NextDouble());
            }
            else
            {
                newValue = currentValue - ((percentValue / 100.0) * currentValue * random.NextDouble());
            }

            return newValue;
        }

        private static double GetValueForLotProperty(IEvaluator evaluator, Entity baseModel, string parameterName, double currentValue, double percentValue)
        {
            if (LotValues.ContainsKey(baseModel) && LotValues[baseModel].ContainsKey(parameterName))
            {
                return LotValues[baseModel][parameterName];
            }

            var random = Randomizer.GetRandom(evaluator.RandomSeed);

            double newValue = 0;
            if (random.Next() % 2 == 0)
            {
                newValue = currentValue + ((percentValue / 100.0) * currentValue * random.NextDouble());
            }
            else
            {
                newValue = currentValue - ((percentValue / 100.0) * currentValue * random.NextDouble());
            }

            if (!LotValues.ContainsKey(baseModel))
            {
                LotValues[baseModel] = new Dictionary<string, double>();
            }

            LotValues[baseModel][parameterName] = newValue;

            return newValue;
        }
    }
}