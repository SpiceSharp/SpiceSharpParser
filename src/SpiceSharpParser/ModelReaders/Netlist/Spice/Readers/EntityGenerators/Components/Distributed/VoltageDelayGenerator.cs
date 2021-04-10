using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Distributed
{
    public class VoltageDelayGenerator : ComponentGenerator
    {
        public override IEntity Generate(string name, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            if (parameters.Count > 7 || parameters.Count < 5)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "Wrong parameter count for voltage delay", parameters.LineInfo));
                return null;
            }

            var vd = new VoltageDelay(name);
            context.CreateNodes(vd, parameters);

            parameters = parameters.Skip(4);

            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    var paramName = ap.Name.ToLower();

                    if (paramName == "reltol")
                    {
                        context.SetParameter(vd, "reltol", ap.Value);
                    }
                    else if (paramName == "abstol")
                    {
                        context.SetParameter(vd, "abstol", ap.Value);
                    }
                    else
                    {
                        context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Wrong parameter {paramName} for voltage delay", parameters.LineInfo));
                        return null;
                    }
                }
                else
                {
                    context.SetParameter(vd, "delay", parameter.Image);
                }
            }

            return vd;
        }
    }
}