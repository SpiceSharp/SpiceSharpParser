using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
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
                                Dictionary<Parameter, Parameter> stochasticDevParameters = modelsRegistry.GetStochasticModelDevParameters(baseModel);

                                if (stochasticDevParameters != null)
                                {
                                    SetModelDevModelParameters(context, simulation, baseModel, componentModel, stochasticDevParameters);
                                }

                                Dictionary<Parameter, Parameter> stochasticLotParameters = modelsRegistry.GetStochasticModelLotParameters(baseModel);
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

        private static void SetModelLotModelParameters(IReadingContext context, BaseSimulation sim, Entity baseModel, Entity componentModel, Dictionary<Parameter, Parameter> stochasticLotParameters)
        {
            var comparer = StringComparerProvider.Get(context.CaseSensitivity.IsEntityParameterNameCaseSensitive);
            foreach (var stochasticParameter in stochasticLotParameters)
            {
                var parameter = stochasticParameter.Key;
                var parameterPercent = stochasticParameter.Value;

                if (parameter is AssignmentParameter asg)
                {
                    var evaluator = context.SimulutionEvaluators.GetEvaluator(sim);

                    var parameterName = asg.Name;
                    var currentValueParameter = sim.EntityParameters[componentModel.Name].GetParameter<double>(parameterName, comparer);
                    var expressionContext = context.SimulationExpressionContexts.GetContext(sim);

                    var currentValue = currentValueParameter.Value;
                    var percentValue = evaluator.EvaluateValueExpression(parameterPercent.Image, expressionContext);
                    double newValue = GetValueForLotParameter(expressionContext, baseModel, parameterName, currentValue, percentValue, comparer);
                    context.SimulationPreparations.SetParameter(componentModel, sim, parameterName, newValue, true, false);
                }
            }
        }

        private static void SetModelDevModelParameters(IReadingContext context, BaseSimulation sim, Entity baseModel, Entity componentModel, System.Collections.Generic.Dictionary<Parameter, Parameter> stochasticDevParameters)
        {
            var comparer = StringComparerProvider.Get(context.CaseSensitivity.IsEntityParameterNameCaseSensitive);
            foreach (var stochasticParameter in stochasticDevParameters)
            {
                var parameter = stochasticParameter.Key;
                var parameterPercent = stochasticParameter.Value;

                if (parameter is AssignmentParameter asg)
                {
                    var evaluator = context.SimulutionEvaluators.GetEvaluator(sim);

                    var asgparamName = asg.Name;
                    var currentValueParameter = sim.EntityParameters[componentModel.Name].GetParameter<double>(asgparamName, comparer);
                    var currentValue = currentValueParameter.Value;
                    var expressionContext = context.SimulationExpressionContexts.GetContext(sim);
                    var percentValue = evaluator.EvaluateValueExpression(parameterPercent.Image, expressionContext);

                    double newValue = GetValueForDevParameter(expressionContext, currentValue, percentValue);
                    context.SimulationPreparations.SetParameter(componentModel, sim, asgparamName, newValue, true, false);
                }
            }
        }

        private static double GetValueForDevParameter(ExpressionContext expressionContext, double currentValue, double percentValue)
        {
            var random = expressionContext.Randomizer.GetRandom(expressionContext.Seed);

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

        private static double GetValueForLotParameter(ExpressionContext expressionContext, Entity baseModel, string parameterName, double currentValue, double percentValue, IEqualityComparer<string> comparer)
        {
            if (LotValues.ContainsKey(baseModel) && LotValues[baseModel].ContainsKey(parameterName))
            {
                return LotValues[baseModel][parameterName];
            }

            var random = expressionContext.Randomizer.GetRandom(expressionContext.Seed);

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
                LotValues[baseModel] = new Dictionary<string, double>(comparer);
            }

            LotValues[baseModel][parameterName] = newValue;

            return newValue;
        }
    }
}
