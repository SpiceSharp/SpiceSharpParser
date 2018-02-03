using System;
using SpiceNetlist.SpiceObjects;
using SpiceSharp.Simulations;
using System.Linq;
using SpiceNetlist.SpiceObjects.Parameters;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors.Controls
{
    class TransientControl : SingleControlProcessor
    {
        public override void Process(Control statement, NetList netlist)
        {
            Transient tran = null;

            switch (statement.Parameters.Values.Count)
            {
                case 0: throw new Exception("Step expected");
                case 1: throw new Exception("Maximum time expected");
            }

            switch (statement.Parameters.Values.Count)
            {
                case 2:
                    tran = new Transient("Transient -" + netlist.Simulations.Count(s => s is Transient),
                        spiceExpressionParser.Parse((statement.Parameters.Values[0] as ValueParameter).RawValue),
                        spiceExpressionParser.Parse((statement.Parameters.Values[1] as ValueParameter).RawValue));
                    break;
                case 3:
                    tran = new Transient("Transient -" + netlist.Simulations.Count(s => s is Transient),
                        spiceExpressionParser.Parse((statement.Parameters.Values[0] as ValueParameter).RawValue),
                        spiceExpressionParser.Parse((statement.Parameters.Values[1] as ValueParameter).RawValue),
                        spiceExpressionParser.Parse((statement.Parameters.Values[2] as ValueParameter).RawValue));
                    break;
                case 4:
                    //TODO: There is something wrong with this
                    throw new Exception("TODO");
            }

            netlist.Simulations.Add(tran);
        }
    }
}
