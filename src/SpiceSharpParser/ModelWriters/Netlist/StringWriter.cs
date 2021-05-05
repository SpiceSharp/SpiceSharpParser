using System;

namespace SpiceSharpParser.ModelWriters.Netlist
{
    public class StringWriter : IStringWriter
    {
        public string Write(Models.Netlist.Spice.SpiceNetlist netlist)
        {
            if (netlist == null)
            {
                throw new ArgumentNullException(nameof(netlist));
            }

            return netlist.ToString();
        }
    }
}
