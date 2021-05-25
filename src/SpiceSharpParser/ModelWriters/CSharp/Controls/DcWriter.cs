using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;
using SpiceSharpParser.ModelWriters.CSharp.Language;

namespace SpiceSharpParser.ModelWriters.CSharp.Controls
{
    public class DcWriter : BaseWriter, IWriter<Control>
    {
        public List<CSharpStatement> Write(Control @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.Parameters.Count < 4)
            {
                result.Add(new CSharpComment("Skipped, wrong parameters count:" + @object));
                return result;
            }

            var dcId = context.GetNewIdentifier("dc");
            int count = @object.Parameters.Count / 4;

            var sweepsId = dcId + "_sweeps";
            result.Add(new CSharpNewStatement(sweepsId, "new List<ISweep>()")
            {
                Kind = CSharpStatementKind.CreateSimulationInitBefore,
                Metadata = new Dictionary<string, string>() { { "type", typeof(DC).Name }, { "dependency", dcId } },
            });

            for (int i = 0; i < count; i++)
            {
                var start = Evaluate(@object.Parameters.Get((4 * i) + 1).Value, context);
                var stop = Evaluate(@object.Parameters.Get((4 * i) + 2).Value, context);
                var step = Evaluate(@object.Parameters.Get((4 * i) + 3).Value, context);
                result.Add(
                    new CSharpNewStatement(
                        sweepsId + "_" + i,
                        @$"new ParameterSweep(""{@object.Parameters.Get(4 * i).Value}"", Enumerable.Range(0, (int)(({stop} - {start}) / {step}) + 1).Select(index => {start} + (index * {step})))")
                    {
                        Kind = CSharpStatementKind.CreateSimulationInitBefore,
                        Metadata = new Dictionary<string, string>() { { "type", typeof(DC).Name }, { "dependency", dcId } },
                    });

                result.Add(new CSharpCallStatement(sweepsId, "Add(" + sweepsId + "_" + i + ")")
                {
                    Kind = CSharpStatementKind.CreateSimulationInitBefore,
                    Metadata = new Dictionary<string, string>() { { "type", typeof(DC).Name }, { "dependency", dcId } },
                });
            }

            result.Add(new CSharpNewStatement(dcId, @$"new DC(""{dcId}"",{sweepsId})")
            {
                Kind = CSharpStatementKind.CreateSimulation,
                Metadata = new Dictionary<string, string>() { { "type", typeof(DC).Name } },
            });

            return result;
        }
    }
}
