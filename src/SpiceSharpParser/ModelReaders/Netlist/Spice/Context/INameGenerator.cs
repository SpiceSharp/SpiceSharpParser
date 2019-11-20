using System.Collections.Generic;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface INameGenerator
    {
        IEnumerable<string> Globals { get; }

        string ParseNodeName(string nodePath);

        string GenerateObjectName(string entityName);

        IObjectNameGenerator CreateChildNameGenerator(string name);

        string GenerateNodeName(string nodeName);

        void AddChild(INodeNameGenerator nodeNameGenerator);

        void SetGlobal(string spImage);
    }
}
