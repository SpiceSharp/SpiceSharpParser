using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class ListSweep : Sweep<double>
    {
        private readonly IEnumerable<double> points;

        public ListSweep(IEnumerable<double> points)
        {
            this.points = points;
        }

        public override IEnumerable<double> Points => points;
    }
}
