using System;
using System.Collections.Generic;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public class SwitchGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type)
            {
                case "s": return GenerateVoltageSwitch(id.ToString(), parameters, context);
                case "w": return GenerateCurrentSwitch(id.ToString(), parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Gets generated Spice types by generator
        /// </summary>
        /// <returns>
        /// Generated Spice types
        /// </returns>
        public override IEnumerable<string> GetGeneratedSpiceTypes()
        {
            return new List<string> { "s", "w" };
        }

        /// <summary>
        /// Generates a voltage switch
        /// </summary>
        /// <param name="name">Name of voltage switch to generate</param>
        /// <param name="parameters">Parameters for voltage switch</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new voltage switch
        /// </returns>
        protected Entity GenerateVoltageSwitch(string name, ParameterCollection parameters, IReadingContext context)
        {
            // Read the model
            if (parameters.Count < 5)
            {
                throw new WrongParametersCountException("Wrong parameter count for voltage switch");
            }

            VoltageSwitch vsw = new VoltageSwitch(name);
            context.CreateNodes(vsw, parameters);

            var model = context.FindModel<VoltageSwitchModel>(parameters.GetString(4));
            if (model != null)
            {
                vsw.SetModel((VoltageSwitchModel)context.ProvideModelFor(vsw, model));
            }
            else
            {
                throw new ModelNotFoundException($"Could not find model {parameters.GetString(2)} for voltage switch {name}");
            }

            // Optional ON or OFF
            if (parameters.Count == 6)
            {
                switch (parameters.GetString(5).ToLower())
                {
                    case "on":
                        vsw.ParameterSets.SetParameter("on");
                        break;
                    case "off":
                        vsw.ParameterSets.SetParameter("off");
                        break;
                    default:
                        throw new Exception("ON or OFF expected");
                }
            }
            else if (parameters.Count > 6)
            {
                throw new WrongParametersCountException("Too many parameters for voltage switch");
            }

            return vsw;
        }

        /// <summary>
        /// Generates a current switch
        /// </summary>
        /// <param name="name">Name of current switch</param>
        /// <param name="parameters">Parameters of current switch</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of current switch
        /// </returns>
        protected Entity GenerateCurrentSwitch(string name, ParameterCollection parameters, IReadingContext context)
        {
            CurrentSwitch csw = new CurrentSwitch(name);
            switch (parameters.Count)
            {
                case 2: throw new WrongParametersCountException(name, "Voltage source expected");
                case 3: throw new WrongParametersCountException(name, "Model expected");
                case 4: break;
                case 5: break;
                default:
                    throw new WrongParametersCountException(name, "Wrong parameter count for current switch");
            }

            context.CreateNodes(csw, parameters);

            // Get the controlling voltage source
            if (parameters[2] is WordParameter || parameters[2] is IdentifierParameter)
            {
                csw.ControllingName = new StringIdentifier(parameters.GetString(2));
            }
            else
            {
                throw new WrongParameterTypeException("Voltage source name expected");
            }

            // Get the model
            var model = context.FindModel<CurrentSwitchModel>(parameters.GetString(3));
            if (model != null)
            {
                csw.SetModel((CurrentSwitchModel)context.ProvideModelFor(csw, model));
            }
            else
            {
                throw new ModelNotFoundException($"Could not find model {parameters.GetString(2)} for current switch {name}");
            }

            // Optional on or off
            if (parameters.Count > 4)
            {
                switch (parameters.GetString(4).ToLower())
                {
                    case "on":
                        csw.ParameterSets.SetParameter("on");
                        break;
                    case "off":
                        csw.ParameterSets.SetParameter("off");
                        break;
                    default:
                        throw new GeneralReaderException("ON or OFF expected");
                }
            }

            return csw;
        }
    }
}
