using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    /// <summary>
    /// Voltage sources generator
    /// </summary>
    public class VoltageSourceGenerator : ComponentGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSourceGenerator"/> class.
        /// </summary>
        public VoltageSourceGenerator()
        {
        }

        public override SpiceSharp.Components.Component Generate(Identifier componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type)
            {
                case "v": return GenerateVoltageSource(componentIdentifier.ToString(), parameters, context);
                case "h": return GenerateCurrentControlledVoltageSource(componentIdentifier.ToString(), parameters, context);
                case "e": return GenerateVoltageControlledVoltageSource(componentIdentifier.ToString(), parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes
        {
            get
            {
                return new List<string>() { "v", "h", "e" };
            }
        }

        /// <summary>
        /// Generates new voltage controlled voltage source: EName
        /// </summary>
        /// <param name="name">The name of voltage source to generate</param>
        /// <param name="parameters">The paramters for voltage source</param>
        /// <param name="context">The reading context</param>
        /// <returns>
        /// A new instance of voltage controlled voltage source
        /// </returns>
        protected SpiceSharp.Components.Component GenerateVoltageControlledVoltageSource(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count == 5)
            {
                var vcvs = new VoltageControlledVoltageSource(name);
                context.CreateNodes(vcvs, parameters);
                context.SetParameter(vcvs, "gain", parameters.GetString(4), true);
                return vcvs;
            }
            else
            {
                if (parameters.Count == 3)
                {
                    if (!(parameters[2] is AssignmentParameter assigmentParameter) || assigmentParameter.Name.ToLower() != "value")
                    {
                        throw new WrongParametersCountException(name, "voltage controlled voltage source expects that third parameter is assigment parameter");
                    }

                    var vcvs = new VoltageSource(name);
                    context.CreateNodes(vcvs, parameters);
                    context.SetParameter(vcvs, "dc", assigmentParameter.Value, true);

                    return vcvs;
                }
                else
                {
                     throw new WrongParametersCountException(name, "voltage controlled voltage source expects 3 or 5 parameters");
                }
            }
        }

        /// <summary>
        /// Generates new current controlled voltage source HName
        /// </summary>
        /// <param name="name">The name of voltage source to generate</param>
        /// <param name="parameters">The paramters for voltage source</param>
        /// <param name="context">The reading context</param>
        /// <returns>
        /// A new instance of current controlled voltage source
        /// </returns>
        protected SpiceSharp.Components.Component GenerateCurrentControlledVoltageSource(string name,  ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count == 3)
            {
                if (!(parameters[2] is AssignmentParameter assigmentParameter) || assigmentParameter.Name.ToLower() != "value")
                {
                    throw new WrongParametersCountException(name, "voltage controlled voltage source expects that third parameter is assigment parameter");
                }

                var vcvs = new VoltageSource(name);
                context.CreateNodes(vcvs, parameters);
                context.SetParameter(vcvs, "dc", assigmentParameter.Value, true);

                return vcvs;
            }
            else
            {
                switch (parameters.Count)
                {
                    case 2: throw new WrongParametersCountException(name, "Voltage source expected");
                    case 3: throw new WrongParametersCountException(name, "Gain expected");
                    case 4: break;
                    default:
                        throw new WrongParametersCountException(name, "Current controlled voltage source expects 4 parameters");
                }

                if (!(parameters[3] is SingleParameter))
                {
                    throw new WrongParameterTypeException(name, "Name of controlling voltage source expected");
                }

                var ccvs = new CurrentControlledVoltageSource(name);
                context.CreateNodes(ccvs, parameters);

                ccvs.ControllingName = parameters.GetString(2);
                context.SetParameter(ccvs, "gain", parameters.GetString(3), true);
                return ccvs;
            }
        }

        /// <summary>
        /// Generates new voltage source
        /// </summary>
        /// <param name="name">The name of voltage source to generate</param>
        /// <param name="parameters">The paramters for voltage source</param>
        /// <param name="context">The reading context</param>
        /// <returns>
        /// A new instance of voltage source
        /// </returns>
        protected SpiceSharp.Components.Component GenerateVoltageSource(string name, ParameterCollection parameters, IReadingContext context)
        {
            var vsrc = new VoltageSource(name);
            context.CreateNodes(vsrc, parameters);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i] is SingleParameter s && s.Image.ToLower() == "dc" && i != parameters.Count - 1)
                {
                    context.SetParameter(vsrc, "dc", parameters.GetString(i + 1), true);
                    i++;
                }
                else if (i == 2 && parameters[i] is SingleParameter vp && parameters[i].Image.ToLower() != "dc" && parameters[i].Image.ToLower() != "ac")
                {
                    context.SetParameter(vsrc, "dc", parameters.GetString(i), true);
                }
                else if (parameters[i] is SingleParameter s2 && s2.Image.ToLower() == "ac")
                {
                    i++;
                    if (i < parameters.Count)
                    {
                        if (parameters[i] is SingleParameter == false)
                        {
                            throw new WrongParameterTypeException(name, "Voltage source AC magnitude has wrong type of parameter: " + parameters[i].GetType());
                        }

                        context.SetParameter(vsrc, "acmag", parameters.GetString(i), true);

                        // Look forward for one more value
                        if (i + 1 < parameters.Count)
                        {
                            // support for all single parameters
                            if (parameters[i + 1] is SingleParameter)
                            {
                                i++;
                                context.SetParameter(vsrc, "acphase", parameters.GetString(i), true);
                            }
                            else
                            {
                                if (!(parameters[i + 1] is BracketParameter))
                                {
                                    throw new WrongParameterTypeException(name, "Voltage source AC phase has wrong type of parameter: " + parameters[i].GetType());
                                }
                            }
                        }
                    }
                }
                else if (parameters[i] is BracketParameter cp)
                {
                    vsrc.SetParameter("waveform", context.WaveformReader.Generate(cp, context));
                }
                else if (parameters[i] is AssignmentParameter ap)
                {
                    if (ap.Name.ToLower() == "value")
                    {
                        context.SetParameter(vsrc, "dc", ap.Value, true);
                    }
                }
                else
                {
                    if (parameters[i].Image.ToLower() != "dc")
                    {
                        throw new WrongParameterException("Wrong parameter at the position " + (i + 1) + " for voltage source: " + parameters[i].Image);
                    }
                }
            }

            return vsrc;
        }
    }
}
