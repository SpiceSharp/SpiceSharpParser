using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;
using SpiceSharpParser.ModelWriters.CSharp.Language;

namespace SpiceSharpParser.ModelWriters.CSharp.Controls
{
    public class ICWriter : BaseWriter, IWriter<Control>
    {
        public List<CSharpStatement> Write(Control @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            foreach (var param in @object.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter ap)
                {
                    string type = ap.Name.ToLower();
                    string initialValue = Evaluate(ap.Value, context);

                    if (type == "v" && ap.Arguments.Count == 1)
                    {
                        var nodeName = ap.Arguments[0];
                        result.Add(
                            new CSharpConditionAssignmentStatement(
                                "{transactionId} is Transient", @$"{{transactionId}}.TimeParameters.InitialConditions[""{nodeName}""]", $@"{initialValue}")
                            {
                                Kind = CSharpStatementKind.SetSimulation,
                                Metadata = new Dictionary<string, string>() { { "type", typeof(Transient).Name } },
                            });
                    }
                }
            }

            return result;
        }
    }
}
