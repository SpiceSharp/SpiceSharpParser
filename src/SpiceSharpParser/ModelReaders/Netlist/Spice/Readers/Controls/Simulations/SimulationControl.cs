using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Base for all control simulation readers.
    /// </summary>
    public abstract class SimulationControl : BaseControl
    {
        private readonly ISimulationsFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationControl"/> class.
        /// </summary>
        protected SimulationControl(IMapper<Exporter> exporterMapper)
        {
            _factory = new SimulationsFactory(exporterMapper);
        }

        /// <summary>
        /// Creates simulations.
        /// </summary>
        protected void CreateSimulations(Control statement, IReadingContext context, Func<string, Control, IReadingContext, Simulation> createSimulation)
        {
            _factory.Create(statement, context, createSimulation);
        }

        /// <summary>
        /// Sets the base parameters of a simulation.
        /// </summary>
        /// <param name="baseSimulation">The simulation to configure.</param>
        /// <param name="context">The reading context.</param>
        protected void ConfigureCommonSettings(Simulation baseSimulation, IReadingContext context)
        {
            if (baseSimulation is BiasingSimulation bs)
            {
                var biasingParameters = bs.BiasingParameters;

                if (context.SimulationConfiguration.Gmin.HasValue)
                {
                    biasingParameters.Gmin = context.SimulationConfiguration.Gmin.Value;
                }

                if (context.SimulationConfiguration.AbsoluteTolerance.HasValue)
                {
                    biasingParameters.AbsoluteTolerance = context.SimulationConfiguration.AbsoluteTolerance.Value;
                }

                if (context.SimulationConfiguration.RelTolerance.HasValue)
                {
                    biasingParameters.RelativeTolerance = context.SimulationConfiguration.RelTolerance.Value;
                }

                if (context.SimulationConfiguration.DCMaxIterations.HasValue)
                {
                    biasingParameters.DcMaxIterations = context.SimulationConfiguration.DCMaxIterations.Value;
                }
            }

            // TODO: Talk with Sven
            /*baseSimulation.Configurations.Add(
                new CollectionConfiguration()
                {
                    VariableComparer = StringComparerProvider.Get(context.CaseSensitivity.IsNodeNameCaseSensitive),
                });*/
        }
    }
}