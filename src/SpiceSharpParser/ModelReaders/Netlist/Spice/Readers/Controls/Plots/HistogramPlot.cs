using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots
{
    /// <summary>
    /// Data for histogram plot.
    /// </summary>
    public class HistogramPlot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistogramPlot"/> class.
        /// </summary>
        /// <param name="name">The name of plot.</param>
        /// <param name="xUnit">Unit of x values.</param>
        /// <param name="xMin">Min value of x values.</param>
        /// <param name="xMax">Max value of x values.</param>
        /// <param name="binWidth">Width of one bin.</param>
        public HistogramPlot(string name, string xUnit, double xMin, double xMax, double binWidth)
        {
            Name = name;
            XUnit = xUnit;
            XMin = xMin;
            XMax = xMax;
            BinWidth = binWidth;
            Bins = new Dictionary<int, Bin>();
        }

        /// <summary>
        /// Gets width of histogram bin.
        /// </summary>
        public double BinWidth { get; }

        /// <summary>
        /// Gets the min value of x values.
        /// </summary>
        public double XMin { get; }

        /// <summary>
        /// Gets the max value of x values.
        /// </summary>
        public double XMax { get; }

        /// <summary>
        /// Gets the name of the plot.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the histogram x-unit.
        /// </summary>
        public string XUnit { get; }

        /// <summary>
        /// Gets the bins of histogram.
        /// </summary>
        public Dictionary<int, Bin> Bins { get; }
    }
}