using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Models
{
    public class MosfetModelWriter : ModelWriter
    {
        public MosfetModelWriter()
        {
            // Default MOS levels
            Levels.Add(1, "Mosfet1Model");
            Levels.Add(2, "Mosfet2Model");
            Levels.Add(3, "Mosfet3Model");
        }

        protected Dictionary<int, string> Levels { get; } = new Dictionary<int, string>();

        public override List<CSharpStatement> Write(Model @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            var type = GetType(@object);
            var parameters = GetModelParameters(@object);
            var modelId = context.GetIdentifier(@object.Name);

            var clonedParameters = (ParameterCollection)parameters.Clone();

            int level = 1;
            int lindex = -1, vindex = -1;
            for (int i = 0; i < clonedParameters.Count; i++)
            {
                if (clonedParameters[i] is AssignmentParameter ap)
                {
                    if (ap.Name.ToLower() == "level")
                    {
                        lindex = i;
                        level = (int)Math.Round(context.EvaluationContext.Evaluate(ap.Value));
                    }

                    if (ap.Name.ToLower() == "version")
                    {
                        vindex = i;
                    }

                    if (vindex >= 0 && lindex >= 0)
                    {
                        break;
                    }
                }
            }

            if (lindex >= 0)
            {
                clonedParameters.RemoveAt(lindex);
            }

            if (vindex >= 0)
            {
                clonedParameters.RemoveAt(vindex < lindex ? vindex : vindex - 1);
            }

            // Generate the model
            if (Levels.ContainsKey(level))
            {
                var model = Levels[level];
                result.Add(new CSharpNewStatement(modelId, $@"new {model}(""{@object.Name}"")"));

                switch (type.ToLower())
                {
                    case "nmos":
                        result.Add(SetParameter(modelId, "nmos", true, context)); break;
                    case "pmos":
                        result.Add(SetParameter(modelId, "pmos", true, context)); break;
                }

                context.RegisterModelType(@object.Name, model);
                SetProperties(result, modelId, clonedParameters, context);
            }

            return result;
        }
    }
}
