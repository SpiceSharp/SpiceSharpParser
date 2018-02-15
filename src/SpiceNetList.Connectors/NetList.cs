using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class NetList : SpiceObject
    {
        public string Title { get; set; }

        public Circuit Circuit { get; set; }

        public List<Simulation> Simulations { get; set; } = new List<Simulation>();

        public List<string> Comments { get; set; } = new List<string>();

        public List<string> Warnings { get; set; } = new List<string>();

        public List<SpiceSharp.Parser.Readers.Export> Exports { get; set; } = new List<SpiceSharp.Parser.Readers.Export>();
    }
}
