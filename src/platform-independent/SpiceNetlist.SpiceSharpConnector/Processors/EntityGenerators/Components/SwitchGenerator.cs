using System;
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
    public class SwitchGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IProcessingContext context)
        {
            switch (type)
            {
                case "s": return GenerateVoltageSwitch(id.Name, parameters, context);
                case "w": return GenerateCurrentSwitch(id.Name, parameters, context);
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
        /// <param name="context">Processing context</param>
        /// <returns>
        /// A new voltage switch
        /// </returns>
        protected Entity GenerateVoltageSwitch(string name, ParameterCollection parameters, IProcessingContext context)
        {
            VoltageSwitch vsw = new VoltageSwitch(name);
            context.CreateNodes(vsw, parameters);

            // Read the model
            if (parameters.Count < 5)
            {
                throw new WrongParametersCountException("Model expected");
            }

            var model = context.FindModel<VoltageSwitchModel>(parameters.GetString(4));
            if (model != null)
            {
                vsw.SetModel(model);
            }
            else
            {
                throw new GeneralConnectorException("Couln't find model for current switch");
            }

            // Optional ON or OFF
            if (parameters.Count == 6)
            {
                switch (parameters.GetString(5).ToLower())
                {
                    case "on":
                        vsw.SetParameter("on", true); // TODO check this
                        break;
                    case "off":
                        vsw.SetParameter("off", true); // TODO check this
                        break;
                    default:
                        throw new Exception("ON or OFF expected");
                }
            }
            else
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
        /// <param name="context">Processing context</param>
        /// <returns>
        /// A new instance of current switch
        /// </returns>
        protected Entity GenerateCurrentSwitch(string name, ParameterCollection parameters, IProcessingContext context)
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
                csw.ControllingName = new Identifier(parameters.GetString(2));
            }
            else
            {
                throw new WrongParameterTypeException("Voltage source name expected");
            }

            // Get the model
            var model = context.FindModel<CurrentSwitchModel>(parameters.GetString(3));
            if (model != null)
            {
                csw.SetModel(model);
            }
            else
            {
                throw new GeneralConnectorException("Couln't find model for current switch");
            }

            // Optional on or off
            if (parameters.Count > 4)
            {
                switch (parameters.GetString(4).ToLower())
                {
                    case "on":
                        csw.SetParameter("on", true); // TODO check this
                        break;
                    case "off":
                        csw.SetParameter("off", true); // TODO check this
                        break;
                    default:
                        throw new GeneralConnectorException("ON or OFF expected");
                }
            }

            return csw;
        }
    }
}
