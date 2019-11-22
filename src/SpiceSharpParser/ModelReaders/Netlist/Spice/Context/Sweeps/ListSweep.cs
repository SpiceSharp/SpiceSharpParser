using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps
{
    public class ListSweep : Sweep<double>
    {
        private readonly IEnumerable<double> _points;

        public ListSweep(IEnumerable<double> points)
        {
            _points = points ?? throw new ArgumentNullException(nameof(points));
        }

        public override IEnumerable<double> Points => _points;
    }
}