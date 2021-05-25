using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp.Controls
{
    public class ParamWriter : IWriter<Control>
    {
        public List<CSharpStatement> Write(Control @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();
            foreach (Parameter param in @object.Parameters)
            {
                if (param is AssignmentParameter assignmentParameter)
                {
                    if (!assignmentParameter.HasFunctionSyntax)
                    {
                        string parameterName = assignmentParameter.Name;
                        string parameterExpression = assignmentParameter.Value;

                        FuncWriter.CreateFunction(context, result, parameterName, parameterExpression, new List<string>());
                    }
                    else
                    {
                        FuncWriter.CreateFunction(context, result, assignmentParameter.Name, assignmentParameter.Value, assignmentParameter.Arguments);
                    }
                }
            }

            return result;
        }
    }
}
