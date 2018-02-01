using SpiceSharp;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceNetList.Connectors.SpiceSharp
{
    public class NetList
    {
        public string Title { get; set; }

        public Circuit Circuit { get; set; }

        public Simulation Simulation { get; set; }

        public List<string> Comments { get; set; }

        public List<string> Warnings { get; set; }

        public List<string> Errors { get; set; }
    }
}
