using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots
{
    /// <summary>
    /// Data series
    /// </summary>
    public class Series
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Series"/> class.
        /// </summary>
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
        /// Gets the x-unit
        /// </summary>
        public string XUnit { get; set; }

        /// <summary>
        /// Gets the y-unit
        /// </summary>
        public string YUnit { get; set; }
    }
}
