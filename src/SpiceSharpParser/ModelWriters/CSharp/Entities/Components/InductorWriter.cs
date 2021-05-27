using SpiceSharp.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class InductorWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < Inductor.InductorPinCount + 1)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(Inductor.InductorPinCount);
            var parameters = @object.PinsAndParameters.Skip(Inductor.InductorPinCount);
            var name = @object.Name;

            var inductorId = context.GetNewIdentifier(name);

            result.Add(new CSharpNewStatement(inductorId, $@"new Inductor(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", {Evaluate(parameters[0].Value, context)})"));

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
