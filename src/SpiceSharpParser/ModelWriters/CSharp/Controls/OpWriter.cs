using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;
using SpiceSharpParser.ModelWriters.CSharp.Language;

namespace SpiceSharpParser.ModelWriters.CSharp.Controls
{
    public class OpWriter : IWriter<Control>
    {
        public List<CSharpStatement> Write(Control @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            var opId = context.GetNewIdentifier("o");
            result.Add(
                new CSharpNewStatement(
                    opId,
                    @$"new OP(""{opId}"")")
                {
                    Kind = CSharpStatementKind.CreateSimulation,
                    Metadata = new Dictionary<string, string>() { { "type", typeof(OP).Name } },
                });

            return result;
        }
    }
}
