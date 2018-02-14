using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class StatementProcessorRegistry
    {
        Dictionary<Type, StatementProcessor> Processors = new Dictionary<Type, StatementProcessor>();

        public StatementProcessorRegistry()
        {
            var modelProcessor = new ModelProcessor();

            Processors[typeof(Component)] = new ComponentProcessor(modelProcessor);
            Processors[typeof(Model)] = modelProcessor;
            Processors[typeof(Control)] = new ControlProcessor();
            Processors[typeof(SubCircuit)] = new SubcircuitDefinitionProcessor();
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
