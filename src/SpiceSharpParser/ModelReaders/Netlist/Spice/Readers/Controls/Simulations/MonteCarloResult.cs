using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Plots;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Simulations
{
    public class MonteCarloResult
    {
        public bool Enabled { get; set; }

        public Dictionary<Simulation, double> Max { get; set; } = new Dictionary<Simulation, double>();

        public Dictionary<Simulation, double> Min { get; set; } = new Dictionary<Simulation, double>();

        public string VariableName { get; set; }

        /// <summary>
        /// Collects the result and updates <see cref="Max"/>, <see cref="Min"/> dictionaries.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="result">Result.</param>
        public void Collect(Simulation simulation, double result)
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
                Max[simulation] = Math.Max(Min[simulation], result);
            }
            else
            {
                Max[simulation] = result;
            }
        }

        public HistogramPlot GetMaxPlot(int bins)
        {
            var values = Max.Values.ToList();
            return CreatePlot("Max", bins, values);
        }

        public HistogramPlot GetMinPlot(int bins)
        {
            var values = Max.Values.ToList();
            return CreatePlot("Min", bins, values);
        }

        private HistogramPlot CreatePlot(string title, int bins, List<double> values)
        {
            var max = values.Max();
            var min = values.Min();
            var binWidth = (max - min) / bins;

            var plot = new HistogramPlot(title, VariableName, min, max, binWidth);

            foreach (var value in values)
            {
                var binIndex = (int)Math.Floor((value - min) / binWidth) + 1;

                if (value == max)
                {
                    binIndex = bins;
                }

                if (plot.Bins.ContainsKey(binIndex))
                {
                    plot.Bins[binIndex].Count += 1;
                }
                else
                {
                    plot.Bins[binIndex] = new ModelReaders.Netlist.Spice.Readers.Controls.Plots.Bin();
                    plot.Bins[binIndex].Count = 1;
                    plot.Bins[binIndex].Value = ((binWidth * binIndex) + min);
                }
            }

            return plot;
        }
    }
}
