using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using System;
using System.Threading;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators
{
    public class CreateSimulationWithStochasticModelsDecorator
    {
        private static int tickCount = Environment.TickCount;

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
                                        var random = new Random(evaluator.RandomSeed ?? Interlocked.Increment(ref tickCount));

                                        double newValue = 0;
                                        if (random.Next() % 2 == 0)
                                        {
                                            newValue = currentValue + (percentValue / 100.0) * currentValue * random.NextDouble();
                                        }
                                        else
                                        {
                                            newValue = currentValue - (percentValue / 100.0) * currentValue * random.NextDouble();
                                        }

                                        context.SimulationContexts.SetModelParameter(asgparamName, componentModel, newValue.ToString(), sim);
                                    }
                                }
                            }
                        }
                    }
                };

                return sim;
            };
        }
    }
}
