using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.Parsers.Expression;
using System.Collections.Generic;
using System.Linq;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class SubCircuitWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var subCircuitDefinitionName = GetSubcircuitDefinitionName(@object.PinsAndParameters);
            context.RegisterDependency(context.CurrentSubcircuitName, subCircuitDefinitionName);

            var subCircuitId = context.GetNewIdentifier(@object.Name);
            GetAssignmentParametersCount(@object.PinsAndParameters, out var parameters);

            if (context.EvaluationContext.Variables.ContainsKey("M"))
            {
                parameters.Add(new AssignmentParameter() { Name = "internal_m", Value = "M" });
            }

            if (context.EvaluationContext.Variables.ContainsKey("m"))
            {
                parameters.Add(new AssignmentParameter() { Name = "internal_m", Value = "m" });
            }

            if (context.EvaluationContext.Variables.ContainsKey("internal_m"))
            {
                parameters.Add(new AssignmentParameter() { Name = "internal_m", Value = "internal_m" });
            }

            var subCircuitDefinitionId = context.GetIdentifier(subCircuitDefinitionName);

            var variables = new List<string>(context.EvaluationContext.Variables.Select(v => v.Key));
            variables.AddRange(parameters.Select(p => p.Name));
            variables = variables.Distinct().ToList();

            var transformer = new ExpressionTransformer(variables, context.EvaluationContext.Functions);

            var key = subCircuitDefinitionId + "_" + string.Join("__", parameters.Select(p =>
            {
                var node = Parser.Parse(Lexer.FromString(p.Value));
                var transformed = transformer.Transform(node);
                return $@"{p.Name}_{transformed}";
            }).ToArray());

            var dict = $@"definitions[$""{key}""]";

            var result = new List<CSharpStatement>();
            result.Add(new CSharpNewStatement(
                subCircuitId,
                $@"new Subcircuit(""{@object.Name}"", {dict})"));

            var pinNames = GetPinNames(@object.PinsAndParameters);
            result.Add(new CSharpCallStatement(subCircuitId, $@"Connect(" + string.Join(",", pinNames.Select(p => $@"""{p}""")) + ")"));

            if (!context.SubcircuitDictionaryPresent)
            {
                result.Add(new CSharpFieldDeclaration("definitions", "Dictionary<string, SubcircuitDefinition>"));
                context.SubcircuitDictionaryPresent = true;
            }

            var methodDefName = "CreateSubcircuitDefinition_" + subCircuitDefinitionName;

            var @params = new List<string>();

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Name == "internal_m" && context.ParentSubcircuit != null)
                {
                    continue;
                }

                var node = Parser.Parse(Lexer.FromString(parameters[i].Value));
                var transformed = transformer.Transform(node);

                transformed = $@"$""{transformed}""";

                if (parameters[i].Name == "M" || parameters[i].Name == "m")
                {
                    parameters[i].Name = "internal_m";
                }

                @params.Add(@$"{parameters[i].Name}:{transformed}");
            }

            var @this = "this." + dict;
            var @contains = @$"!this.definitions.ContainsKey($""{key}"")";
            var createStatement = new CSharpAssignmentStatement(@this, $"{@contains} ? {methodDefName}({string.Join(",", @params)}): {@this}", true);

            context.SubcircuitCreateStatements.Add((subCircuitDefinitionName, createStatement));

            return result;
        }

        private static int GetAssignmentParametersCount(ParameterCollection parameters, out List<AssignmentParameter> subCktParameters)
        {
            var parameterParameters = 0;
            subCktParameters = new List<AssignmentParameter>();
            while (true)
            {
                if (parameters[parameters.Count - parameterParameters - 1].Value.ToLower() == "params:")
                {
                    parameterParameters++;
                }

                if (!(parameters[parameters.Count - parameterParameters - 1] is AssignmentParameter a))
                {
                    break;
                }
                else
                {
                    subCktParameters.Add(a);
                    parameterParameters++;
                }
            }

            return parameterParameters;
        }

        private List<string> GetPinNames(ParameterCollection parameters)
        {
            var result = new List<string>();

            // setting evaluator
            var parameterParameters = GetAssignmentParametersCount(parameters, out _);

            // setting node name generator
            for (var i = 0; i < parameters.Count - parameterParameters - 1; i++)
            {
                var nodeName = parameters.Get(i).Value;

                result.Add(nodeName);
            }

            return result;
        }

        private string GetSubcircuitDefinitionName(ParameterCollection parameters)
        {
            // first step is to find subcircuit name in parameters, a=b parameters needs to be skipped
            int skipCount = 0;
            while (parameters[parameters.Count - skipCount - 1] is AssignmentParameter || parameters[parameters.Count - skipCount - 1].Value.ToLower() == "params:")
            {
                skipCount++;
            }

            return parameters.Get(parameters.Count - skipCount - 1).Value;
        }
    }
}
