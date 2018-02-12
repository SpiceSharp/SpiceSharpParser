using System.Collections.Generic;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Expressions;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class NetList
    {
        public string Title { get; set; }

        public Circuit Circuit { get; set; }

        public List<Simulation> Simulations { get; set; } = new List<Simulation>();

        public List<string> Comments { get; set; }

        internal BaseConfiguration BaseConfiguration { get; set; }

        internal FrequencyConfiguration FrequencyConfiguration { get; set; }

        internal TimeConfiguration TimeConfiguration { get; set; }

        internal DCConfiguration DCConfiguration { get; set; }
    }
}
