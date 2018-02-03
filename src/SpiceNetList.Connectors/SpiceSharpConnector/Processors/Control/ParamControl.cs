using System;
using SpiceNetlist.Connectors.SpiceSharpConnector.Expressions;
using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors.Control
{
    class ParamControl
    {
        SpiceExpression spiceExpressionParser = new SpiceExpression();

        internal void Process(Statement statement, NetList netlist)
        {
            var c = statement as SpiceNetlist.SpiceObjects.Control;

            foreach (var param in c.Parameters.Values)
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
