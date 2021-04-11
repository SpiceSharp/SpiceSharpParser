using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Results of Monte Carlo Analysis.
    /// </summary>
    public class MonteCarloResult
    {
        private readonly object _locker = new object();

        /// <summary>
        /// Gets or sets a value indicating whether MC analysis was executed.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the random seed for Monte Carlo simulations evaluators.
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of max values.
        /// </summary>
        public Dictionary<Simulation, double> Max { get; protected set; } = new Dictionary<Simulation, double>();

        /// <summary>
        /// Gets or sets the dictionary of min values.
        /// </summary>
        public Dictionary<Simulation, double> Min { get; protected set; } = new Dictionary<Simulation, double>();

        /// <summary>
        /// Gets or sets the output variable name.
        /// </summary>
        public string OutputVariable { get; set; }

        /// <summary>
        /// Gets or sets the function name.
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// Collects the result and updates <see cref="Max"/>, <see cref="Min"/> dictionaries.
        /// </summary>
        /// <param name="simulation">Simulation of the result.</param>
        /// <param name="result">Result value.</param>
        public void Collect(Simulation simulation, double result)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            lock (_locker)
            {
                if (Min.ContainsKey(simulation))
                {
                    Min[simulation] = Math.Min(Min[simulation], result);
                }
                else
                {
                    Min[simulation] = result;
                }

                if (Max.ContainsKey(simulation))
                {
                    Max[simulation] = Math.Max(Max[simulation], result);
                }
                else
                {
                    Max[simulation] = result;
                }
            }
        }

        /// <summary>
        /// Gets the plot from Monte Carlo results.
        /// </summary>
        /// <param name="bins">Number of bins.</param>
        /// <returns>
        /// A plot.
        /// </returns>
        public HistogramPlot GetPlot(int bins)
        {
            switch (Function.ToUpper())
            {
                case "MAX":
                    return GetMaxPlot(bins);

                case "MIN":
                    return GetMinPlot(bins);

                case "YMAX":
                    return GetYMaxPlot(bins);
            }

            throw new SpiceSharpParserException($"Unknown Monte Carlo function:{Function}");
        }

        protected HistogramPlot GetYMaxPlot(int bins)
        {
            var values = new List<double>();

            foreach (var simulation in Max.Keys)
            {
                values.Add(Math.Abs(Max[simulation] - Min[simulation]));
            }

            return CreatePlot("YMAX - " + OutputVariable, bins, values);
        }

        protected HistogramPlot GetMaxPlot(int bins)
        {
            var values = Max.Values.ToList();
            return CreatePlot("MAX - " + OutputVariable, bins, values);
        }

        protected HistogramPlot GetMinPlot(int bins)
        {
            var values = Min.Values.ToList();
            return CreatePlot("MIN - " + OutputVariable, bins, values);
        }

        protected HistogramPlot CreatePlot(string title, int bins, List<double> values)
        {
            var max = values.Max();
            var min = values.Min();

            var binWidth = (max - min) / bins;
            if (Math.Abs(binWidth) < 1e-16)
            {
                bins = 1;
                binWidth = 0;
            }

            var plot = new HistogramPlot(title, OutputVariable, min, max, binWidth);

            for (var i = 1; i <= bins; i++)
            {
                plot.Bins[i] = new Bin { Value = (binWidth * (i - 1)) + min };
            }

            foreach (var value in values)
            {
                int binIndex = binWidth != 0 ? (int)Math.Floor((value - min) / binWidth) + 1 : bins;

                if (binIndex > bins)
                {
                    binIndex = bins;
                }

                plot.Bins[binIndex].Count += 1;
            }

            return plot;
        }
    }
}