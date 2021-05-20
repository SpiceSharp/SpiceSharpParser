using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Models
{
    public class DiodeModelWriter : ModelWriter
    {
        public override List<CSharpStatement> Write(Model @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            var parameters = GetModelParameters(@object);
            var modelId = context.GetIdentifier(@object.Name);

            result.Add(new CSharpNewStatement(modelId, $@"new DiodeModel(""{@object.Name}"")"));

            SetProperties(result, modelId, parameters, context);
            context.RegisterModelType(@object.Name, "DiodeModel");

            return result;
        }
    }
}
