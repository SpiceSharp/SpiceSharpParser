using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
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
            SimulationBeforeSetupActions = new ConcurrentDictionary<ISimulationWithEvents, List<SimulationUpdateAction>>();
            SimulationBeforeTemperatureActions = new ConcurrentDictionary<ISimulationWithEvents, List<SimulationUpdateAction>>();
            CommonBeforeSetupActions = new List<SimulationUpdateAction>();
            CommonBeforeTemperatureActions = new List<SimulationUpdateAction>();
        }

        protected SimulationEvaluationContexts Contexts { get; private set; }

        protected ConcurrentDictionary<ISimulationWithEvents, List<SimulationUpdateAction>> SimulationBeforeSetupActions { get; }

        protected ConcurrentDictionary<ISimulationWithEvents, List<SimulationUpdateAction>> SimulationBeforeTemperatureActions { get; }

        protected List<SimulationUpdateAction> CommonBeforeSetupActions { get; }

        protected List<SimulationUpdateAction> CommonBeforeTemperatureActions { get; }

        public void Apply(ISimulationWithEvents simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }
            ISimulationWithEvents biasingSimulation = simulation as ISimulationWithEvents;

            // Apply common updates
            biasingSimulation.EventBeforeSetup += (_, _) =>
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

            if (biasingSimulation != null)
            {
                biasingSimulation.EventBeforeTemperature += (_, _) =>
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

        public void AddBeforeSetup(ISimulationWithEvents simulation, Action<ISimulationWithEvents, SimulationEvaluationContexts> action)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            var actions = SimulationBeforeSetupActions.GetOrAdd(simulation, _ => new List<SimulationUpdateAction>());
            actions.Add(new SimulationUpdateAction(action));
        }

        public void AddBeforeTemperature(ISimulationWithEvents simulation, Action<ISimulationWithEvents, SimulationEvaluationContexts> action)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            var actions = SimulationBeforeTemperatureActions.GetOrAdd(simulation, _ => new List<SimulationUpdateAction>());
            actions.Add(new SimulationUpdateAction(action));
        }

        public void AddBeforeSetup(Action<ISimulationWithEvents, SimulationEvaluationContexts> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            CommonBeforeSetupActions.Add(new SimulationUpdateAction(action));
        }

        public void AddBeforeTemperature(Action<ISimulationWithEvents, SimulationEvaluationContexts> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            CommonBeforeTemperatureActions.Add(new SimulationUpdateAction(action));
        }
    }
}