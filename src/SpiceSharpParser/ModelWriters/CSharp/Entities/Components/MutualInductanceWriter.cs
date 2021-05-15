using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class MutualInductanceWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component component, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            var pins = component.PinsAndParameters.Take(2);
            var parameters = component.PinsAndParameters.Skip(2);
            var name = component.Name;

            var inductanceId = context.GetNewIdentifier(name);

            result.Add(new CSharpNewStatement(inductanceId, $@"new MutualInductance(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", {base.Evaluate(parameters[2].Value, context)})"));

            for (var i = 1; i < parameters.Count; i++)
            {
                if (parameters[i] is AssignmentParameter assignmentParameter)
                {
                    result.Add(SetParameter(inductanceId, assignmentParameter.Name,assignmentParameter.Value, context));
                }
            }

            return result;
        }
    }
}
