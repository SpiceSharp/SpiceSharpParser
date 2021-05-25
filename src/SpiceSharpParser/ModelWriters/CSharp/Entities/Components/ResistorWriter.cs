using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Linq;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class ResistorWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < 3)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(Resistor.ResistorPinCount);
            var parameters = @object.PinsAndParameters.Skip(Resistor.ResistorPinCount);
            var name = @object.Name;

            if (parameters.Count >= 1)
            {
                // RName Node1 Node2 something ...
                var something = parameters[0];
                string expression;

                if (something is AssignmentParameter asp)
                {
                    expression = asp.Value;
                }
                else
                {
                    expression = something.Value;
                }

                var resistorId = context.GetNewIdentifier(name);

                if (context.EvaluationContext.HaveSpiceProperties(expression))
                {
                    var mParameter = parameters.FirstOrDefault(p => p is AssignmentParameter p1 && p1.Name.ToLower() == "m");
                    var nParameter = parameters.FirstOrDefault(p => p is AssignmentParameter p1 && p1.Name.ToLower() == "n");

                    expression = MultiplyIfNeeded(expression, ((AssignmentParameter)mParameter)?.Value, ((AssignmentParameter)nParameter)?.Value);
                    result.Add(new CSharpNewStatement(resistorId, $@"new BehavioralResistor(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"",""{expression}"")"));
                }

                bool modelBased = (something is WordParameter || something is IdentifierParameter)
                                  && context.FindModelType(something.Value) != null;

                // Check if something is a model name
                if (!modelBased)
                {
                    result.Add(new CSharpNewStatement(resistorId, $@"new Resistor(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", {Evaluate(something.Value, context)})"));
                }
                else
                {
                    var modelName = something.Value;
                    result.Add(new CSharpNewStatement(resistorId, $@"new Resistor(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", ""{modelName})"""));
                }

                for (var i = 1; i < parameters.Count; i++)
                {
                    if (parameters[i] is AssignmentParameter assignmentParameter)
                    {
                        if (assignmentParameter.Name.ToLower() == "tc" || assignmentParameter.Name.ToLower() == "tc1" || assignmentParameter.Name.ToLower() == "tc2")
                        {
                            result.Add(new CSharpComment($"TC parameters for {name} resistor where skipped"));
                            continue;
                        }

                        if (assignmentParameter.Name.ToLower() != "m")
                        {
                            result.Add(SetParameter(resistorId, assignmentParameter.Name, assignmentParameter.Value, context));
                        }
                    }
                }

                SetParallelParameter(result, resistorId, parameters, context);
            }

            return result;
        }

        private string MultiplyIfNeeded(string expression, string mExpression, string nExpression)
        {
            if (!string.IsNullOrEmpty(mExpression) && !string.IsNullOrEmpty(nExpression))
            {
                return $"({expression} / {mExpression}) * {nExpression}";
            }

            if (!string.IsNullOrEmpty(mExpression))
            {
                return $"({expression} / {mExpression})";
            }

            if (!string.IsNullOrEmpty(nExpression))
            {
                return $"({expression} * {nExpression})";
            }

            return expression;
        }
    }
}
