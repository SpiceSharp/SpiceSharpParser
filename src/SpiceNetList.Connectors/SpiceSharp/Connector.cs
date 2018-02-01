using SpiceNetlist.SpiceObjects;
using SpiceNetList.Connectors.SpiceSharp.Processors;
using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SpiceNetList.Connectors.SpiceSharp
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
            NetList netList = new NetList
            {
                Circuit = new Circuit(),
                Title = netlist.Title
            };

            foreach (Statement statement in netlist.Statements.OrderBy(statement => GetStatementOrder(statement)))
            { 
                if (Processors.Supports(statement.GetType()))
                {
                    StatementProcessor processor = Processors.GetProcessor(statement.GetType());
                    processor.Process(statement, netList);
                }
            }

            return netList;
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
