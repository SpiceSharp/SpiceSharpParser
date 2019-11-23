using SpiceSharp.Simulations;
using System;
using System.Collections.Concurrent;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class SimulationsUpdates
    {
        public SimulationsUpdates(SimulationEvaluationContexts contexts)
        {
            Contexts = contexts;
            SpecificUpdates = new ConcurrentDictionary<BaseSimulation, SimulationUpdates>();
            CommonUpdates = new SimulationUpdates();
        }

        protected SimulationEvaluationContexts Contexts { get; set; }

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
                    pUpdate.Update(simulation, Contexts);
                }
            };

            simulation.BeforeTemperature += (object sender, LoadStateEventArgs args) =>
            {
                foreach (var pUpdate in CommonUpdates.ParameterUpdatesBeforeTemperature)
                {
                    pUpdate.Update(simulation, Contexts);
                }
            };

            simulation.BeforeLoad += (object sender, LoadStateEventArgs args) =>
            {
                foreach (var pUpdate in CommonUpdates.ParameterUpdatesBeforeLoad)
                {
                    pUpdate.Update(simulation, Contexts);
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
                        pUpdate.Update(simulation, Contexts);
                    }
                };

                simulation.BeforeTemperature += (object sender, LoadStateEventArgs args) =>
                {
                    foreach (var pUpdate in update.ParameterUpdatesBeforeTemperature)
                    {
                        pUpdate.Update(simulation, Contexts);
                    }
                };
            }
        }

        public void AddBeforeSetup(BaseSimulation simulation, Action<BaseSimulation, SimulationEvaluationContexts> update)
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

        public void AddBeforeTemperature(BaseSimulation simulation, Action<BaseSimulation, SimulationEvaluationContexts> update)
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

        public void AddBeforeSetup(Action<BaseSimulation, SimulationEvaluationContexts> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            CommonUpdates.ParameterUpdatesBeforeSetup.Add(new SimulationUpdate() { Simulation = null, Update = update });
        }

        public void AddBeforeLoad(Action<BaseSimulation, SimulationEvaluationContexts> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            CommonUpdates.ParameterUpdatesBeforeLoad.Add(new SimulationUpdate() { Update = update });
        }

        public void AddBeforeTemperature(Action<BaseSimulation, SimulationEvaluationContexts> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            CommonUpdates.ParameterUpdatesBeforeTemperature.Add(new SimulationUpdate() { Simulation = null, Update = update });
        }
    }
}