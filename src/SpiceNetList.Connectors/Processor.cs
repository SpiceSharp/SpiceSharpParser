using System;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors;
using SpiceSharp;

namespace SpiceNetlist.SpiceSharpConnector
{
    class Processor
    {
        private ModelProcessor modelProcessor;
        private ComponentProcessor componentProcessor;
        private SubcircuitDefinitionProcessor subcircuitDefinitionProcessor;
        private ControlsProcessor controlProcessor;

        public Processor()
        {
            modelProcessor = new ModelProcessor();
            controlProcessor = new ControlsProcessor();
            subcircuitDefinitionProcessor = new SubcircuitDefinitionProcessor();
            componentProcessor = new ComponentProcessor(modelProcessor);
        }

        internal NetList Process(SpiceNetlist.NetList netlist)
        {
            NetList result = new NetList
            {
                Circuit = new Circuit(),
                Title = netlist.Title
            };

            var rootContext = new ProcessingContext(string.Empty, result);

            foreach (Statement statement in netlist.Statements.OrderBy(StatementOrder))
            {
                var processor = GetProcessor(statement);
                if (processor != null)
                {
                    processor.Process(statement, rootContext);
                }
            }

            return result;
        }

        private int StatementOrder(Statement statement)
        {
            if (statement is Model)
            {
                return 200;
            }

            if (statement is Component)
            {
                return 300;
            }

            if (statement is SubCircuit)
            {
                return 100;
            }

            if (statement is Control c)
            {
                return 0 + controlProcessor.GetSubOrder(c);
            }

            if (statement is CommentLine)
            {
                return 0;
            }

            return -1;
        }

        private IStatementProcessor GetProcessor(Statement statement)
        {
            if (statement is Model)
            {
                return modelProcessor;
            }

            if (statement is Component)
            {
                return componentProcessor;
            }

            if (statement is SubCircuit)
            {
                return subcircuitDefinitionProcessor;
            }

            if (statement is Control)
            {
                return controlProcessor;
            }

            if (statement is CommentLine)
            {
                return null;
            }

            throw new System.Exception("Unsupported statement");
        }
    }
}
