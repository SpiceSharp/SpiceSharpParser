using SpiceNetlist.SpiceObjects;
using SpiceNetList.Connectors.SpiceSharp.Processors;
using System;
using System.Collections.Generic;

namespace SpiceNetList.Connectors.SpiceSharp
{
    public class StatementProcessorRegistry
    {
        Dictionary<Type, StatementProcessor> Processors = new Dictionary<Type, StatementProcessor>();

        public StatementProcessorRegistry()
        {
            Processors[typeof(Component)] = new ComponentProcessor();
        }

        public void Init()
        {
            foreach (var processor in Processors.Values)
            {
                processor.Init();
            }
        }

        internal StatementProcessor GetProcessor(Type statementType)
        {
            return Processors[statementType];
        }

        internal bool Supports(Type statementType)
        {
            return Processors.ContainsKey(statementType);
        }
    }
}
