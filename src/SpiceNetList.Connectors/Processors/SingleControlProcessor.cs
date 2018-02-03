using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Expressions;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public abstract class SingleControlProcessor
    {
        protected SpiceExpression spiceExpressionParser = new SpiceExpression();

        public abstract void Process(Control statement, NetList netlist);
    }
}
