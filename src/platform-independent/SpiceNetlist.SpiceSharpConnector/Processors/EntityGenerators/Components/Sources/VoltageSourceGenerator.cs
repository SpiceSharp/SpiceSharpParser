using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    public class VoltageSourceGenerator : EntityGenerator
    {
        private readonly WaveformProcessor waveFormGenerator;

        public VoltageSourceGenerator(WaveformProcessor waveFormGenerator)
        {
            this.waveFormGenerator = waveFormGenerator;
        }

        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IProcessingContext context)
        {
            switch (type)
            {
                case "v": return GenerateVoltageSource(id.Name, type, parameters, context);
                case "h": return GenerateCurrentControlledVoltageSource(id.Name, type, parameters, context);
                case "e": return GenerateVoltageControlledVoltageSource(id.Name, type, parameters, context);
            }

            return null;
        }

        public Entity GenerateVoltageControlledVoltageSource(string name, string type, ParameterCollection parameters, IProcessingContext context)
        {
            if (parameters.Count < 5)
            {
                throw new Exception("Value expected");
            }

            var vcvs = new VoltageControlledVoltageSource(name);
            context.CreateNodes(vcvs, parameters);
            context.SetParameter(vcvs, "gain", parameters.GetString(4));

            return vcvs;
        }

        public Entity GenerateCurrentControlledVoltageSource(string name, string type, ParameterCollection parameters, IProcessingContext context)
        {
            switch (parameters.Count)
            {
                case 2: throw new Exception("Voltage source expected");
                case 3: throw new Exception("Value expected");
            }

            if (!(parameters[2] is WordParameter))
            {
                throw new Exception("Component name expected");
            }

            var ccvs = new CurrentControlledVoltageSource(name);
            context.CreateNodes(ccvs, parameters);

            ccvs.ControllingName = parameters.GetString(3);
            context.SetParameter(ccvs, "gain", parameters.GetString(4));
            return ccvs;
        }

        public Entity GenerateVoltageSource(string name, string type, ParameterCollection parameters, IProcessingContext context)
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
                }
                else if (i == 2 && parameters[i] is SingleParameter vp && parameters[i].Image != "dc" && parameters[i].Image != "ac")
                {
                    context.SetParameter(vsrc, "dc", parameters.GetString(i));
                }
                // AC specification
                else if (parameters[i] is SingleParameter s2 && s2.Image.ToLower() == "ac")
                {
                    i++;
                    if (i < parameters.Count)
                    {
                        context.SetParameter(vsrc, "acmag", parameters.GetString(i));

                        // Look forward for one more value
                        if (i + 1 < parameters.Count && (parameters[i + 1] is ValueParameter || parameters[i + 1] is WordParameter))
                        {
                            i++;
                            context.SetParameter(vsrc, "acphase", parameters.GetString(i));
                        }
                    }
                }
                // Waveforms
                else if (parameters[i] is BracketParameter cp)
                {
                    vsrc.SetParameter("waveform", waveFormGenerator.Generate(cp, context));
                }
                else if (parameters[i] is WordParameter w2 && w2.Image != "dc")
                {
                    throw new Exception();
                }
            }

            return vsrc;
        }

        public override List<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "v", "h", "e" };
        }
    }
}
