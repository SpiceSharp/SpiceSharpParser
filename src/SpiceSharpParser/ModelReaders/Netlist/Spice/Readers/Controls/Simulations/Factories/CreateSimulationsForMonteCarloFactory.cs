using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Exporters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public class CreateSimulationsForMonteCarloFactory : ICreateSimulationsForMonteCarloFactory
    {
        public ICreateSimulationsForAllTemperaturesFactory AllTemperatures { get; }
        public ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory AllTemperaturesAndSweeps { get; }

        public CreateSimulationsForMonteCarloFactory(ICreateSimulationsForAllTemperaturesFactory allTemperatures, ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory allTemperaturesAndSweeps)
        {
            AllTemperaturesAndSweeps = allTemperaturesAndSweeps;
            AllTemperatures = allTemperatures;
        }

        public void Create(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> simulationWithStochasticModels)
        {
            context.Result.MonteCarlo.Enabled = true;
            context.Result.MonteCarlo.RandomSeed = context.Result.SimulationConfiguration.RandomSeed;
            context.Result.MonteCarlo.VariableName = context.Result.SimulationConfiguration.MonteCarloConfiguration.OutputVariable;
            context.Result.MonteCarlo.Function = context.Result.SimulationConfiguration.MonteCarloConfiguration.Function;

            if (context.Result.SimulationConfiguration.ParameterSweeps.Count == 0)
            {
                for (var i = 0; i < context.Result.SimulationConfiguration.MonteCarloConfiguration.Runs; i++)
                {
                    var simulations = AllTemperatures.CreateSimulations(statement, context, simulationWithStochasticModels);
                    AttachMonteCarloDataGathering(context, simulations);
                }
            }
            else
            {
                for (var i = 0; i < context.Result.SimulationConfiguration.MonteCarloConfiguration.Runs; i++)
                {
                    var simulations = AllTemperaturesAndSweeps.CreateSimulations(statement, context, simulationWithStochasticModels);
                    AttachMonteCarloDataGathering(context, simulations);
                }
            }
        }

        protected static void AttachMonteCarloDataGathering(IReadingContext context, IEnumerable<BaseSimulation> simulations)
        {
            foreach (var simulation in simulations)
            {
                AttachMonteCarloDataGatheringForSimulation(context, simulation);
            }
        }

        protected static void AttachMonteCarloDataGatheringForSimulation(IReadingContext context, BaseSimulation simulation)
        {
            Export export = null;

            simulation.BeforeExecute += (object sender, BeforeExecuteEventArgs args) =>
            {
                export = context.Result.Exports.SingleOrDefault(e => e.Simulation == simulation && e.Name.ToLower() == context.Result.SimulationConfiguration.MonteCarloConfiguration.OutputVariable.ToLower());
            };

            simulation.ExportSimulationData += (object sender, ExportDataEventArgs args) =>
            {
                if (export != null)
                {
                    var value = export.Extract();
                    context.Result.MonteCarlo.Collect(simulation, value);
                }
            };
        }

    }
}
