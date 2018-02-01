using SpiceNetlist.SpiceObjects;
using SpiceSharp.Circuits;
using System.Collections.Generic;

namespace SpiceNetList.Connectors.SpiceSharp.Processors
{
    public abstract class Generator
    {
        public abstract Entity Generate(string name, string type, ParameterCollection parameters, NetList currentNetList);

        public abstract List<string> GetGeneratedTypes();
    }
}
