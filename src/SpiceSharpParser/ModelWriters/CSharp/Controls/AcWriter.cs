using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;
using SpiceSharpParser.ModelWriters.CSharp.Language;

namespace SpiceSharpParser.ModelWriters.CSharp.Controls
{
    public class AcWriter : BaseWriter, IWriter<Control>
    {
        public List<CSharpStatement> Write(Control @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.Parameters.Count < 4)
            {
                result.Add(new CSharpComment("Skipped, wrong parameters count:" + @object));
                return result;
            }

            var acId = context.GetNewIdentifier("ac");
            string type = @object.Parameters[0].Value.ToLower();
            var numberSteps = Evaluate(@object.Parameters[1].Value, context);
            var start = Evaluate(@object.Parameters[2].Value, context);
            var stop = Evaluate(@object.Parameters[3].Value, context);

            switch (type)
            {
                case "lin":
                    result.Add(
                        new CSharpNewStatement(
                            acId,
                            @$"new AC(""{acId}"", new LinearSweep({start}, {stop}, (int){numberSteps}))")
                        {
                            Kind = CSharpStatementKind.CreateSimulation,
                            Metadata = new Dictionary<string, string>() { { "type", typeof(AC).Name } },
                        });
                    break;
                case "oct":
                    result.Add(
                        new CSharpNewStatement(
                            acId,
                            @$"new AC(""{acId}"", new OctaveSweep({start}, {stop}, (int){numberSteps}))")
                        {
                            Kind = CSharpStatementKind.CreateSimulation,
                            Metadata = new Dictionary<string, string>() { { "type", typeof(AC).Name } },
                        });
                    break;
                case "dec":
                    result.Add(
                        new CSharpNewStatement(
                            acId,
                            @$"new AC(""{acId}"", new DecadeSweep({start}, {stop}, (int){numberSteps}))")
                        {
                            Kind = CSharpStatementKind.CreateSimulation,
                            Metadata = new Dictionary<string, string>() { { "type", typeof(AC).Name } },
                        });
                    break;
            }

            return result;
        }
    }
}
