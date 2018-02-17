using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ControlsProcessor : StatementProcessor<Control>
    {
        protected List<BaseControl> controls = new List<BaseControl>();

        public ControlsProcessor()
        {
            controls.AddRange(new List<BaseControl>()
            {
                new ParamControl(),
                new OptionControl(),
                new TransientControl(),
                new ACControl(),
                new DCControl(),
                new OPControl(),
                new SaveControl(),
                new ICControl()
            });
        }

        public override void Process(Control statement, ProcessingContext context)
        {
            string type = statement.Name.ToLower();

            foreach (var control in controls)
            {
                if (control.Type == type)
                {
                    control.Process(statement, context);
                }
            }
        }

        internal int GetSubOrder(Control c)
        {
            int i = 0;
            foreach (var control in controls)
            {
                if (control.Type == c.Name.ToLower())
                {
                    return i;
                }

                i++;
            }

            return 0;
        }
    }
}
