using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Context
{
    public class ListSweep : Sweep<double>
    {
        private readonly IEnumerable<double> _points;

        public ListSweep(IEnumerable<double> points)
        {
            _points = points;
        }

        public override IEnumerable<double> Points => _points;
    }
}
