using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class InductorWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component component, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            var pins = component.PinsAndParameters.Take(Inductor.InductorPinCount);
            var parameters = component.PinsAndParameters.Skip(Inductor.InductorPinCount);
            var name = component.Name;

            var inductorId = context.GetNewIdentifier(name);

            result.Add(new CSharpNewStatement(inductorId, $@"new Inductor(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", {base.Evaluate(parameters[0].Value, context)})"));

            for (var i = 1; i < parameters.Count; i++)
            {
                if (parameters[i] is AssignmentParameter assignmentParameter)
                {
                    result.Add(SetParameter(inductorId, assignmentParameter.Name, assignmentParameter.Value, context));
                }
            }

            return result;
        }
    }
}
