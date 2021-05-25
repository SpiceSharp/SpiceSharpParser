using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using SpiceSharpParser.ModelWriters.CSharp.Language;

namespace SpiceSharpParser.ModelWriters.CSharp.Controls
{
    public class TransientWriter : BaseWriter, IWriter<Control>
    {
        public List<CSharpStatement> Write(Control @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.Parameters.Count < 2)
            {
                result.Add(new CSharpComment("Skipped, wrong parameters count:" + @object));
                return result;
            }

            var transientId = context.GetNewIdentifier("t");
            bool useIc = false;
            var clonedParameters = (ParameterCollection)@object.Parameters.Clone();
            var lastParameter = clonedParameters[clonedParameters.Count - 1];
            if (lastParameter is WordParameter w && w.Value.ToLower() == "uic")
            {
                useIc = true;
                clonedParameters.RemoveAt(clonedParameters.Count - 1);
            }

            string maxStep = null;
            string step = null;
            string final = null;
            string start = null;

            switch (clonedParameters.Count)
            {
                case 2:
                    step = Evaluate(clonedParameters[0].Value, context);
                    final = Evaluate(clonedParameters[1].Value, context);
                    break;
                case 3:
                    step = Evaluate(clonedParameters[0].Value, context);
                    final = Evaluate(clonedParameters[1].Value, context);
                    maxStep = Evaluate(clonedParameters[2].Value, context);
                    break;
                case 4:
                    step = Evaluate(clonedParameters[0].Value, context);
                    final = Evaluate(clonedParameters[1].Value, context);
                    start = Evaluate(clonedParameters[2].Value, context);
                    maxStep = Evaluate(clonedParameters[3].Value, context);
                    break;
            }

            if (clonedParameters.Count == 2)
            {
                result.Add(
                    new CSharpNewStatement(
                        transientId,
                        @$"new Transient(""{transientId}"", {step}, {final})")
                    {
                        Kind = CSharpStatementKind.CreateSimulation,
                        Metadata = new Dictionary<string, string>() { { "type", typeof(Transient).Name } },
                    });
            }
            else
            {
                if (clonedParameters.Count == 3)
                {
                    result.Add(
                        new CSharpNewStatement(
                            transientId,
                            @$"new Transient(""{transientId}"", {step}, {final}, {maxStep ?? step})")
                        {
                            Kind = CSharpStatementKind.CreateSimulation,
                            Metadata = new Dictionary<string, string>() { { "type", typeof(Transient).Name } },
                        });
                }
                else
                {
                    result.Add(new CSharpNewStatement(
                        transientId,
                        @$"new Transient(""{transientId}"", new Trapezoidal() {{ StartTime = {start ?? "0.0"}, StopTime = {final}, MaxStep = {maxStep ?? step}}}, InitialStep = {step}}})")
                    {
                        Kind = CSharpStatementKind.CreateSimulation,
                        Metadata = new Dictionary<string, string>() { { "type", typeof(Transient).Name } },
                    });
                }
            }

            result.Add(new CSharpAssignmentStatement(@$"{transientId}.TimeParameters.UseIc", $"{useIc.ToString().ToLower()}")
            {
                Kind = CSharpStatementKind.CreateSimulationInitAfter,
                Metadata = new Dictionary<string, string>() { { "type", typeof(Transient).Name }, { "dependency", transientId } },
            });

            return result;
        }
    }
}
