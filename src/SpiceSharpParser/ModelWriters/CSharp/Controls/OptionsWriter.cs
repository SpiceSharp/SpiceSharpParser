using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;
using SpiceSharpParser.ModelWriters.CSharp.Language;

namespace SpiceSharpParser.ModelWriters.CSharp.Controls
{
    public class OptionsWriter : BaseWriter, IWriter<Control>
    {
        public List<CSharpStatement> Write(Control @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();
            result.Add(new CSharpFieldDeclaration("configuration", "Dictionary<string, double>"));

            foreach (var param in @object.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter a)
                {
                    string name = a.Name.ToLower();
                    string value = Evaluate(a.Value, context);
                    result.Add(new CSharpAssignmentStatement($@"this.configuration[""{name}""]", value));
                }

                if (param is Models.Netlist.Spice.Objects.Parameters.WordParameter w && w.Value.ToLower() == "keepopinfo")
                {
                    result.Add(new CSharpAssignmentStatement(@"this.configuration[""keepopinfo""]", "1"));
                }
            }

            result.ForEach(r => r.Kind = CSharpStatementKind.Configuration);

            return result;
        }
    }
}
