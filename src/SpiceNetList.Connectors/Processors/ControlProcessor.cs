using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ControlProcessor : StatementProcessor<Control>
    {
        protected ControlsRegistry registry = new ControlsRegistry();

        public ControlProcessor()
        {
            registry.Add(new ParamControl());
            registry.Add(new OptionControl());
            registry.Add(new TransientControl());
            registry.Add(new ACControl());
            registry.Add(new DCControl());
            registry.Add(new OPControl());
            registry.Add(new SaveControl());
            registry.Add(new ICControl());
        }

        public override void Process(Control statement, ProcessingContext context)
        {
            string type = statement.Name.ToLower();

            if (!registry.Supports(type))
            {
                throw new System.Exception("Unsupported control");
            }

            registry.GetControl(type).Process(statement, context);
        }

        internal int GetSubOrder(Control statement)
        {
            string type = statement.Name.ToLower();
            return registry.IndexOf(type);
        }
    }
}
