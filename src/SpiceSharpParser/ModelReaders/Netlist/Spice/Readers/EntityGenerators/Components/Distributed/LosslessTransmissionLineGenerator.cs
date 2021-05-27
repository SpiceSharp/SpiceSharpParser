using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Distributed
{
    public class LosslessTransmissionLineGenerator : ComponentGenerator
    {
        public override IEntity Generate(string name, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            var losslessLine = new LosslessTransmissionLine(name);
            context.CreateNodes(losslessLine, parameters.Take(LosslessTransmissionLine.PinCount));
            parameters = parameters.Skip(LosslessTransmissionLine.PinCount);

            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    var paramName = ap.Name.ToLower();

                    if (paramName == "z0" || paramName == "zo")
                    {
                        context.SetParameter(losslessLine, "z0", ap.Value);
                    }
                    else if (paramName == "f")
                    {
                        context.SetParameter(losslessLine, "f", ap.Value);
                    }
                    else if (paramName == "td")
                    {
                        context.SetParameter(losslessLine, "td", ap.Value);
                    }
                    else if (paramName == "reltol")
                    {
                        context.SetParameter(losslessLine, "reltol", ap.Value);
                    }
                    else if (paramName == "abstol")
                    {
                        context.SetParameter(losslessLine, "abstol", ap.Value);
                    }
                    else if (paramName == "m")
                    {
                        context.SetParameter(losslessLine, "m", ap.Value);
                    }
                    else
                    {
                        context.Result.ValidationResult.AddError(ValidationEntrySource.Reader,  $"Invalid parameter: {parameter}", parameter.LineInfo);
                    }
                }
            }

            return losslessLine;
        }
    }
}