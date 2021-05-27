using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Models
{
    public class JFETModelWriter : ModelWriter
    {
        public override List<CSharpStatement> Write(Model @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            var type = GetType(@object);
            var parameters = GetModelParameters(@object);
            var modelId = context.GetIdentifier(@object.Name);

            result.Add(new CSharpNewStatement(modelId, $@"new JFETModel(""{@object.Name}"")"));
            switch (type.ToLower())
            {
                case "pjf": result.Add(SetParameter(modelId, "pjf", true, context)); break;
                case "njf": result.Add(SetParameter(modelId, "njf", true, context)); break;
            }

            SetProperties(result, modelId, parameters, context);
            context.RegisterModelType(@object.Name, "JFETModel");

            return result;
        }
    }
}
