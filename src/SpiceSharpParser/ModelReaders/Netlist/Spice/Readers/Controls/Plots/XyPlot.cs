using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots
{
    /// <summary>
    /// Data for plot with data series.
    /// </summary>
    public class XyPlot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XyPlot"/> class.
        /// </summary>
        /// <param name="name">The name of plot.</param>
        public XyPlot(string name)
        {
            Name = name;
            Series = new List<Series>();
        }

        /// <summary>
        /// Gets the name of the plot.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the series in the plot.
        /// </summary>
        public List<Series> Series { get; }
    }
}