using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class ArbitraryBehavioralWriter : BaseWriter, IWriter<Component>
    {
        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < 3)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(2);
            var parameters = @object.PinsAndParameters.Skip(2);
            var name = @object.Name;

            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "v"))
            {
                var valueParameter = (AssignmentParameter)parameters.Single(p => p is AssignmentParameter ap && ap.Name.ToLower() == "v");
                string expression = valueParameter.Value;

                SourceWriterHelper.CreateBehavioralVoltageSource(result, name, pins, expression, context);
            }

            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "i"))
            {
                var valueParameter = (AssignmentParameter)parameters.Single(p => p is AssignmentParameter ap && ap.Name.ToLower() == "i");
                string expression = valueParameter.Value;

                SourceWriterHelper.CreateBehavioralCurrentSource(result, name, pins, expression, context);
            }

            return result;
        }
    }
}
