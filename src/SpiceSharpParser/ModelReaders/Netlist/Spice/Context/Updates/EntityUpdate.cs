using System.Collections.Generic;
using SpiceSharp.Circuits;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class EntityUpdate
    {
        public EntityUpdate()
        {
            ParameterUpdatesBeforeLoad = new List<EntityParameterUpdate>();
            ParameterUpdatesBeforeTemperature = new List<EntityParameterUpdate>();
        }

        public Entity Entity { get; set; }

        public List<EntityParameterUpdate> ParameterUpdatesBeforeLoad { get; protected set; }

        public List<EntityParameterUpdate> ParameterUpdatesBeforeTemperature { get; protected set; }
    }
}
