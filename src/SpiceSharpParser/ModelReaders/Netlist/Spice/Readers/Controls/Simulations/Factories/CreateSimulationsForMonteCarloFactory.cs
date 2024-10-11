using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public class CreateSimulationsForMonteCarloFactory : ICreateSimulationsForMonteCarloFactory
    {
        public CreateSimulationsForMonteCarloFactory(
            ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory temperatureAndParameterSweepsSimulationFactory,
            IExportFactory exportFactory,
            IMapper<Exporter> mapperExporter)
        {
            TemperatureAndParameterSweepsSimulationFactory = temperatureAndParameterSweepsSimulationFactory;
            ExportFactory = exportFactory;
            MapperExporter = mapperExporter;
        }

        /// <summary>
        /// Gets the exporter mapper.
        /// </summary>
        protected IMapper<Exporter> MapperExporter { get; }

        /// <summary>
        /// Gets the simulations factory.
        /// </summary>
        protected ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory TemperatureAndParameterSweepsSimulationFactory { get; }

        /// <summary>
        /// Gets the export factory.
        /// </summary>
        protected IExportFactory ExportFactory { get; }

        /// <summary>
        /// Creates simulations for Monte Carlo simulation.
        /// </summary>
        /// <param name="statement">Statement.</param>
        /// <param name="context">Context.</param>
        /// <param name="createSimulation">Simulation factory.</param>
        public List<ISimulationWithEvents> Create(Control statement, IReadingContext context, Func<string, Control, IReadingContext, ISimulationWithEvents> createSimulation)
        {
            context.Result.MonteCarloResult.Enabled = true;
            context.Result.MonteCarloResult.Seed = context.SimulationConfiguration.MonteCarloConfiguration.Seed;
            context.Result.MonteCarloResult.OutputVariable = context.SimulationConfiguration.MonteCarloConfiguration.OutputVariable.Value;
            context.Result.MonteCarloResult.Function = context.SimulationConfiguration.MonteCarloConfiguration.Function;

            var result = new List<ISimulationWithEvents>();

            for (var i = 0; i < context.SimulationConfiguration.MonteCarloConfiguration.Runs; i++)
            {
                var simulations = TemperatureAndParameterSweepsSimulationFactory.CreateSimulations(statement, context, createSimulation);
                AttachMonteCarloDataGathering(context, simulations);
                result.AddRange(simulations);
            }

            return result;
        }

        protected void AttachMonteCarloDataGathering(IReadingContext context, IEnumerable<ISimulationWithEvents> simulations)
        {
            foreach (var simulation in simulations)
            {
                AttachMonteCarloDataGatheringForSimulation(context, simulation);
            }
        }

        protected void AttachMonteCarloDataGatheringForSimulation(IReadingContext context, ISimulationWithEvents simulation)
        {
            var exportParam = context.SimulationConfiguration.MonteCarloConfiguration.OutputVariable;

            simulation.EventBeforeSetup += (sender, args) =>
            {
                Export export = context.Result.Exports.FirstOrDefault(e => e.Simulation == simulation && e.Name == exportParam.Value);

                if (export == null)
                {
                    export = ExportFactory.Create(exportParam, context, simulation, MapperExporter);
                }

                simulation.EventExportData += (exportSender, exportArgs) =>
                {
                    if (export != null)
                    {
                        var value = export.Extract();
                        context.Result.MonteCarloResult.Collect(simulation, value);
                    }
                };
            };
        }
    }
}