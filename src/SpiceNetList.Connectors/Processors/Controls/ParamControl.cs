using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    class ParamControl : SingleControlProcessor
    {
        public override void Process(Control statement, NetList netlist)
        {
            foreach (var param in statement.Parameters.Values)
            {
                if (param is SpiceObjects.Parameters.AssignmentParameter a)
                {
                    string name = a.Name.ToLower();
                    string value = a.Value;

                    netlist.UserGlobalParameters[name] = spiceExpressionParser.Parse(value);
                }
            }
        }
    }
}
