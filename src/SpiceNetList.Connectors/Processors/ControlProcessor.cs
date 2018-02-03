using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Expressions;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    class ControlProcessor : StatementProcessor
    {
        protected SpiceExpression spiceExpressionParser = new SpiceExpression();
        Dictionary<string, SingleControlProcessor> ControlProcessors = new Dictionary<string, SingleControlProcessor>();

        public override void Init()
        {
            ControlProcessors["options"] = new OptionControl();
            ControlProcessors["param"] = new ParamControl();
            ControlProcessors["tran"] = new TransientControl();
            ControlProcessors["dc"] = new DCControl();
            ControlProcessors["op"] = new OPControl();
        }

        public override void Process(Statement statement, NetList netlist)
        {
            var control = statement as SpiceNetlist.SpiceObjects.Control;
            if (ControlProcessors.ContainsKey(control.Name))
            {
                ControlProcessors[control.Name].Process(control, netlist);
            }
        }
    }
}
