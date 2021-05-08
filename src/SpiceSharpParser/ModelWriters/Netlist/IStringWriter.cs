using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelWriters.Netlist
{
    public interface IStringWriter
    {
        string Write(SpiceNetlist netlist);
    }
}