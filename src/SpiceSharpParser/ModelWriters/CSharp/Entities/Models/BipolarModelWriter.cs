using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Models
{
    public class BipolarModelWriter : ModelWriter
    {
        public override List<CSharpStatement> Write(Model @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            var type = GetType(@object);
            var parameters = GetModelParameters(@object);
            var modelId = context.GetIdentifier(@object.Name);

            result.Add(new CSharpNewStatement(modelId, $@"new BipolarJunctionTransistorModel(""{@object.Name}"")"));

            if (type.ToLower() == "npn")
            {
                result.Add(SetParameter(modelId, "npn", true, context));
            }
            else if (type.ToLower() == "pnp")
            {
                result.Add(SetParameter(modelId, "pnp", true, context));
            }

            SetProperties(result, modelId, parameters, context);

            context.RegisterModelType(@object.Name, "BipolarJunctionTransistorModel");

            return result;
        }
    }
}
