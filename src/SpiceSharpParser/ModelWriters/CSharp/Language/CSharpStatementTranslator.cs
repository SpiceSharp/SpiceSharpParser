using System.Linq;
using System.Text;
using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpStatementTranslator
    {
        public string GetCSharpCode(CSharpClass @class)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"using System;");
            builder.AppendLine($"using System.Collections.Generic;");
            builder.AppendLine($"using SpiceSharp;");
            builder.AppendLine($"using SpiceSharp.Components;");
            builder.AppendLine($"using SpiceSharp.Simulations;");
            builder.AppendLine($"using SpiceSharp.Entities;");
            builder.AppendLine($"using SpiceSharpBehavioral.Parsers;");
            builder.AppendLine($"using System.Linq;");
            builder.AppendLine();

            builder.AppendLine($"public class {@class.ClassName}");
            builder.AppendLine("{");
            builder.AppendLine();

            foreach (var field in @class.Fields)
            {
                builder.Append(GetCSharpCode(field, 5));
                builder.AppendLine();
            }

            if (@class.Fields.Any())
            {
                builder.AppendLine();
            }

            foreach (var method in @class.Methods.OrderBy(m => m.MethodName))
            {
                builder.Append(GetCSharpCode(method, 5));
                builder.AppendLine();
            }

            builder.AppendLine("}");

            return builder.ToString();
        }

        private string GetCSharpCode(CSharpMethod method, int spaces)
        {
            var builder = new StringBuilder();
            var returnType = method.ReturnType;

            builder.Append(GetSpace(spaces) + (method.IsPublic != null ? (method.IsPublic.Value ? "public " : "private ") : string.Empty) + $"{returnType} {method.MethodName}(");

            for (var i = 0; i < method.ArgumentNames.Length; i++)
            {
                builder.Append(method.ArgumentTypes[i].Name + " " + method.ArgumentNames[i] + (method.OptionalArguments ? " = null" : string.Empty));

                if (i != method.ArgumentNames.Length - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(")");
            builder.AppendLine();
            builder.AppendLine(GetSpace(spaces) + "{");

            if (method.DefaultArgumentValues != null && method.OptionalArguments)
            {
                for (var i = 0; i < method.DefaultArgumentValues.Length; i++)
                {
                    var value = method.DefaultArgumentValues[i];
                    if (value != null)
                    {
                        var node = Parser.Parse(Lexer.FromString(value));
                        var transformed = new ExpressionTransformer(new System.Collections.Generic.List<string>(), new System.Collections.Generic.List<string>()).Transform(node);
                        builder.AppendLine(GetSpace(spaces + 5) + @$"if ({method.ArgumentNames[i]} == null) {method.ArgumentNames[i]} = $""{transformed}"";");
                    }
                }

                builder.AppendLine();
            }

            foreach (var statement in method.Statements)
            {
                if (statement is CSharpMethod)
                {
                    builder.AppendLine();
                }

                var code = GetCSharpCode(statement, spaces + 5);
                if (code != null)
                {
                    builder.Append(code);
                    builder.AppendLine();
                }
            }

            builder.AppendLine(GetSpace(spaces) + "}");

            return builder.ToString();
        }

        private string GetSpace(int count)
        {
            string result = string.Empty;

            for (var i = 0; i < count; i++)
            {
                result += " ";
            }

            return result;
        }

        private string GetCSharpCode(CSharpStatement statement, int spaces)
        {
            if (statement is CSharpMethod method)
            {
                return GetCSharpCode(method, spaces); // local function
            }

            if (statement is CSharpComment @comment)
            {
                return GetSpace(spaces) + "// " + comment.Text;
            }

            if (statement is CSharpNewStatement @new)
            {
                return GetSpace(spaces) + "var " + @new.VariableName + " = " + @new.NewExpression + ";";
            }

            if (statement is CSharpAssignmentStatement @as)
            {
                return GetSpace(spaces) + @as.Left + " = " + @as.ValueExpression + ";";
            }

            if (statement is CSharpConditionAssignmentStatement cond)
            {
                return GetSpace(spaces) + "if (" + cond.Condition + ") { " + cond.Left + " = " + cond.ValueExpression + "; }";
            }

            if (statement is CSharpReturnStatement @ret)
            {
                return GetSpace(spaces) + "return " + @ret.VariableName + ";";
            }

            if (statement is CSharpCallStatement @call)
            {
                if (!string.IsNullOrEmpty(@call.VariableName))
                {
                    return GetSpace(spaces) + @call.VariableName + "." + @call.CallExpression + ";";
                }
                else
                {
                    return GetSpace(spaces) + @call.CallExpression + ";";
                }
            }

            if (statement is CSharpFieldDeclaration @field)
            {
                return GetSpace(spaces) + $"private {@field.Type} {@field.FieldName}" + $" = new {@field.Type}();";
            }

            return null;
        }
    }
}
