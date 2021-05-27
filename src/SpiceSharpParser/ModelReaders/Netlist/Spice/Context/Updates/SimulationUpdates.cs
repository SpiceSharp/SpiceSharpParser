using SpiceSharp.Simulations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class SimulationUpdates
    {
        public SimulationUpdates(SimulationEvaluationContexts contexts)
        {
            Contexts = contexts;
            SimulationBeforeSetupActions = new ConcurrentDictionary<Simulation, List<SimulationUpdateAction>>();
            SimulationBeforeTemperatureActions = new ConcurrentDictionary<Simulation, List<SimulationUpdateAction>>();
            CommonBeforeSetupActions = new List<SimulationUpdateAction>();
            CommonBeforeTemperatureActions = new List<SimulationUpdateAction>();
        }

        protected SimulationEvaluationContexts Contexts { get; private set; }

        protected ConcurrentDictionary<Simulation, List<SimulationUpdateAction>> SimulationBeforeSetupActions { get; }

        protected ConcurrentDictionary<Simulation, List<SimulationUpdateAction>> SimulationBeforeTemperatureActions { get; }

        protected List<SimulationUpdateAction> CommonBeforeSetupActions { get; }

        protected List<SimulationUpdateAction> CommonBeforeTemperatureActions { get; }

        public void Apply(Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            // Apply common updates
            simulation.BeforeSetup += (_, _) =>
            {
                foreach (var action in CommonBeforeSetupActions)
                {
                    action.Run(simulation, Contexts);
                }

                if (SimulationBeforeSetupActions.ContainsKey(simulation))
                {
                    var actions = SimulationBeforeSetupActions[simulation];

                    foreach (var action in actions)
                    {
                        action.Run(simulation, Contexts);
                    }
                }
            };

            BiasingSimulation biasingSimulation = simulation as BiasingSimulation;
            if (biasingSimulation != null)
            {
                biasingSimulation.BeforeTemperature += (_, _) =>
                {
                    foreach (var action in CommonBeforeTemperatureActions)
                    {
                        action.Run(simulation, Contexts);
                    }

                    if (SimulationBeforeTemperatureActions.ContainsKey(simulation))
                    {
                        var actions = SimulationBeforeTemperatureActions[simulation];

                        foreach (var action in actions)
                        {
                            action.Run(simulation, Contexts);
                        }
                    }
                };
            }
        }

        public void AddBeforeSetup(Simulation simulation, Action<Simulation, SimulationEvaluationContexts> action)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (!SimulationBeforeSetupActions.ContainsKey(simulation))
            {
                SimulationBeforeSetupActions[simulation] = new List<SimulationUpdateAction>() { new SimulationUpdateAction(action) };
            }
            else
            {
                SimulationBeforeSetupActions[simulation].Add(new SimulationUpdateAction(action));
            }
        }

        public void AddBeforeTemperature(Simulation simulation, Action<Simulation, SimulationEvaluationContexts> action)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (!SimulationBeforeTemperatureActions.ContainsKey(simulation))
            {
                SimulationBeforeTemperatureActions[simulation] = new List<SimulationUpdateAction>() { new SimulationUpdateAction(action) };
            }
            else
            {
                SimulationBeforeTemperatureActions[simulation].Add(new SimulationUpdateAction(action));
            }
        }

        public void AddBeforeSetup(Action<Simulation, SimulationEvaluationContexts> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            CommonBeforeSetupActions.Add(new SimulationUpdateAction(action));
        }

        public void AddBeforeTemperature(Action<Simulation, SimulationEvaluationContexts> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            CommonBeforeTemperatureActions.Add(new SimulationUpdateAction(action));
        }
    }
}