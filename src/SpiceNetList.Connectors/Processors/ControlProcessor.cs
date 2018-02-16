using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ControlProcessor : StatementProcessor
    {
        Dictionary<string, SingleControlProcessor> ControlProcessors = new Dictionary<string, SingleControlProcessor>();

        public override void Init()
        {
            ControlProcessors["options"] = new OptionControl();
            ControlProcessors["param"] = new ParamControl();
            ControlProcessors["tran"] = new TransientControl();
            ControlProcessors["ac"] = new ACControl();
            ControlProcessors["dc"] = new DCControl();
            ControlProcessors["op"] = new OPControl();
            ControlProcessors["save"] = new SaveControl();
            ControlProcessors["ic"] = new ICControl();
        }

        public override void Process(Statement statement, ProcessingContext context)
        {
            var control = statement as Control;
            string type = control.Name.ToLower();

            if (ControlProcessors.ContainsKey(type))
            {
                ControlProcessors[type].Process(control, context);
            }
        }

        public List<string> GetOrder()
        {
            return new List<string>() { "options", "param", "ic", "ac", "dc", "op", "tran", "save" };
        }
    }
}
