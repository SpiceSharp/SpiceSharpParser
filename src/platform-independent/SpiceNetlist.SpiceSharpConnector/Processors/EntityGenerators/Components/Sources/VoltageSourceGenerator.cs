using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Exceptions;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    /// <summary>
    /// Voltage sources generator
    /// </summary>
    public class VoltageSourceGenerator : EntityGenerator
    {
        /// <summary>
        /// Waveform processor
        /// </summary>
        private readonly IWaveformProcessor waveFormGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSourceGenerator"/> class.
        /// </summary>
        /// <param name="waveFormGenerator">Waveform processor</param>
        public VoltageSourceGenerator(IWaveformProcessor waveFormGenerator)
        {
            this.waveFormGenerator = waveFormGenerator ?? throw new System.ArgumentNullException(nameof(waveFormGenerator));
        }

        /// <summary>
        /// Generates new voltage source
        /// </summary>
        /// <param name="id">The identifier of new voltage source</param>
        /// <param name="originalName">The name of voltage source</param>
        /// <param name="type">A type of voltage source</param>
        /// <param name="parameters">Parameters for voltage source</param>
        /// <param name="context">Processing context</param>
        /// <returns>
        /// A new instance of voltage source
        /// </returns>
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IProcessingContext context)
        {
            switch (type)
            {
                case "v": return GenerateVoltageSource(id.Name, parameters, context);
                case "h": return GenerateCurrentControlledVoltageSource(id.Name, parameters, context);
                case "e": return GenerateVoltageControlledVoltageSource(id.Name, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Gets generated Spice types by generator
        /// </summary>
        /// <returns>
        /// Generated Spice Types
        /// </returns>
        public override IEnumerable<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "v", "h", "e" };
        }

        /// <summary>
        /// Generates new voltage controlled voltage source
        /// </summary>
        /// <param name="name">The name of voltage source to generate</param>
        /// <param name="parameters">The paramters for voltage source</param>
        /// <param name="context">The processing context</param>
        /// <returns>
        /// A new instance of voltage controlled voltage source
        /// </returns>
        protected Entity GenerateVoltageControlledVoltageSource(string name, ParameterCollection parameters, IProcessingContext context)
        {
            if (parameters.Count != 5)
            {
                throw new WrongParametersCountException(name, "Voltage controlled voltage source expects 5 parameters");
            }

            var vcvs = new VoltageControlledVoltageSource(name);
            context.CreateNodes(vcvs, parameters);
            context.SetParameter(vcvs, "gain", parameters.GetString(4));

            return vcvs;
        }

        /// <summary>
        /// Generates new current controlled voltage source
        /// </summary>
        /// <param name="name">The name of voltage source to generate</param>
        /// <param name="parameters">The paramters for voltage source</param>
        /// <param name="context">The processing context</param>
        /// <returns>
        /// A new instance of current controlled voltage source
        /// </returns>
        protected Entity GenerateCurrentControlledVoltageSource(string name,  ParameterCollection parameters, IProcessingContext context)
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
                throw new WrongParameterTypeException(name,  "Name of controlling voltage source expected");
            }

            var ccvs = new CurrentControlledVoltageSource(name);
            context.CreateNodes(ccvs, parameters);

            ccvs.ControllingName = parameters.GetString(2);
            context.SetParameter(ccvs, "gain", parameters.GetString(3));
            return ccvs;
        }

        /// <summary>
        /// Generates new voltage source
        /// </summary>
        /// <param name="name">The name of voltage source to generate</param>
        /// <param name="parameters">The paramters for voltage source</param>
        /// <param name="context">The processing context</param>
        /// <returns>
        /// A new instance of voltage source
        /// </returns>
        protected Entity GenerateVoltageSource(string name, ParameterCollection parameters, IProcessingContext context)
        {
            var vsrc = new VoltageSource(name);
            context.CreateNodes(vsrc, parameters);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i] is SingleParameter s && s.Image.ToLower() == "dc" && i != parameters.Count - 1)
                {
                    context.SetParameter(vsrc, "dc", parameters.GetString(i + 1));
                    i++;
                }
                else if (i == 2 && parameters[i] is SingleParameter vp && parameters[i].Image.ToLower() != "dc" && parameters[i].Image.ToLower() != "ac")
                {
                    context.SetParameter(vsrc, "dc", parameters.GetString(i));
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

                        context.SetParameter(vsrc, "acmag", parameters.GetString(i));

                        // Look forward for one more value
                        if (i + 1 < parameters.Count)
                        {
                            // support for all single parameters
                            if (parameters[i + 1] is SingleParameter)
                            {
                                i++;
                                context.SetParameter(vsrc, "acphase", parameters.GetString(i));
                            }
                            else
                            {
                                throw new WrongParameterTypeException(name, "Voltage source AC phase has wrong type of parameter: " + parameters[i].GetType());
                            }
                        }
                    }
                }
                else if (parameters[i] is BracketParameter cp)
                {
                    vsrc.SetParameter("waveform", waveFormGenerator.Generate(cp, context));
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
