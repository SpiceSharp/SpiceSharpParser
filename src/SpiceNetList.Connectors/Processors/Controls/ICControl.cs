using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    public class ICControl : BaseControl
    {
        public override string Type => "ic";

        public override void Process(Control statement, ProcessingContext context)
        {
            foreach (var param in statement.Parameters)
            {
                if (param is SpiceObjects.Parameters.AssignmentParameter ap)
                {
                    string type = ap.Name.ToLower();
                    string initialValue = ap.Value;

                    if (type == "v" && ap.Arguments.Count == 1)
                    {
                        context.SetICVoltage(ap.Arguments[0], initialValue);
                    }
                    else
                    {
                        throw new System.Exception();
                    }
                }
            }
        }
    }
}
