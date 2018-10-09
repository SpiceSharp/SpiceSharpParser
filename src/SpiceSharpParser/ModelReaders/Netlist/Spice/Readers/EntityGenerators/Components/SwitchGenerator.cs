using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public class SwitchGenerator : ComponentGenerator
    {
        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes => new List<string> { "s", "w" };

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "s": return GenerateVoltageSwitch(componentIdentifier, parameters, context);
                case "w": return GenerateCurrentSwitch(componentIdentifier, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Generates a voltage switch.
        /// </summary>
        /// <param name="name">Name of voltage switch to generate.</param>
        /// <param name="parameters">Parameters for voltage switch.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new voltage switch.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateVoltageSwitch(string name, ParameterCollection parameters, IReadingContext context)
        {
            // Read the model
            if (parameters.Count < 5)
            {
                throw new WrongParametersCountException("Wrong parameter count for voltage switch");
            }

            VoltageSwitch vsw = new VoltageSwitch(name);
            context.CreateNodes(vsw, parameters);

            context.ModelsRegistry.SetModel<VoltageSwitchModel>(
              vsw,
              parameters.GetString(4),
              $"Could not find model {parameters.GetString(4)} for voltage switch {name}",
              (VoltageSwitchModel model) => vsw.SetModel(model));

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
        /// Generates a current switch.
        /// </summary>
        /// <param name="name">Name of current switch.</param>
        /// <param name="parameters">Parameters of current switch.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of current switch.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateCurrentSwitch(string name, ParameterCollection parameters, IReadingContext context)
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
                csw.ControllingName = context.ComponentNameGenerator.Generate(parameters.GetString(2));
            }
            else
            {
                throw new WrongParameterTypeException("Voltage source name expected");
            }

            // Get the model
            context.ModelsRegistry.SetModel<CurrentSwitchModel>(
               csw,
               parameters.GetString(3),
               $"Could not find model {parameters.GetString(3)} for current switch {name}",
               (CurrentSwitchModel model) => csw.SetModel(model));

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
