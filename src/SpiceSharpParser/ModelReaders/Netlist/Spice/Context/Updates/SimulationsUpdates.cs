using System;
using System.Collections.Concurrent;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class SimulationsUpdates
    {
        public SimulationsUpdates(ISimulationEvaluators evaluators, SimulationExpressionContexts contexts)
        {
            Contexts = contexts;
            Evaluators = evaluators;
            SpecificUpdates = new ConcurrentDictionary<BaseSimulation, SimulationUpdates>();
            CommonUpdates = new SimulationUpdates();
        }

        protected ISimulationEvaluators Evaluators { get; set; }

        protected SimulationExpressionContexts Contexts { get; set; }

        protected ConcurrentDictionary<BaseSimulation, SimulationUpdates> SpecificUpdates { get; set; }

        protected SimulationUpdates CommonUpdates { get; set; }

        public void Apply(BaseSimulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }
            // Apply common updates
            simulation.BeforeSetup += (object sender, EventArgs args) =>
            {
                foreach (var pUpdate in CommonUpdates.ParameterUpdatesBeforeSetup)
                {
                    pUpdate.Update(simulation, Evaluators, Contexts);
                }
            };

            simulation.BeforeTemperature += (object sender, LoadStateEventArgs args) =>
            {
                foreach (var pUpdate in CommonUpdates.ParameterUpdatesBeforeTemperature)
                {
                    pUpdate.Update(simulation, Evaluators, Contexts);
                }
            };

            simulation.BeforeLoad += (object sender, LoadStateEventArgs args) =>
            {
                foreach (var pUpdate in CommonUpdates.ParameterUpdatesBeforeLoad)
                {
                    pUpdate.Update(simulation, Evaluators, Contexts);
                }
            };

            // Apply simulation specific updates
            if (SpecificUpdates.ContainsKey(simulation))
            {
                var update = SpecificUpdates[simulation];

                simulation.BeforeSetup += (object sender, EventArgs args) =>
                {
                    foreach (var pUpdate in update.ParameterUpdatesBeforeSetup)
                    {
                        pUpdate.Update(simulation, Evaluators, Contexts);
                    }
                };

                simulation.BeforeTemperature += (object sender, LoadStateEventArgs args) =>
                {
                    foreach (var pUpdate in update.ParameterUpdatesBeforeTemperature)
                    {
                        pUpdate.Update(simulation, Evaluators, Contexts);
                    }
                };
            }
        }

        public void AddBeforeSetup(BaseSimulation simulation, Action<BaseSimulation, ISimulationEvaluators, SimulationExpressionContexts> update)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (SpecificUpdates.ContainsKey(simulation) == false)
            {
                SpecificUpdates[simulation] = new SimulationUpdates() { Simulation = simulation };
            }

            SpecificUpdates[simulation].ParameterUpdatesBeforeSetup.Add(new SimulationUpdate() { Simulation = simulation, Update = update });
        }

        public void AddBeforeTemperature(BaseSimulation simulation, Action<BaseSimulation, ISimulationEvaluators, SimulationExpressionContexts> update)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (SpecificUpdates.ContainsKey(simulation) == false)
            {
                SpecificUpdates[simulation] = new SimulationUpdates() { Simulation = simulation };
            }

            SpecificUpdates[simulation].ParameterUpdatesBeforeSetup.Add(new SimulationUpdate() { Simulation = simulation, Update = update });
        }

        public void AddBeforeSetup(Action<BaseSimulation, ISimulationEvaluators, SimulationExpressionContexts> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            CommonUpdates.ParameterUpdatesBeforeSetup.Add(new SimulationUpdate() { Simulation = null, Update = update });
        }

        public void AddBeforeLoad(Action<BaseSimulation, ISimulationEvaluators, SimulationExpressionContexts> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            CommonUpdates.ParameterUpdatesBeforeLoad.Add(new SimulationUpdate() { Update = update });
        }

        public void AddBeforeTemperature(Action<BaseSimulation, ISimulationEvaluators, SimulationExpressionContexts> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            CommonUpdates.ParameterUpdatesBeforeTemperature.Add(new SimulationUpdate() { Simulation = null, Update = update });
        }
    }
}
