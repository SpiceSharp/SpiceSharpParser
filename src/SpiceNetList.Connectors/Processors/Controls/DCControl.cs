using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    class DCControl : SingleControlProcessor
    {
        public override void Process(Control statement, NetList netlist)
        {
            int count = statement.Parameters.Values.Count / 4;
            switch (statement.Parameters.Values.Count - 4 * count)
            {
                case 0:
                    if (statement.Parameters.Values.Count == 0)
                        throw new Exception("Source st.Name expected");
                    break;
                case 1: throw new Exception("Start value expected");
                case 2: throw new Exception("Stop value expected");
                case 3: throw new Exception("Step value expected");
            }

            // Format: .DC SRCNAM VSTART VSTOP VINCR [SRC2 START2 STOP2 INCR2]
            List<SweepConfiguration> sweeps = new List<SweepConfiguration>();

            for (int i = 0; i < count; i++)
            {
                SweepConfiguration sweep = new SweepConfiguration(
                    (statement.Parameters.Values[4 * i] as SingleParameter).RawValue,
                    spiceExpressionParser.Parse((statement.Parameters.Values[4 * i + 1] as SingleParameter).RawValue),
                    spiceExpressionParser.Parse((statement.Parameters.Values[4 * i + 2] as SingleParameter).RawValue),
                    spiceExpressionParser.Parse((statement.Parameters.Values[4 * i + 3] as SingleParameter).RawValue));

                sweeps.Add(sweep);
            }

            DC dc = new DC("DC " + (netlist.Simulations.Count + 1), sweeps);
            netlist.Simulations.Add(dc);
        }
    }
}
