using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Models
{
    public class SwitchModelWriter : ModelWriter
    {
        public override List<CSharpStatement> Write(Model @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            var type = GetType(@object);
            var parameters = GetModelParameters(@object);
            var modelId = context.GetIdentifier(@object.Name);

            switch (type.ToLower())
            {
                case "sw":
                    context.RegisterModelType(@object.Name, "VoltageSwitchModel");
                    result.Add(new CSharpNewStatement(modelId, $@"new VoltageSwitchModel(""{@object.Name}"")"));
                    break;
                case "csw":
                    context.RegisterModelType(@object.Name, "CurrentSwitchModel");
                    result.Add(new CSharpNewStatement(modelId, $@"new CurrentSwitchModel(""{@object.Name}"")"));
                    break;
            }

            SetProperties(result, modelId, parameters, context);

            return result;
        }
    }
}
