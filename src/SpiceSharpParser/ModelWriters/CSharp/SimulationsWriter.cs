using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.ModelWriters.CSharp.Language;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class SimulationsWriter
    {
        public List<CSharpStatement> Write(string methodName, List<CSharpStatement> simulationStatements, WriterContext context)
        {
            var result = new List<CSharpStatement>();

            var methodStatements = new List<CSharpStatement>();

            methodStatements.Add(new CSharpCallStatement("this", "CreateConfiguration()"));
            methodStatements.Add(new CSharpNewStatement("transactions", "new List<Simulation>()") { Kind = CSharpStatementKind.OtherSimulation });

            foreach (var createSimulation in simulationStatements.Where(s => s.Kind == CSharpStatementKind.CreateSimulation))
            {
                var variableName = ((CSharpNewStatement)createSimulation).VariableName;

                methodStatements.AddRange(simulationStatements.Where(s =>
                  s.Kind == CSharpStatementKind.CreateSimulationInitBefore
                  && s.Metadata.ContainsKey("dependency")
                  && s.Metadata["dependency"] == variableName));

                methodStatements.Add(createSimulation);

                methodStatements.AddRange(simulationStatements.Where(s =>
                    s.Kind == CSharpStatementKind.CreateSimulationInitAfter
                    && s.Metadata.ContainsKey("dependency")
                    && s.Metadata["dependency"] == variableName));

                foreach (var setSimulation in simulationStatements.Where(s => s.Kind == CSharpStatementKind.SetSimulation && s.Metadata.ContainsKey("type")))
                {
                    if (createSimulation.Metadata["type"] == setSimulation.Metadata["type"])
                    {
                        if (setSimulation is CSharpAssignmentStatement asg)
                        {
                            methodStatements.Add(new CSharpAssignmentStatement(asg.Left.Replace("{transactionId}", variableName), asg.ValueExpression));
                        }

                        if (setSimulation is CSharpConditionAssignmentStatement cond)
                        {
                            methodStatements.Add(new CSharpConditionAssignmentStatement(
                                cond.Condition.Replace("{transactionId}", variableName),
                                cond.Left.Replace("{transactionId}", variableName),
                                cond.ValueExpression));
                        }
                    }
                }

                methodStatements.Add(new CSharpCallStatement("transactions", $"Add({variableName})"));
            }

            methodStatements.Add(new CSharpReturnStatement("transactions"));
            result.Add(new CSharpMethod(true, methodName, "List<Simulation>", new string[0], new string[0], new Type[0], methodStatements, false));
            var configurationStatements = simulationStatements.Where(s => s.Kind == CSharpStatementKind.Configuration).ToList();

            result.Add(new CSharpMethod(true, "CreateConfiguration", "void", new string[0], new string[0], new Type[0], configurationStatements.Where(s => !(s is CSharpFieldDeclaration)).ToList(), false));
            return result;
        }
    }
}
