using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    /// <summary>
    /// Current sources generator
    /// </summary>
    public class CurrentSourceGenerator : ComponentGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSourceGenerator"/> class.
        /// </summary>
        public CurrentSourceGenerator()
        {
        }

        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes => new List<string>() { "I", "G", "F" };

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "i": return GenerateCurrentSource(componentIdentifier, parameters, context);
                case "g": return GenerateVoltageControlledCurrentSource(componentIdentifier, parameters, context);
                case "f": return GenerateCurrentControlledCurrentSource(componentIdentifier, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Generates a new current controlled current source: FName
        /// </summary>
        /// <param originalName="name">Name of generated current controlled current source</param>
        /// <param originalName="parameters">Parameters for current source</param>
        /// <param originalName="context">Reading context</param>
        /// <returns>
        /// A new instance of current controlled current source
        /// </returns>
        protected SpiceSharp.Components.Component GenerateCurrentControlledCurrentSource(string name,  ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count == 4)
            {
                CurrentControlledCurrentSource cccs = new CurrentControlledCurrentSource(name);
                context.CreateNodes(cccs, parameters);
                cccs.ControllingName = context.ComponentNameGenerator.Generate(parameters.GetString(2));
                context.SetParameter(cccs, "gain", parameters.GetString(3));
                return cccs;
            }
            else
            {
                if (parameters.Count == 3)
                {
                    if (!(parameters[2] is AssignmentParameter assignmentParameter) || assignmentParameter.Name.ToLower() != "value")
                    {
                        throw new WrongParametersCountException(name, "current controlled current source expects that third parameter is assignment parameter");
                    }

                    var cs = new CurrentSource(name);
                    context.CreateNodes(cs, parameters);
                    context.SetParameter(cs, "dc", assignmentParameter.Value);
                    return cs;
                }
                else
                {
                    throw new WrongParametersCountException(name, "current controlled current source expects 3 or 4 parameters");
                }
            }
        }

        /// <summary>
        /// Generates a new voltage controlled current source: GName
        /// </summary>
        /// <param originalName="name">Name of generated voltage controlled current source</param>
        /// <param originalName="parameters">Parameters for current source</param>
        /// <param originalName="context">Reading context</param>
        /// <returns>
        /// A new instance of voltage controlled current source
        /// </returns>
        protected SpiceSharp.Components.Component GenerateVoltageControlledCurrentSource(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count == 5)
            {
                VoltageControlledCurrentSource vccs = new VoltageControlledCurrentSource(name);
                context.CreateNodes(vccs, parameters);
                context.SetParameter(vccs, "gain", parameters.GetString(4));

                return vccs;
            }
            else
            {
                if (parameters.Count == 3)
                {
                    if (!(parameters[2] is AssignmentParameter assignmentParameter) || assignmentParameter.Name.ToLower() != "value")
                    {
                        throw new WrongParametersCountException(name, "voltage controlled current source expects that third parameter is assignment parameter");
                    }

                    var cs = new CurrentSource(name);
                    context.CreateNodes(cs, parameters);
                    context.SetParameter(cs, "dc", assignmentParameter.Value);
                    return cs;
                }
                else
                {
                    throw new WrongParametersCountException(name, "voltage controlled current source expects 3 or 5 parameters");
                }
            }
        }

        /// <summary>
        /// Generates a new current source.
        /// </summary>
        /// <param originalName="name">Name of generated current source.</param>
        /// <param originalName="parameters">Parameters for current source.</param>
        /// <param originalName="context">Reading context.</param>
        /// <returns>
        /// A new instance of current source.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateCurrentSource(string name,  ParameterCollection parameters, IReadingContext context)
        {
            CurrentSource isrc = new CurrentSource(name);
            context.CreateNodes(isrc, parameters);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i] is SingleParameter s && s.Image.ToLower() == "dc" && i != parameters.Count - 1)
                {
                    context.SetParameter(isrc, "dc", parameters.GetString(i + 1));
                    i++;
                }
                else if (i == 2 && parameters[i] is SingleParameter vp && parameters[i].Image.ToLower() != "dc" && parameters[i].Image.ToLower() != "ac")
                {
                    context.SetParameter(isrc, "dc", parameters.GetString(i));
                }
                else if (parameters[i] is SingleParameter s2 && s2.Image.ToLower() == "ac")
                {
                    i++;
                    if (i < parameters.Count)
                    {
                        if (parameters[i] is SingleParameter == false)
                        {
                            throw new WrongParameterTypeException(name, "Current source AC magnitude has wrong type of parameter: " + parameters[i].GetType());
                        }

                        context.SetParameter(isrc, "acmag", parameters.GetString(i));

                        // Look forward for one more value
                        if (i + 1 < parameters.Count)
                        {
                            // support for all single parameters
                            if (parameters[i + 1] is SingleParameter)
                            {
                                i++;
                                context.SetParameter(isrc, "acphase", parameters.GetString(i));
                            }
                            else
                            {
                                if (!(parameters[i + 1] is BracketParameter))
                                {
                                    throw new WrongParameterTypeException(name, "Current source AC phase has wrong type of parameter: " + parameters[i].GetType());
                                }
                            }
                        }
                    }
                }
                else if (parameters[i] is BracketParameter cp)
                {
                    isrc.SetParameter("waveform", context.WaveformReader.Generate(cp, context));
                }
                else if (parameters[i] is AssignmentParameter ap && ap.Name.ToLower() == "value")
                {
                    context.SetParameter(isrc, "dc", ap.Value);
                }
                else
                {
                    if (parameters[i].Image.ToLower() != "dc")
                    {
                        throw new WrongParameterException("Wrong parameter at the position " + (i + 1) + " for current source: " + parameters[i].Image);
                    }
                }
            }

            return isrc;
        }
    }
}
