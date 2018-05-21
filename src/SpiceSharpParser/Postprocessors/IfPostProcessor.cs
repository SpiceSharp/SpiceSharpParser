using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Circuits;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Evaluation;
using SpiceSharpParser.Connector.Processors.Controls;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser.Postprocessors
{
    /// <summary>
    /// Not smart .if/.endif "parser".
    /// </summary>
    public class IfPostProcessor
    {
        public IfPostProcessor(IEvaluator evaluator)
        {
            Evaluator = evaluator;
        }

        protected IEvaluator Evaluator { get; }

        //TODO: please do something about .ToLower() in so many places ....
        public Statements Process(Statements statements)
        {
            var processingContext = new IfPostProcessorProcessingContext(Evaluator);

            ParamControl paramControl = new ParamControl();
            foreach (Control param in statements.Where(statement => statement is Control c && c.Name.ToLower() == "param"))
            {
                paramControl.Process(param, processingContext);
            }

            return ProcessIfs(statements);
        }

        private Statements ProcessIfs(Statements statements)
        {
            //1. Find first .IF
            var firstIf = statements.FirstOrDefault(statement => statement is Control c && c.Name.ToLower() == "if");
            if (firstIf == null)
            {
                return statements;
            }
            var firstIfIndex = statements.IndexOf(firstIf);
            var result = (Statements)statements.Clone();

            //2. Find matching .ENDIF
            var matchedEndIfIndex = FindFirstMatched(result, firstIfIndex + 1, "endif");

            if (matchedEndIfIndex == statements.Count)
            {
                throw new Exception("Couldn't find matching .endif");
            }

            //3. Compute result of .if
            var ifResultStatements = ComputeIfResult(result, firstIfIndex, matchedEndIfIndex);

            //4.Replace .if statements with its result
            result.Replace(firstIfIndex, matchedEndIfIndex, ifResultStatements);

            //5. Do it again.
            result = ProcessIfs(result);

            return result;
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

        private IEnumerable<Statement> ComputeIfResult(Statements result, int ifIndex, int endIfIndex)
        {
            var ifControl = result[ifIndex] as Control;
            var ifCondition = ifControl.Parameters[0] as Model.SpiceObjects.Parameters.ExpressionParameter;

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

            if (Evaluator.EvaluateDouble(ifCondition.Image) >= 1.0)
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

    internal class IfPostProcessorProcessingContext : IProcessingContext
    {
        public IEvaluator Evaluator { get; set; }

        public IfPostProcessorProcessingContext(IEvaluator evaluator)
        {
            Evaluator = evaluator;
        }

        public string ContextName => throw new NotImplementedException();

        public IProcessingContext Parent => throw new NotImplementedException();

        public ICollection<IProcessingContext> Children => throw new NotImplementedException();

        public ICollection<SubCircuit> AvailableSubcircuits => throw new NotImplementedException();

        public IResultService Result => throw new NotImplementedException();

        public INodeNameGenerator NodeNameGenerator => throw new NotImplementedException();

        public IObjectNameGenerator ObjectNameGenerator => throw new NotImplementedException();

        public void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters)
        {
            throw new NotImplementedException();
        }

        public T FindModel<T>(string modelName) where T : Entity
        {
            throw new NotImplementedException();
        }

        public double ParseDouble(string expression)
        {
            throw new NotImplementedException();
        }

        public void SetICVoltage(string nodeName, string expression)
        {
            throw new NotImplementedException();
        }

        public void SetNodeSetVoltage(string nodeName, string expression)
        {
            throw new NotImplementedException();
        }

        public bool SetParameter(Entity entity, string parameterName, string expression)
        {
            throw new NotImplementedException();
        }

        public bool SetParameter(Entity entity, string parameterName, object @object)
        {
            throw new NotImplementedException();
        }
    }
}
