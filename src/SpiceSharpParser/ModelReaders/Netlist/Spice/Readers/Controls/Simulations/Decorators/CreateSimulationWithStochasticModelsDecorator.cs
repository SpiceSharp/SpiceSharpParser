using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators
{
    public class CreateSimulationWithStochasticModelsDecorator
    {
        private static readonly ConcurrentDictionary<Entity, Dictionary<string, double>> LotValues = new ConcurrentDictionary<Entity, Dictionary<string, double>>();

        public static Func<string, Control, IReadingContext, BaseSimulation> Decorate(IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation)
        {
            return (string simulationName, Control control, IReadingContext context2) =>
            {
                var simulation = createSimulation(simulationName, control, context2);
                var modelsRegistry = context.ModelsRegistry as IStochasticModelsRegistry;

                if (modelsRegistry != null)
                {
                    simulation.BeforeExecute += (object sender, BeforeExecuteEventArgs arg) =>
                    {
                        foreach (var stochasticModels in modelsRegistry.GetStochasticModels())
                        {
                            var baseModel = stochasticModels.Key;
                            var componentModels = stochasticModels.Value;

                            foreach (var componentModel in componentModels)
                            {
                                Dictionary<Parameter, PercentSpecification> stochasticDevParameters = modelsRegistry.GetStochasticModelDevParameters(baseModel);

                                if (stochasticDevParameters != null)
                                {
                                    SetModelDevModelParameters(context, simulation, baseModel, componentModel, stochasticDevParameters);
                                }

                                Dictionary<Parameter, PercentSpecification> stochasticLotParameters = modelsRegistry.GetStochasticModelLotParameters(baseModel);
                                if (stochasticLotParameters != null)
                                {
                                    SetModelLotModelParameters(context, simulation,  baseModel, componentModel, stochasticLotParameters);
                                }
                            }
                        }
                    };
                }

                return simulation;
            };
        }

        private static void SetModelLotModelParameters(IReadingContext context, BaseSimulation sim, Entity baseModel, Entity componentModel, Dictionary<Parameter, PercentSpecification> stochasticLotParameters)
        {
            var comparer = StringComparerProvider.Get(context.CaseSensitivity.IsEntityParameterNameCaseSensitive);
            foreach (var stochasticParameter in stochasticLotParameters)
            {
                var parameter = stochasticParameter.Key;
                var parameterPercent = stochasticParameter.Value;

                if (parameter is AssignmentParameter asg)
                {
                    var evaluator = context.SimulationEvaluators.GetEvaluator(sim);

                    var parameterName = asg.Name;
                    var currentValueParameter = sim.EntityParameters[componentModel.Name].GetParameter<double>(parameterName, comparer);
                    var expressionContext = context.SimulationExpressionContexts.GetContext(sim);

                    var currentValue = currentValueParameter.Value;
                    var percentValue = evaluator.EvaluateValueExpression(parameterPercent.Parameter.Image, expressionContext);
                    double newValue = GetValueForLotParameter(expressionContext, baseModel, parameterName, currentValue, percentValue, parameterPercent.DistributionName, comparer);
                    context.SimulationPreparations.SetParameter(componentModel, sim, parameterName, newValue, true, false);
                }
            }
        }

        private static void SetModelDevModelParameters(IReadingContext context, BaseSimulation sim, Entity baseModel, Entity componentModel, Dictionary<Parameter, PercentSpecification> stochasticDevParameters)
        {
            var comparer = StringComparerProvider.Get(context.CaseSensitivity.IsEntityParameterNameCaseSensitive);
            foreach (var stochasticParameter in stochasticDevParameters)
            {
                var parameter = stochasticParameter.Key;
                var parameterPercent = stochasticParameter.Value;

                if (parameter is AssignmentParameter asg)
                {
                    var evaluator = context.SimulationEvaluators.GetEvaluator(sim);

                    var asgparamName = asg.Name;
                    var currentValueParameter = sim.EntityParameters[componentModel.Name].GetParameter<double>(asgparamName, comparer);
                    var currentValue = currentValueParameter.Value;
                    var expressionContext = context.SimulationExpressionContexts.GetContext(sim);
                    var percentValue = evaluator.EvaluateValueExpression(parameterPercent.Parameter.Image, expressionContext);

                    double newValue = GetValueForDevParameter(expressionContext, currentValue, percentValue, parameterPercent.DistributionName);
                    context.SimulationPreparations.SetParameter(componentModel, sim, asgparamName, newValue, true, false);
                }
            }
        }

        private static double GetValueForDevParameter(ExpressionContext expressionContext, double currentValue, double percentValue, string distributionName)
        {
            var random = expressionContext.Randomizer.GetRandomProvider(expressionContext.Seed, distributionName);
            var r = random.NextSignedDouble();
            double newValue = currentValue + (percentValue / 100.0 * currentValue * r);
            return newValue;
        }

        private static double GetValueForLotParameter(ExpressionContext expressionContext, Entity baseModel, string parameterName, double currentValue, double percentValue, string distributionName, IEqualityComparer<string> comparer)
        {
            if (LotValues.ContainsKey(baseModel) && LotValues[baseModel].ContainsKey(parameterName))
            {
                return LotValues[baseModel][parameterName];
            }

            var random = expressionContext.Randomizer.GetRandomProvider(expressionContext.Seed, distributionName);
            double newValue = currentValue + (percentValue / 100.0 * currentValue * random.NextSignedDouble());

            if (!LotValues.ContainsKey(baseModel))
            {
                LotValues[baseModel] = new Dictionary<string, double>(comparer);
            }

            LotValues[baseModel][parameterName] = newValue;

            return newValue;
        }
    }
}
