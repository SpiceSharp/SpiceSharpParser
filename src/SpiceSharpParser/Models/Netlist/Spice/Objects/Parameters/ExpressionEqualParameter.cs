using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    public class ExpressionEqualParameter : Parameter
    {
        public string Expression { get; set; }

        public Points Points { get; set; }

        public override string Image => Expression + " = (" + string.Join(",", Points.Select(p => p.Image)) + ")";

        public override SpiceObject Clone()
        {
            return this;
        }
    }
}
