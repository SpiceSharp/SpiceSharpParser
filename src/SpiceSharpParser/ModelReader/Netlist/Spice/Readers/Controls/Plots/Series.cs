using System.Collections.Generic;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls.Plots
{
    /// <summary>
    /// Data series
    /// </summary>
    public class Series
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Series"/> class.
        /// </summary>
        /// <param name="name">Name of the series</param>
        public Series(string name)
        {
            Points = new List<Point>();
            Name = name;
        }

        /// <summary>
        /// Gets the series points
        /// </summary>
        public List<Point> Points { get; }

        /// <summary>
        /// Gets the name of series
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the x-unit
        /// </summary>
        public string XUnit { get; set; }

        /// <summary>
        /// Gets or sets the y-unit
        /// </summary>
        public string YUnit { get; set; }
    }
}
