using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls.Plots
{
    /// <summary>
    /// Data for plot with data series
    /// </summary>
    public class Plot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plot"/> class.
        /// </summary>
        /// <param name="name">The name of plot</param>
        public Plot(string name)
        {
            Name = name;
            Series = new List<Series>();
        }

        /// <summary>
        /// Gets the name of the plot
        /// </summary>
        public string Name { get;  }

        /// <summary>
        /// Gets the series in the plot
        /// </summary>
        public List<Series> Series { get; }

        /// <summary>
        /// Exports the plot's series to CSV
        /// </summary>
        /// <param name="seriesIndex">An index of series to export</param>
        /// <returns>
        /// A string with CSV
        /// </returns>
        public string ExportToCSV(int seriesIndex = 0)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var point in Series[seriesIndex].Points)
            {
                builder.AppendLine(point.X + ";" + point.Y + ";");
            }

            return builder.ToString();
        }
    }
}
