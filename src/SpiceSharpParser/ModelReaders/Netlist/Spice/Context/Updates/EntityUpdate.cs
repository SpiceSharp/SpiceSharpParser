using SpiceSharp.Entities;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class EntityUpdate
    {
        public EntityUpdate()
        {
            ParameterUpdatesBeforeLoad = new List<EntityParameterUpdate>();
            ParameterUpdatesBeforeTemperature = new List<EntityParameterUpdate>();
        }

        public IEntity Entity { get; set; }

        public List<EntityParameterUpdate> ParameterUpdatesBeforeLoad { get; }

        public List<EntityParameterUpdate> ParameterUpdatesBeforeTemperature { get; }
    }
}