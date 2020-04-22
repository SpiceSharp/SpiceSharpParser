using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps
{
    public class ListSweep : IEnumerable<double>
    {
        private IEnumerable<double> points;

        public ListSweep(IEnumerable<double> points)
        {
            this.points = points;
        }

        public IEnumerator<double> GetEnumerator()
        {
            return points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return points.GetEnumerator();
        }
    }
}