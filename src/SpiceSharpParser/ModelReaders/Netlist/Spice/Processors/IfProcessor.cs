using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Processors;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    /// <summary>
    /// Not smart .if/.endif "parser".
    /// </summary>
    public class IfProcessor : IProcessor, IEvaluatorConsumer
    {
        public EvaluationContext EvaluationContext { get; set; }

        public ValidationEntryCollection Validation { get; set; }

        /// <summary>
        /// Gets or sets the evaluator.
        /// </summary>
        public SpiceNetlistCaseSensitivitySettings CaseSettings { get; set; }

        public Statements Process(Statements statements)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            ParamControl paramControl = new ParamControl();
            foreach (Control param in statements.Where(statement => statement is Control c && c.Name.ToLower() == "param").Cast<Control>())
            {
                paramControl.Read(param, EvaluationContext, Validation);
            }

            return ReadIfs(statements);
        }

        private static int FindFirstMatched(Statements result, int startIndex, string controlToFind)
        {
            int ifCount = 0;

            while (startIndex < result.Count)
            {
                if (result[startIndex] is Control c && c.Name.ToLower() == controlToFind && ifCount == 0)
                {
                    break;
                }

                if (result[startIndex] is Control c2 && c2.Name.ToLower() == "endif")
                {
                    ifCount--;
                }

                if (result[startIndex] is Control c3 && c3.Name.ToLower() == "if")
                {
                    ifCount++;
                }

                startIndex++;
            }

            return startIndex;
        }

        private Statements ReadIfs(Statements statements)
        {
            // 1. Find first .IF
            var firstIf = statements.FirstOrDefault(statement => statement is Control c && c.Name.ToLower() == "if");
            if (firstIf == null)
            {
                return statements;
            }

            var firstIfIndex = statements.IndexOf(firstIf);
            var result = (Statements)statements.Clone();

            // 2. Find matching .ENDIF
            var matchedEndIfIndex = FindFirstMatched(result, firstIfIndex + 1, "endif");

            if (matchedEndIfIndex == statements.Count)
            {
                Validation.AddError(ValidationEntrySource.Processor, "Cannot find matched .endif");
                return result;
            }

            // 3. Compute result of .if
            var ifResultStatements = ComputeIfResult(result, firstIfIndex, matchedEndIfIndex);

            // 4.Replace .if statements with its result
            result.Replace(firstIfIndex, matchedEndIfIndex, ifResultStatements);

            // 5. Do it again.
            result = ReadIfs(result);

            return result;
        }

        private IEnumerable<Statement> ComputeIfResult(Statements result, int ifIndex, int endIfIndex)
        {
            var ifControl = (Control)result[ifIndex];
            var ifCondition = (Models.Netlist.Spice.Objects.Parameters.ExpressionParameter)ifControl.Parameters[0];

            Control elseControl = null;
            Control elseIfControl = null;

            var elseControlIndex = FindFirstMatched(result, ifIndex + 1, "else");
            if (elseControlIndex != result.Count)
            {
                elseControl = result[elseControlIndex] as Control;
            }

            var elseIfControlIndex = FindFirstMatched(result, ifIndex + 1, "elseif");
            if (elseIfControlIndex != result.Count)
            {
                elseIfControl = result[elseIfControlIndex] as Control;
            }

            if (EvaluationContext.Evaluator.EvaluateDouble(ifCondition.Value) >= 1.0)
            {
                if (elseIfControl != null)
                {
                    return result.Skip(ifIndex + 1).Take(elseIfControlIndex - ifIndex - 1).ToList();
                }
                else
                {
                    if (elseControl == null)
                    {
                        return result.Skip(ifIndex + 1).Take(endIfIndex - ifIndex - 1).ToList();
                    }
                    else
                    {
                        return result.Skip(ifIndex + 1).Take(elseControlIndex - ifIndex - 1).ToList();
                    }
                }
            }
            else
            {
                if (elseIfControl != null)
                {
                    return ComputeIfResult(result, elseIfControlIndex, endIfIndex);
                }
                else
                {
                    if (elseControl == null)
                    {
                        return new List<Statement>();
                    }
                    else
                    {
                        return result.Skip(elseControlIndex + 1).Take(endIfIndex - elseControlIndex - 1).ToList();
                    }
                }
            }
        }
    }
}