using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.Connectors.SpiceSharpConnector.Expressions;
using SpiceSharp.Circuits;
using System.Collections.Generic;
using System.Linq;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors
{
    public abstract class Generator
    {
        protected SpiceExpression expressionParser = new SpiceExpression();

        public abstract Entity Generate(string name, string type, ParameterCollection parameters, NetList currentNetList);

        public abstract List<string> GetGeneratedTypes();

        protected void SetParameters(Entity entity, ParameterCollection parameters, int toSkip = 0, int count = 0)
        {
            foreach (var parameter in parameters.Values.Skip(toSkip).Take(parameters.Values.Count - toSkip - count))
            {
                if (parameter is AssignmentParameter ap)
                {
                    entity.Parameters.SetProperty(ap.Name, expressionParser.Parse(ap.Value));
                }
            }
        }
    }
}
