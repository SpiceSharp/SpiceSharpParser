using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
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
    public class CurrentSourceGenerator : EntityGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSourceGenerator"/> class.
        /// </summary>
        public CurrentSourceGenerator()
        {
        }

        /// <summary>
        /// Generates a new current source
        /// </summary>
        /// <param name="id">The identifier of new current source</param>
        /// <param name="originalName">The name of current source</param>
        /// <param name="type">A type of current source</param>
        /// <param name="parameters">Parameters for current source</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of current source
        /// </returns>
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type)
            {
                case "i": return GenerateCurrentSource(id.ToString(), parameters, context);
                case "g": return GenerateVoltageControlledCurrentSource(id.ToString(), parameters, context);
                case "f": return GenerateCurrentControlledCurrentSource(id.ToString(), parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Gets the generated types
        /// </summary>
        /// <returns>
        /// A list of generated types
        /// </returns>
        public override IEnumerable<string> GetGeneratedTypes()
        {
            return new List<string>() { "i", "g", "f" };
        }

        /// <summary>
        /// Generates a new current controlled current source
        /// </summary>
        /// <param name="name">Name of generated current controlled current source</param>
        /// <param name="parameters">Parameters for current source</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of current controlled current source
        /// </returns>
        protected Entity GenerateCurrentControlledCurrentSource(string name,  ParameterCollection parameters, IReadingContext context)
        {
            CurrentControlledCurrentSource cccs = new CurrentControlledCurrentSource(name);
            context.CreateNodes(cccs, parameters);

            switch (parameters.Count)
            {
                case 2: throw new Exception("Voltage source expected");
                case 3: throw new Exception("Value expected");
            }

            cccs.ControllingName = new StringIdentifier(parameters.GetString(2));
            context.SetParameter(cccs, "gain", parameters.GetString(3));
            return cccs;
        }

        /// <summary>
        /// Generates a new voltage controlled current source
        /// </summary>
        /// <param name="name">Name of generated voltage controlled current source</param>
        /// <param name="parameters">Parameters for current source</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of voltage controlled current source
        /// </returns>
        protected Entity GenerateVoltageControlledCurrentSource(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 5)
            {
                throw new Exception("Value expected");
            }

            VoltageControlledCurrentSource vccs = new VoltageControlledCurrentSource(name);
            context.CreateNodes(vccs, parameters);
            context.SetParameter(vccs, "gain", parameters.GetString(4));

            return vccs;
        }

        /// <summary>
        /// Generates a new current source
        /// </summary>
        /// <param name="name">Name of generated current source</param>
        /// <param name="parameters">Parameters for current source</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of current source
        /// </returns>
        protected Entity GenerateCurrentSource(string name,  ParameterCollection parameters, IReadingContext context)
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
                    context.SetParameter(isrc, "waveform", context.ReadersRegistry.WaveformReader.Generate(cp, context));
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
