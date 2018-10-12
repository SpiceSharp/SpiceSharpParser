using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
    using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;

    public class CreateSimulationsForMonteCarloFactory : ICreateSimulationsForMonteCarloFactory
    {
        public CreateSimulationsForMonteCarloFactory(
            ICreateSimulationsForAllTemperaturesFactory allTemperatures, 
            ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory allTemperaturesAndSweeps,
            IExportFactory exportFactory,
            IMapper<Exporter> mapperExporter)
        {
            AllTemperaturesAndSweeps = allTemperaturesAndSweeps;
            ExportFactory = exportFactory;
            AllTemperatures = allTemperatures;
            MapperExporter = mapperExporter;
        }

        public IMapper<Exporter> MapperExporter { get; set; }

        public ICreateSimulationsForAllTemperaturesFactory AllTemperatures { get; }

        public ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory AllTemperaturesAndSweeps { get; }

        public IExportFactory ExportFactory { get; }



        public List<BaseSimulation> Create(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> simulationWithStochasticModels)
        {
            context.Result.MonteCarlo.Enabled = true;
            context.Result.MonteCarlo.RandomSeed = context.Result.SimulationConfiguration.Seed;
            context.Result.MonteCarlo.OutputVariable =
                context.Result.SimulationConfiguration.MonteCarloConfiguration.OutputVariable.Image;
            context.Result.MonteCarlo.Function = context.Result.SimulationConfiguration.MonteCarloConfiguration.Function;

            var result = new List<BaseSimulation>();

            if (context.Result.SimulationConfiguration.ParameterSweeps.Count == 0)
            {
                for (var i = 0; i < context.Result.SimulationConfiguration.MonteCarloConfiguration.Runs; i++)
                {
                    var simulations = AllTemperatures.CreateSimulations(statement, context, simulationWithStochasticModels);
                    AttachMonteCarloDataGathering(context, simulations);

                    result.AddRange(simulations);
                }
            }
            else
            {
                for (var i = 0; i < context.Result.SimulationConfiguration.MonteCarloConfiguration.Runs; i++)
                {
                    var simulations = AllTemperaturesAndSweeps.CreateSimulations(statement, context, simulationWithStochasticModels);
                    AttachMonteCarloDataGathering(context, simulations);
                    result.AddRange(simulations);
                }
            }

            return result;
        }

        protected void AttachMonteCarloDataGathering(IReadingContext context, IEnumerable<BaseSimulation> simulations)
        {
            foreach (var simulation in simulations)
            {
                AttachMonteCarloDataGatheringForSimulation(context, simulation);
            }
        }

        protected void AttachMonteCarloDataGatheringForSimulation(IReadingContext context, BaseSimulation simulation)
        {
            var exportParam = context.Result.SimulationConfiguration.MonteCarloConfiguration.OutputVariable;

            simulation.BeforeSetup += (sender, args) =>
            {
                Export export = context.Result.Exports.FirstOrDefault(e => e.Simulation == simulation && e.Name == exportParam.Image);

                if (export == null)
                {
                    export = ExportFactory.Create(exportParam, context, simulation, MapperExporter);
                }

                simulation.ExportSimulationData += (exportSender, exportArgs) =>
                    {
                        if (export != null)
                        {
                            var value = export.Extract();
                            context.Result.MonteCarlo.Collect(simulation, value);
                        }
                    };
            };
        }
    }
}
