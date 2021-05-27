using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelWriters.CSharp.Language;
using System;
using System.Linq;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class NetlistWriter
    {
        public NetlistWriter(StatementsWriter writer, SimulationsWriter simulationsWriter)
        {
            CircuitWriter = writer;
            SimulationsWriter = simulationsWriter;
        }

        protected StatementsWriter CircuitWriter { get; }

        protected SimulationsWriter SimulationsWriter { get; }

        public CSharpClass Write(string className, Statements statements)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            var context = new WriterContext();
            context.EvaluationContext = new EvaluationContext(
                new ExpressionParser(
                    new SpiceSharpBehavioral.Builders.Direct.RealBuilder(),
                    false));

            var allStatements = CircuitWriter.Write(
                true,
                false,
                "CreateCircuit",
                statements,
                new System.Collections.Generic.List<Models.Netlist.Spice.Objects.Parameters.AssignmentParameter>(),
                context,
                createSubCircuitDefinitions: true);

            var fields = allStatements.OfType<CSharpFieldDeclaration>().ToList();
            var methods = allStatements.OfType<CSharpMethod>().ToList();

            var simulationStatements = allStatements.Where(c => c.Kind != CSharpStatementKind.CreateEntity).ToList();
            var createSimulationStatements = SimulationsWriter.Write("CreateSimulations", simulationStatements, context);

            methods.AddRange(createSimulationStatements.OfType<CSharpMethod>());
            var resultClass = new CSharpClass(className, fields, methods);

            return resultClass;
        }
    }
}
