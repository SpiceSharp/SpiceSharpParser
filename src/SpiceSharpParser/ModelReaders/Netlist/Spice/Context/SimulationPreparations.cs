using System;
using System.Collections.Generic;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationPreparations : ISimulationPreparations
    {
        public SimulationPreparations(EntityUpdates entityUpdates, SimulationUpdates simulationUpdates)
        {
            EntityUpdates = entityUpdates ?? throw new ArgumentNullException(nameof(entityUpdates));
            SimulationUpdates = simulationUpdates ?? throw new ArgumentNullException(nameof(simulationUpdates));
            BeforeSetup = new List<Action<Simulation>>();
            AfterSetup = new List<Action<Simulation>>();
        }

        protected EntityUpdates EntityUpdates { get; }

        protected SimulationUpdates SimulationUpdates { get; }

        protected List<Action<Simulation>> BeforeSetup { get; }

        protected List<Action<Simulation>> AfterSetup { get; }

        public void Prepare(Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            simulation.BeforeSetup += (_, _) =>
            {
                foreach (var action in BeforeSetup)
                {
                    action(simulation);
                }
            };

            simulation.AfterSetup += (_, _) =>
            {
                foreach (var action in AfterSetup)
                {
                    action(simulation);
                }
            };

            SimulationUpdates.Apply(simulation);
            EntityUpdates.Apply(simulation);
        }

        public void SetNodeSetVoltage(string nodeId, string expression)
        {
            if (nodeId == null)
            {
                throw new ArgumentNullException(nameof(nodeId));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            SimulationUpdates.AddBeforeTemperature((simulation, contexts) =>
            {
                var context = contexts.GetContext(simulation);
                var value = context.Evaluator.EvaluateDouble(expression);

                if (simulation is BiasingSimulation biasingSimulation)
                {
                    biasingSimulation.BiasingParameters.Nodesets[nodeId] = value;
                }
            });
        }

        public void SetICVoltage(string nodeId, string expression)
        {
            if (nodeId == null)
            {
                throw new ArgumentNullException(nameof(nodeId));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            SimulationUpdates.AddBeforeSetup((simulation, contexts) =>
            {
                var context = contexts.GetContext(simulation);
                var value = context.Evaluator.EvaluateDouble(expression);

                if (simulation is Transient ts)
                {
                    ts.TimeParameters.InitialConditions[nodeId] = value;
                }
            });
        }

        public void ExecuteActionBeforeSetup(Action<Simulation> action)
        {
            BeforeSetup.Add(action);
        }

        public void ExecuteActionAfterSetup(Action<Simulation> action)
        {
            AfterSetup.Add(action);
        }

        public void SetParameterBeforeTemperature(IEntity @object, string paramName, string expression)
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

            EntityUpdates.Add(@object, paramName, expression, true);
        }

        public void SetParameterBeforeTemperature(IEntity @object, string paramName, double value)
        {
            if (@object == null)
            {
                throw new ArgumentNullException(nameof(@object));
            }

            if (paramName == null)
            {
                throw new ArgumentNullException(nameof(paramName));
            }

            EntityUpdates.Add(@object, paramName, value, true);
        }

        public void SetParameterBeforeTemperature(IEntity @object, string paramName, double value, Simulation simulation)
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

            EntityUpdates.Add(@object, paramName, value, true, simulation);
        }
    }
}