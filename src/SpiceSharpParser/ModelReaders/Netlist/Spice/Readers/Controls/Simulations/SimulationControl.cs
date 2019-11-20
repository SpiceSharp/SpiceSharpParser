using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

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
        protected void CreateSimulations(Control statement, ICircuitContext context, Func<string, Control, ICircuitContext, BaseSimulation> createSimulation)
        {
            _factory.Create(statement, context, createSimulation);
        }

        /// <summary>
        /// Sets the base parameters of a simulation.
        /// </summary>
        /// <param name="baseSimulation">The simulation to configure.</param>
        /// <param name="context">The reading context.</param>
        protected void ConfigureCommonSettings(BaseSimulation baseSimulation, ICircuitContext context)
        {
            var baseConfiguration = baseSimulation.Configurations.Get<BaseConfiguration>();
            if (context.Result.SimulationConfiguration.Gmin.HasValue)
            {
                baseConfiguration.Gmin = context.Result.SimulationConfiguration.Gmin.Value;
            }

            if (context.Result.SimulationConfiguration.AbsoluteTolerance.HasValue)
            {
                baseConfiguration.AbsoluteTolerance = context.Result.SimulationConfiguration.AbsoluteTolerance.Value;
            }

            if (context.Result.SimulationConfiguration.RelTolerance.HasValue)
            {
                baseConfiguration.RelativeTolerance = context.Result.SimulationConfiguration.RelTolerance.Value;
            }

            if (context.Result.SimulationConfiguration.DCMaxIterations.HasValue)
            {
                baseConfiguration.DcMaxIterations = context.Result.SimulationConfiguration.DCMaxIterations.Value;
            }

            baseSimulation.Configurations.Add(
                new CollectionConfiguration()
                {
                    VariableComparer = StringComparerProvider.Get(context.CaseSensitivity.IsNodeNameCaseSensitive),
                });
        }
    }
}
