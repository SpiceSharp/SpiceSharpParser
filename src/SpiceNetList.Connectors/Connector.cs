using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors;
using SpiceSharp;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class Connector
    {
        Dictionary<Type, int> StatementOrder = new Dictionary<Type, int>();
        StatementProcessorRegistry Processors = new StatementProcessorRegistry();

        public Connector()
        {
            StatementOrder[typeof(Model)] = 0;
            StatementOrder[typeof(Component)] = 1;
            StatementOrder[typeof(Control)] = 2;

            Processors.Init();
        }

        public NetList Translate(SpiceNetlist.NetList netlist)
        {
            NetList result = new NetList
            {
                Circuit = new Circuit(),
                Title = netlist.Title
            };

            var rootContext = new ProcessingContext(string.Empty, result);

            foreach (Statement statement in netlist.Statements.OrderBy(statement => GetStatementOrder(statement)))
            {
                if (Processors.Supports(statement.GetType()))
                {
                    StatementProcessor processor = Processors.GetProcessor(statement.GetType());
                    processor.Process(statement, rootContext);
                }
            }

            return result;
        }

        protected int GetStatementOrder(Statement statement)
        {
            if (StatementOrder.ContainsKey(statement.GetType()))
            { 
                return StatementOrder[statement.GetType()];
            }
            return 0;
        }
    }
}
