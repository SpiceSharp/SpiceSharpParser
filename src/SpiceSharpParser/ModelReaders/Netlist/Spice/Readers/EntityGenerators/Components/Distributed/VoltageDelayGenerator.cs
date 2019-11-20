using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Distributed
{
    public class VoltageDelayGenerator : ComponentGenerator
    {
        public override SpiceSharp.Components.Component Generate(string name, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            if (parameters.Count > 7 || parameters.Count < 5)
            {
                throw new WrongParametersCountException("Wrong parameter count for voltage delay");
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
                        throw new UnknownParameterException(paramName);
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
