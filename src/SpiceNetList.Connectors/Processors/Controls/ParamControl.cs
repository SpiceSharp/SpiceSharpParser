using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    public class ParamControl : BaseControl
    {
        public override string Type => "param";

        public override void Process(Control statement, ProcessingContext context)
        {
            foreach (var param in statement.Parameters)
            {
                if (param is SpiceObjects.Parameters.AssignmentParameter a)
                {
                    string name = a.Name.ToLower();
                    string value = a.Value;

                    context.AvailableParameters[name] = context.ParseDouble(value);
                }
            }
        }
    }
}
