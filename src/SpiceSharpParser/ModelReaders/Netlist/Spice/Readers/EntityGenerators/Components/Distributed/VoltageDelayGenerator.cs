using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Distributed
{
    public class VoltageDelayGenerator : ComponentGenerator
    {
        public override IEntity Generate(string name, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 5)
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "Wrong parameter count for voltage delay", parameters.LineInfo);
                return null;
            }

            var vd = new VoltageDelay(name);
            context.CreateNodes(vd, parameters.Take(VoltageDelay.PinCount));

            parameters = parameters.Skip(VoltageDelay.PinCount);

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
                    else if (paramName == "m")
                    {
                        //ignore
                    }
                    else
                    {
                        context.Result.ValidationResult.AddError(ValidationEntrySource.Reader,  $"Wrong parameter {paramName} for voltage delay", parameters.LineInfo);
                        return null;
                    }
                }
                else
                {
                    context.SetParameter(vd, "delay", parameter.Value);
                }
            }

            return vd;
        }
    }
}