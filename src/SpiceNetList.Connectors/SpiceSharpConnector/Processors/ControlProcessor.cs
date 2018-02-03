using SpiceNetlist.Connectors.SpiceSharpConnector.Processors.Control;
using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors.Generators
{
    class ControlProcessor : StatementProcessor
    {
        public override void Init()
        {
        }

        public override void Process(Statement statement, NetList netlist)
        {
            var control = statement as SpiceNetlist.SpiceObjects.Control;

            if (control.Name == "options")
            {
                var optionControl = new OptionControl();
                optionControl.Process(statement, netlist);
            }

            if (control.Name == "param")
            {
                var paramControl = new ParamControl();
                paramControl.Process(statement, netlist);
            }
        }
    }
}
