using System.Collections.Concurrent;
using System.Collections.Generic;
using SpiceSharp;
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
    public class EnableStochasticModelsSimulationDecorator
    {
        private readonly ICircuitContext _context;
        private readonly ConcurrentDictionary<Entity, Dictionary<string, double>> LotValues = new ConcurrentDictionary<Entity, Dictionary<string, double>>();

        public EnableStochasticModelsSimulationDecorator(ICircuitContext context)
        {
            _context = context;
        }

        public BaseSimulation Decorate(BaseSimulation simulation)
        {
            var modelsRegistry = _context.ModelsRegistry as IStochasticModelsRegistry;

            if (modelsRegistry != null)
            {
                simulation.BeforeExecute += (object sender, BeforeExecuteEventArgs arg) =>
                {
                    foreach (var stochasticModels in modelsRegistry.GetStochasticModels(simulation))
                    {
                        var baseModel = stochasticModels.Key;
                        var componentModels = stochasticModels.Value;

                        foreach (var componentModel in componentModels)
                        {
                            Dictionary<Parameter, ParameterRandomness> stochasticDevParameters = modelsRegistry.GetStochasticModelDevParameters(baseModel);
                            if (stochasticDevParameters != null)
                            {
                                SetModelDevModelParameters(simulation, baseModel, componentModel, stochasticDevParameters);
                            }

                            Dictionary<Parameter, ParameterRandomness> stochasticLotParameters = modelsRegistry.GetStochasticModelLotParameters(baseModel);
                            if (stochasticLotParameters != null)
                            {
                                SetModelLotModelParameters(simulation, baseModel, componentModel, stochasticLotParameters);
                            }
                        }
                    }
                };
            }

            return simulation;
        }

        private void SetModelLotModelParameters(BaseSimulation sim, Entity baseModel, Entity componentModel, Dictionary<Parameter, ParameterRandomness> stochasticLotParameters)
        {
            var comparer = StringComparerProvider.Get(_context.CaseSensitivity.IsEntityParameterNameCaseSensitive);
            foreach (var stochasticParameter in stochasticLotParameters)
            {
                var parameter = stochasticParameter.Key;
                var parameterPercent = stochasticParameter.Value;

                if (parameter is AssignmentParameter asg)
                {
                    var parameterName = asg.Name;
                    var currentValueParameter = sim.EntityParameters[componentModel.Name].GetParameter<Parameter<double>>(parameterName, comparer);

                    var currentValue = currentValueParameter.Value;
                    var percentValue = _context.CircuitEvaluator.EvaluateDouble(parameterPercent.Tolerance.Image, sim);
                    double newValue = GetValueForLotParameter(_context.CircuitEvaluator.GetContext(sim), baseModel, parameterName, currentValue, percentValue, parameterPercent.RandomDistributionName, comparer);

                    _context.SimulationPreparations.SetParameter(componentModel, sim, parameterName, newValue, true, false);
                }
            }
        }

        private void SetModelDevModelParameters(BaseSimulation sim, Entity baseModel, Entity componentModel, Dictionary<Parameter, ParameterRandomness> stochasticDevParameters)
        {
            var comparer = StringComparerProvider.Get(_context.CaseSensitivity.IsEntityParameterNameCaseSensitive);
            foreach (var stochasticParameter in stochasticDevParameters)
            {
                var parameter = stochasticParameter.Key;
                var parameterPercent = stochasticParameter.Value;

                if (parameter is AssignmentParameter assignmentParameter)
                {

                    var parameterName = assignmentParameter.Name;
                    var currentValueParameter = sim.EntityParameters[componentModel.Name].GetParameter<Parameter<double>>(parameterName, comparer);
                    var currentValue = currentValueParameter.Value;
                    var percentValue = _context.CircuitEvaluator.EvaluateDouble(parameterPercent.Tolerance.Image, sim);

                    var random = _context.CircuitEvaluator.GetContext().Randomizer.GetRandomProvider(parameterPercent.RandomDistributionName);
                    var r = random.NextSignedDouble();
                    var newValue = currentValue + (percentValue / 100.0) * currentValue * r;

                    _context.SimulationPreparations.SetParameter(componentModel, sim, parameterName, newValue, true, false);
                }
            }
        }

        private double GetValueForLotParameter(ExpressionContext expressionContext, Entity baseModel, string parameterName, double currentValue, double percentValue, string distributionName, IEqualityComparer<string> comparer)
        {
            if (LotValues.ContainsKey(baseModel) && LotValues[baseModel].ContainsKey(parameterName))
            {
                return LotValues[baseModel][parameterName];
            }

            var random = expressionContext.Randomizer.GetRandomProvider(distributionName);
            double newValue = currentValue + (percentValue / 100.0) * currentValue * random.NextSignedDouble();

            if (!LotValues.ContainsKey(baseModel))
            {
                LotValues[baseModel] = new Dictionary<string, double>(comparer);
            }

            LotValues[baseModel][parameterName] = newValue;

            return newValue;
        }
    }
}
