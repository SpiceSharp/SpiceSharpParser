using System.Collections.Generic;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Plots
{
    /// <summary>
    /// Data for histogram plot.
    /// </summary>
    public class HistogramPlot
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="HistogramPlot"/> class.
        /// </summary>
        /// <param name="name">The name of plot</param>
        /// <param name="xUnit">X-unit</param>
        /// <param name="xMin">Min value of X-Unit</param>
        /// <param name="xMax">Max value of X-Unit</param>
        public HistogramPlot(string name, string xUnit, double xMin, double xMax, double binWidth)
        {
            BinWidth = binWidth;
            XMax = xMax;
            XMin = xMin;
            Name = name;
            XUnit = xUnit;
            Bins = new Dictionary<int, Bin>();
        }

        public double BinWidth { get; }
        public double XMin { get; }
        public double XMax { get; }

        /// <summary>
        /// Gets the name of the plot.
        /// </summary>
        public string Name { get;  }

        /// <summary>
        /// Gets the histogram x-unit.
        /// </summary>
        public string XUnit { get; }

        /// <summary>
        /// Gets the bins.
        /// </summary>
        public Dictionary<int, Bin> Bins { get; }
    }
}
