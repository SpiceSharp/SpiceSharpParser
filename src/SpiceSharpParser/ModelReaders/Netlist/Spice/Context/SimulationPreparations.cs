using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationPreparations : ISimulationPreparations
    {
        public SimulationPreparations(EntityUpdates entityUpdates, SimulationsUpdates simulationUpdates)
        {
            EntityUpdates = entityUpdates ?? throw new ArgumentNullException(nameof(entityUpdates));
            SimulationUpdates = simulationUpdates ?? throw new ArgumentNullException(nameof(simulationUpdates));
            BeforeExecute = new List<Action<BaseSimulation>>();
        }

        protected EntityUpdates EntityUpdates { get; }

        protected SimulationsUpdates SimulationUpdates { get; }

        protected List<Action<BaseSimulation>> BeforeExecute { get; }

        public void Prepare(BaseSimulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            simulation.BeforeSetup += (obj, args) =>
            {
                foreach (var action in BeforeExecute)
                {
                    action(simulation);
                }
            };

            SimulationUpdates.Apply(simulation);
            EntityUpdates.Apply(simulation);
        }

        public void SetNodeSetVoltage(string nodeId, string expression, IReadingContext readingContext)
        {
            if (nodeId == null)
            {
                throw new ArgumentNullException(nameof(nodeId));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            SimulationUpdates.AddBeforeTemperature((simulation, evaluators, contexts) =>
            {
                var simEval = evaluators.GetEvaluator(simulation);
                var context = contexts.GetContext(simulation);
                var value = simEval.Evaluate(new DynamicExpression(expression), context, simulation, readingContext);

                simulation.Configurations.Get<BaseConfiguration>().Nodesets[nodeId] = value;
            });
        }

        public void SetICVoltage(string nodeId, string expression, IReadingContext readingContext)
        {
            if (nodeId == null)
            {
                throw new ArgumentNullException(nameof(nodeId));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            SimulationUpdates.AddBeforeSetup((simulation, evaluators, contexts) =>
            {
                var simEval = evaluators.GetEvaluator(simulation);
                var context = contexts.GetContext(simulation);
                var value = simEval.Evaluate(new DynamicExpression(expression), context, simulation, readingContext);

                if (simulation is TimeSimulation ts)
                {
                    ts.Configurations.Get<TimeConfiguration>().InitialConditions[nodeId] = value;
                }
            });
        }

        public void ExecuteTemperatureBehaviorBeforeLoad(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            SimulationUpdates.AddBeforeLoad((simulation, evaluators, contexts) =>
            {
                if (simulation.EntityBehaviors[entity.Name].TryGet<ITemperatureBehavior>(out var temperatureBehavior))
                {
                    temperatureBehavior.Temperature();
                }
                else
                {
                    throw new InvalidOperationException($"No temperature behavior for {entity.Name}");
                }
            });
        }

        public void ExecuteActionBeforeSetup(Action<BaseSimulation> action)
        {
            BeforeExecute.Add(action);
        }

        public void SetParameter(Entity @object, string paramName, string expression, bool beforeTemperature, bool onload, IReadingContext readingContext)
        {
            if (@object == null)
            {
                throw new ArgumentNullException(nameof(@object));
            }

            if (paramName == null)
            {
                throw new ArgumentNullException(nameof(paramName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            EntityUpdates.Add(@object, paramName, expression, beforeTemperature, onload, readingContext);
        }

        public void SetParameter(Entity @object, string paramName, double value, bool beforeTemperature, bool onload)
        {
            if (@object == null)
            {
                throw new ArgumentNullException(nameof(@object));
            }

            if (paramName == null)
            {
                throw new ArgumentNullException(nameof(paramName));
            }

            EntityUpdates.Add(@object, paramName, value, beforeTemperature, onload);
        }

        public void SetParameter(Entity @object, BaseSimulation simulation, string paramName, double value, bool beforeTemperature, bool onload)
        {
            if (@object == null)
            {
                throw new ArgumentNullException(nameof(@object));
            }

            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (paramName == null)
            {
                throw new ArgumentNullException(nameof(paramName));
            }

            EntityUpdates.Add(@object, simulation, paramName, value, beforeTemperature, onload);
        }
    }
}
