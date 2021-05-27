using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelWriters.CSharp.Controls
{
    public class FuncWriter : IWriter<Control>
    {
        public static void CreateFunction(IWriterContext context, List<CSharpStatement> result, string functionName, string functionExpression, List<string> functionArguments)
        {
            if (functionArguments.Count == 0
                && !context.EvaluationContext.HaveSpiceProperties(functionExpression)
                && !context.EvaluationContext.HaveFunctions(functionExpression)
                && !context.EvaluationContext.HaveVariables(functionExpression))
            {
                var statements = new List<CSharpStatement>();
                statements.Add(new CSharpReturnStatement(functionExpression));

                result.Add(new CSharpMethod(
                    context.CurrentSubcircuitName != WriterContext.RootCircuitName ? null : true,
                    functionName,
                    "double",
                    functionArguments.ToArray(),
                    null,
                    new Type[0],
                    statements,
                    false)
                {
                    Local = context.CurrentSubcircuitName != WriterContext.RootCircuitName,
                });

                context.EvaluationContext.Functions.Add(functionName);
            }
            else
            {
                foreach (var argument in functionArguments)
                {
                    context.EvaluationContext.Variables[argument] = "null";
                }

                var parameterFunction = context.EvaluationContext.Transform(functionExpression);
                var statements = new List<CSharpStatement>();
                statements.Add(new CSharpReturnStatement(@"$""" + parameterFunction + @""""));

                result.Add(new CSharpMethod(
                    context.CurrentSubcircuitName != WriterContext.RootCircuitName ? null : true,
                    functionName,
                    "string",
                    functionArguments.ToArray(),
                    null,
                    functionArguments.Select(_ => typeof(string)).ToArray(),
                    statements,
                    false)
                {
                    Local = context.CurrentSubcircuitName != WriterContext.RootCircuitName,
                });

                foreach (var argument in functionArguments)
                {
                    context.EvaluationContext.Variables.Remove(argument);
                }

                context.EvaluationContext.Functions.Add(functionName);
            }
        }

        public List<CSharpStatement> Write(Control @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();
            if (@object.Parameters == null)
            {
                throw new ArgumentNullException(nameof(@object), "Parameters are null");
            }

            for (var i = 0; i < @object.Parameters.Count; i++)
            {
                var param = @object.Parameters[i];

                if (param is AssignmentParameter assignmentParameter)
                {
                    if (!assignmentParameter.HasFunctionSyntax)
                    {
                        continue;
                    }

                    string functionName = assignmentParameter.Name;
                    string functionExpression = assignmentParameter.Value;
                    List<string> functionArguments = assignmentParameter.Arguments;
                    CreateFunction(context, result, functionName, functionExpression, functionArguments);
                }
                else
                {
                    if (param is BracketParameter bracketParameter)
                    {
                        var arguments = new List<string>();

                        if (bracketParameter.Parameters[0] is VectorParameter vp)
                        {
                            arguments.AddRange(vp.Elements.Select(element => element.Value));
                        }
                        else
                        {
                            if (bracketParameter.Parameters.Count != 0)
                            {
                                arguments.Add(bracketParameter.Parameters[0].Value);
                            }
                        }

                        string functionName = bracketParameter.Name;
                        string functionExpression = @object.Parameters[i + 1].Value;
                        CreateFunction(context, result, functionName, functionExpression, arguments);

                        i++;
                    }
                }
            }

            return result;
        }
    }
}
