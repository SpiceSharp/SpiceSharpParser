using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators
{
    public class EnableStochasticModelsSimulationDecorator
    {
        private readonly ICircuitContext _context;
        private readonly ConcurrentDictionary<string, Dictionary<string, double>> _lotValues = new ConcurrentDictionary<string, Dictionary<string, double>>();

        public EnableStochasticModelsSimulationDecorator(ICircuitContext context)
        {
            _context = context;
        }

        public Simulation Decorate(Simulation simulation)
        {
            if (_context.ModelsRegistry is IStochasticModelsRegistry modelsRegistry)
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
                                SetModelDevModelParameters(simulation, componentModel.Entity, stochasticDevParameters);
                            }

                            Dictionary<Parameter, ParameterRandomness> stochasticLotParameters = modelsRegistry.GetStochasticModelLotParameters(baseModel);
                            if (stochasticLotParameters != null)
                            {
                                SetModelLotModelParameters(simulation, baseModel, componentModel.Entity, stochasticLotParameters);
                            }
                        }
                    }
                };
            }

            return simulation;
        }

        private void SetModelLotModelParameters(Simulation simulation, string baseModel, IEntity componentModel, Dictionary<Parameter, ParameterRandomness> stochasticLotParameters)
        {
            var comparer = StringComparerProvider.Get(false);
            foreach (var stochasticParameter in stochasticLotParameters)
            {
                var parameter = stochasticParameter.Key;
                var parameterPercent = stochasticParameter.Value;

                if (parameter is AssignmentParameter asg)
                {
                    var parameterName = asg.Name;
                    var currentValue = simulation.EntityBehaviors[componentModel.Name].GetProperty<double>(parameterName);
                    var percentValue = _context.Evaluator.EvaluateDouble(parameterPercent.Tolerance.Image, simulation);
                    double newValue = GetValueForLotParameter(_context.Evaluator.GetEvaluationContext(simulation), baseModel, parameterName, currentValue, percentValue, parameterPercent.RandomDistributionName, comparer);

                    _context.SimulationPreparations.SetParameter(componentModel, simulation, parameterName, newValue, true, false);
                }
            }
        }

        private void SetModelDevModelParameters(Simulation simulation, IEntity componentModel, Dictionary<Parameter, ParameterRandomness> stochasticDevParameters)
        {
            foreach (var stochasticParameter in stochasticDevParameters)
            {
                var parameter = stochasticParameter.Key;
                var parameterPercent = stochasticParameter.Value;

                if (parameter is AssignmentParameter assignmentParameter)
                {
                    var parameterName = assignmentParameter.Name;
                    var currentValue = simulation.EntityBehaviors[componentModel.Name].GetProperty<double>(parameterName);
                    var percentValue = _context.Evaluator.EvaluateDouble(parameterPercent.Tolerance.Image, simulation);

                    var random = _context.Evaluator.GetEvaluationContext(simulation).Randomizer.GetRandomProvider(parameterPercent.RandomDistributionName);
                    var r = random.NextSignedDouble();
                    var newValue = currentValue + ((percentValue / 100.0) * currentValue * r);

                    _context.SimulationPreparations.SetParameter(componentModel, simulation, parameterName, newValue, true, false);
                }
            }
        }

        private double GetValueForLotParameter(EvaluationContext evaluationContext, string baseModel, string parameterName, double currentValue, double percentValue, string distributionName, IEqualityComparer<string> comparer)
        {
            if (_lotValues.ContainsKey(baseModel) && _lotValues[baseModel].ContainsKey(parameterName))
            {
                return _lotValues[baseModel][parameterName];
            }

            var random = evaluationContext.Randomizer.GetRandomProvider(distributionName);
            double newValue = currentValue + ((percentValue / 100.0) * currentValue * random.NextSignedDouble());

            if (!_lotValues.ContainsKey(baseModel))
            {
                _lotValues[baseModel] = new Dictionary<string, double>(comparer);
            }

            _lotValues[baseModel][parameterName] = newValue;

            return newValue;
        }
    }
}