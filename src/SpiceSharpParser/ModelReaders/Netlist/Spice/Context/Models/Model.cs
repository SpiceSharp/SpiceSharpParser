using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public class Model
    {
        public Model(string name, IEntity entity, IParameterSet parameters)
        {
            Name = name;
            Entity = entity;
            Parameters = parameters;
        }

        public string Name { get; }

        public IEntity Entity { get; }

        public IParameterSet Parameters { get; }
    }
}
