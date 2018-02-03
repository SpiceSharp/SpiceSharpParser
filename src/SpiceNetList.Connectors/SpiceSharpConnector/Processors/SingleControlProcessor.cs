using SpiceNetlist.Connectors.SpiceSharpConnector.Expressions;
using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors
{
    public abstract class SingleControlProcessor
    {
        protected SpiceExpression spiceExpressionParser = new SpiceExpression();

        public abstract void Process(Control statement, NetList netlist);
    }
}
