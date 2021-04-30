using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using ParameterSweep = SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps.ParameterSweep;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Configurations
{
    public class SimulationConfiguration
    {
        public double? AbsoluteTolerance { get; set; }

        public double? RelTolerance { get; set; }

        public double? Gmin { get; set; }

        public int? DCMaxIterations { get; set; }

        public int? SweepMaxIterations { get; set; }

        public TransientConfiguration TransientConfiguration { get; set; } = new TransientConfiguration();

        public Func<TransientConfiguration, TimeParameters> TimeParametersFactory { get; set; }

        public bool? KeepOpInfo { get; set; }

        /// <summary>
        /// Gets or sets the random seed for simulations.
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// Gets or sets the nominal temperature for the circuit
        /// Used for model parameters as the default.
        /// </summary>
        public double? NominalTemperatureInKelvins { get; set; }

        /// <summary>
        /// Gets or sets temperatures for this circuit.
        /// </summary>
        public List<double> TemperaturesInKelvins { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets value of circuit temperature from .OPTIONS.
        /// </summary>
        public double? TemperaturesInKelvinsFromOptions { get; set; }

        /// <summary>
        /// Gets the parameter sweeps.
        /// </summary>
        public List<ParameterSweep> ParameterSweeps { get; } = new List<ParameterSweep>();

        /// <summary>
        /// Gets the Monte Carlo Analysis configuration.
        /// </summary>
        public MonteCarloConfiguration MonteCarloConfiguration { get; } = new MonteCarloConfiguration();

        /// <summary>
        /// Gets or sets the value indicating whether local solver should be used for subcircuits.
        /// </summary>
        public bool LocalSolver { get; set; }
    }
}