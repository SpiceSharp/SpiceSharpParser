using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    public class SwitchGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, ProcessingContext context)
        {
            switch (type)
            {
                case "s": return GenerateVoltageSwitch(id.Name, parameters, context);
                case "w": return GenerateCurrentSwitch(id.Name, parameters, context);
            }

            return null;
        }

        public Entity GenerateVoltageSwitch(string name, ParameterCollection parameters, ProcessingContext context)
        {
            VoltageSwitch vsw = new VoltageSwitch(name);
            context.CreateNodes(parameters, vsw);

            // Read the model
            if (parameters.Count < 5)
            {
                throw new Exception("Model expected");
            }

            vsw.SetModel(context.FindModel<VoltageSwitchModel>(parameters.GetString(4)));

            // Optional ON or OFF
            if (parameters.Count == 6)
            {
                switch (parameters.GetString(5).ToLower())
                {
                    case "on":
                        vsw.ParameterSets.SetProperty("on", true); // TODO check this
                        break;
                    case "off":
                        vsw.ParameterSets.SetProperty("off", true); // TODO check this
                        break;
                    default:
                        throw new Exception("ON or OFF expected");
                }
            }

            return vsw;
        }

        public Entity GenerateCurrentSwitch(string name, ParameterCollection parameters, ProcessingContext context)
        {
            CurrentSwitch csw = new CurrentSwitch(name);
            switch (parameters.Count)
            {
                case 2: throw new Exception("Voltage source expected");
                case 3: throw new Exception("Model expected");
            }

            context.CreateNodes(parameters, csw);

            // Get the controlling voltage source
            if (parameters[2] is WordParameter || parameters[2] is IdentifierParameter)
            {
                csw.ControllingName = new Identifier(parameters.GetString(2));
            }
            else
            {
                throw new Exception("Voltage source name expected");
            }

            // Get the model
            csw.SetModel(context.FindModel<CurrentSwitchModel>(parameters.GetString(3)));

            // Optional on or off
            if (parameters.Count > 4)
            {
                switch (parameters.GetString(4).ToLower())
                {
                    case "on":
                        csw.ParameterSets.SetProperty("on", true); // TODO check this
                        break;
                    case "off":
                        csw.ParameterSets.SetProperty("off", true); // TODO check this
                        break;
                    default:
                        throw new Exception("ON or OFF expected");
                }
            }

            return csw;
        }

        public override List<string> GetGeneratedTypes()
        {
            return new List<string> { "s", "w" };
        }
    }
}
