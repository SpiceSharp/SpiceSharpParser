using System;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    class OPControl : SingleControlProcessor
    {
        public override void Process(Control statement, NetList netlist)
        {
            var op = new OP(netlist.Simulations.Count(s => s is OP).ToString());
            netlist.Simulations.Add(op);
        }
    }
}
