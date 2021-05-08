using SpiceSharp.Entities;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class EntityUpdate
    {
        public EntityUpdate()
        {
            ParameterUpdatesBeforeTemperature = new List<EntityParameterUpdate>();
        }

        public IEntity Entity { get; set; }

        public List<EntityParameterUpdate> ParameterUpdatesBeforeTemperature { get; }
    }
}