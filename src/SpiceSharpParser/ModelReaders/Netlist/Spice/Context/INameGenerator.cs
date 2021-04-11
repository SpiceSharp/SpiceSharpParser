using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface INameGenerator
    {
        IEnumerable<string> Globals { get; }

        INodeNameGenerator NodeNameGenerator { get; }

        string ParseNodeName(string nodePath);

        string GenerateObjectName(string entityName);

        IObjectNameGenerator CreateChildNameGenerator(string name);

        string GenerateNodeName(string nodeName);

        void AddChild(INodeNameGenerator nodeNameGenerator);

        void SetGlobal(string spImage);
    }
}