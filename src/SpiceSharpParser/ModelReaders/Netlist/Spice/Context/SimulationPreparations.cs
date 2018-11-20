using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationPreparations : ISimulationPreparations
    {
        public SimulationPreparations(EntityUpdates entityUpdates, SimulationsUpdates simulationUpdates)
        {
            EntityUpdates = entityUpdates;
            SimulationUpdates = simulationUpdates;
        }

        protected EntityUpdates EntityUpdates { get; }

        protected SimulationsUpdates SimulationUpdates { get; }

        public void Prepare(BaseSimulation simulation)
        {
            SimulationUpdates.Apply(simulation);
            EntityUpdates.Apply(simulation);
        }

        public void SetNodeSetVoltage(string nodeId, string expression)
        {
            SimulationUpdates.AddBeforeTemperature((simulation, evaluators, contexts) =>
            {
                var simEval = evaluators.GetEvaluator(simulation);
                var context = contexts.GetContext(simulation);
                var value = simEval.EvaluateValueExpression(expression, context);

                simulation.Configurations.Get<BaseConfiguration>().Nodesets[nodeId] = value;
            });
        }

        public void SetICVoltage(string nodeId, string expression)
        {
            SimulationUpdates.AddBeforeSetup((simulation, evaluators, contexts) =>
            {
                var simEval = evaluators.GetEvaluator(simulation);
                var context = contexts.GetContext(simulation);
                var value = simEval.EvaluateValueExpression(expression, context);

                if (simulation is TimeSimulation ts)
                {
                    ts.Configurations.Get<TimeConfiguration>().InitialConditions[nodeId] = value;
                }
            });
        }

        public void ExecuteTemperatuteBehaviorBeforeLoad(Entity entity)
        {
            SimulationUpdates.AddBeforeLoad((simulation, evaluators, contexts) =>
            {
                if (simulation.EntityBehaviors[entity.Name].TryGet<ITemperatureBehavior>(out var temperatureBehavior))
                {
                    temperatureBehavior.Temperature(simulation);
                }
                else
                {
                    throw new InvalidOperationException($"No temperature behavior for {entity.Name}");
                }
            });
        }

        public void SetParameter(Entity @object, string paramName, string expression, bool beforeTemperature, bool onload)
        {
            EntityUpdates.Add(@object, paramName, expression, beforeTemperature, onload);
        }

        public void SetParameter(Entity @object, string paramName, double value, bool beforeTemperature, bool onload)
        {
            EntityUpdates.Add(@object, paramName, value, beforeTemperature, onload);
        }

        public void SetParameter(Entity @object, BaseSimulation simulation, string paramName, double value, bool beforeTemperature, bool onload)
        {
            EntityUpdates.Add(@object, simulation, paramName, value, beforeTemperature, onload);
        }
    }
}
