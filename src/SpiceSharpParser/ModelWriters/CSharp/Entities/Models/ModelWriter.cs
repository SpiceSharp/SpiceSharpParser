using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Models
{
    public abstract class ModelWriter : BaseWriter, IWriter<Model>
    {
        public static string GetType(Model @object)
        {
            if (@object.Parameters.Count == 0)
            {
                return null;
            }

            if (@object.Parameters[0] is BracketParameter bracketParameter)
            {
                return bracketParameter.Name;
            }

            if (@object.Parameters[0] is SingleParameter parameter)
            {
                return parameter.Value;
            }

            return null;
        }

        public abstract List<CSharpStatement> Write(Model @object, IWriterContext context);

        public ParameterCollection GetModelParameters(Model @object)
        {
            if (@object.Parameters.Count == 0)
            {
                return new ParameterCollection();
            }

            if (@object.Parameters[0] is BracketParameter bracketParameter)
            {
                return bracketParameter.Parameters;
            }

            if (@object.Parameters[0] is SingleParameter)
            {
                return @object.Parameters.Skip(1);
            }

            return new ParameterCollection();
        }

        protected void SetProperties(List<CSharpStatement> result, string modelId, ParameterCollection parameters, IWriterContext context)
        {
            foreach (var parameter in parameters)
            {
                if (parameter is AssignmentParameter asg)
                {
                    result.Add(SetParameter(modelId, asg.Name, asg.Value, context));
                }
            }
        }
    }
}
