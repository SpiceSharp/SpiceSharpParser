using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class BaseWriter
    {
        public CSharpStatement SetProperty(string identifier, string property, string expression, IWriterContext context)
        {
            if (context.EvaluationContext.HaveFunctions(expression)
                || context.EvaluationContext.HaveVariables(expression))
            {
                var functionExpression = context.EvaluationContext.Transform(expression);
                return new CSharpAssignmentStatement(identifier + "." + property, $@"SpiceHelper.ParseNumber($""{functionExpression}"")");
            }
            else
            {
                return new CSharpAssignmentStatement(identifier + "." + property, context.EvaluationContext.Evaluate(expression).ToString(CultureInfo.InvariantCulture));
            }
        }

        public CSharpStatement SetParameter(string identifier, string parameter, string expression, IWriterContext context)
        {
            if (context.EvaluationContext.HaveFunctions(expression)
                || context.EvaluationContext.HaveVariables(expression))
            {
                var functionExpression = context.EvaluationContext.Transform(expression);
                return new CSharpCallStatement(identifier, $@"SetParameter(""{parameter}"", SpiceHelper.ParseNumber($""{functionExpression}""))");
            }
            else
            {
                return new CSharpCallStatement(identifier, $@"SetParameter(""{parameter}"", {context.EvaluationContext.Evaluate(expression)}d)");
            }
        }

        public CSharpStatement SetParameter(string identifier, string parameter, double value, IWriterContext context)
        {
            return new CSharpCallStatement(identifier, $@"SetParameter(""{parameter}"", {value}d)");
        }

        public CSharpStatement SetParameter(string identifier, string parameter, bool value, IWriterContext context)
        {
            string boolString = value ? "true" : "false";
            return new CSharpCallStatement(identifier, $@"SetParameter(""{parameter}"", {boolString})");
        }

        public string Evaluate(string expression, IWriterContext context)
        {
            if (context.EvaluationContext.HaveFunctions(expression)
              || context.EvaluationContext.HaveVariables(expression))
            {
                var functionExpression = context.EvaluationContext.Transform(expression);

                return $@"SpiceHelper.ParseNumber($""{functionExpression}"")";
            }
            else
            {
                return context.EvaluationContext.Evaluate(expression).ToString(CultureInfo.InvariantCulture);
            }
        }

        public void SetParallelParameter(List<CSharpStatement> result, string id, ParameterCollection parameters, IWriterContext context)
        {
            var mParameter = parameters.FirstOrDefault(p => p is AssignmentParameter asg && asg.Name.ToLower() == "m");
            if (mParameter != null)
            {
                if (context.EvaluationContext.Variables.ContainsKey("m"))
                {
                    result.Add(SetParameter(id, "m", $"({mParameter.Value}) * (m)", context));
                }
                else
                {
                    result.Add(SetParameter(id, "m", mParameter.Value, context));
                }
            }
            else
            {
                if (context.EvaluationContext.Variables.ContainsKey("m"))
                {
                    result.Add(SetParameter(id, "m", $"M", context));
                }
            }
        }
    }
}
