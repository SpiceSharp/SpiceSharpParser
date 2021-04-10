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
            SpecificUpdates = new ConcurrentDictionary<Simulation, SimulationUpdates>();
            CommonUpdates = new SimulationUpdates();
        }

        protected SimulationEvaluationContexts Contexts { get; set; }

        protected ConcurrentDictionary<Simulation, SimulationUpdates> SpecificUpdates { get; set; }

        protected SimulationUpdates CommonUpdates { get; set; }

        public void Apply(Simulation simulation)
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

            var biasingSimulation = simulation as BiasingSimulation;

            biasingSimulation.BeforeTemperature += (object sender, TemperatureStateEventArgs args) =>
            {
                foreach (var pUpdate in CommonUpdates.ParameterUpdatesBeforeTemperature)
                {
                    pUpdate.Update(simulation, Contexts);
                }
            };

            biasingSimulation.BeforeLoad += (object sender, LoadStateEventArgs args) =>
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

                biasingSimulation.BeforeTemperature += (object sender, TemperatureStateEventArgs args) =>
                {
                    foreach (var pUpdate in update.ParameterUpdatesBeforeTemperature)
                    {
                        pUpdate.Update(simulation, Contexts);
                    }
                };
            }
        }

        public void AddBeforeSetup(Simulation simulation, Action<Simulation, SimulationEvaluationContexts> update)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (!SpecificUpdates.ContainsKey(simulation))
            {
                SpecificUpdates[simulation] = new SimulationUpdates() { Simulation = simulation };
            }

            SpecificUpdates[simulation].ParameterUpdatesBeforeSetup.Add(new SimulationUpdate() { Simulation = simulation, Update = update });
        }

        public void AddBeforeTemperature(Simulation simulation, Action<Simulation, SimulationEvaluationContexts> update)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (!SpecificUpdates.ContainsKey(simulation))
            {
                SpecificUpdates[simulation] = new SimulationUpdates() { Simulation = simulation };
            }

            SpecificUpdates[simulation].ParameterUpdatesBeforeTemperature.Add(new SimulationUpdate() { Simulation = simulation, Update = update });
        }

        public void AddBeforeSetup(Action<Simulation, SimulationEvaluationContexts> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            CommonUpdates.ParameterUpdatesBeforeSetup.Add(new SimulationUpdate() { Simulation = null, Update = update });
        }

        public void AddBeforeLoad(Action<Simulation, SimulationEvaluationContexts> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            CommonUpdates.ParameterUpdatesBeforeLoad.Add(new SimulationUpdate() { Update = update });
        }

        public void AddBeforeTemperature(Action<Simulation, SimulationEvaluationContexts> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            CommonUpdates.ParameterUpdatesBeforeTemperature.Add(new SimulationUpdate() { Simulation = null, Update = update });
        }
    }
}