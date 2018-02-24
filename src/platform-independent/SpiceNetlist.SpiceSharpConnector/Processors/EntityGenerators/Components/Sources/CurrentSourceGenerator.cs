using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    public class CurrentSourceGenerator : EntityGenerator
    {
        private readonly WaveformProcessor waveFormGenerator;

        public CurrentSourceGenerator(WaveformProcessor waveFormGenerator)
        {
            this.waveFormGenerator = waveFormGenerator;
        }

        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, ProcessingContext context)
        {
            switch (type)
            {
                case "i": return GenerateCurrentSource(id.Name, type, parameters, context);
                case "g": return GenerateVoltageControlledCurrentSource(id.Name, type, parameters, context);
                case "f": return GenerateCurrentControlledCurrentSource(id.Name, type, parameters, context);
            }

            return null;
        }

        public Entity GenerateCurrentControlledCurrentSource(string name, string type, ParameterCollection parameters, ProcessingContext context)
        {
            CurrentControlledCurrentSource cccs = new CurrentControlledCurrentSource(name);
            context.CreateNodes(cccs, parameters);

            switch (parameters.Count)
            {
                case 2: throw new Exception("Voltage source expected");
                case 3: throw new Exception("Value expected");
            }

            cccs.ControllingName = new Identifier(parameters.GetString(2));
            cccs.ParameterSets.SetProperty("gain", context.ParseDouble(parameters.GetString(3)));
            return cccs;
        }

        public Entity GenerateVoltageControlledCurrentSource(string name, string type, ParameterCollection parameters, ProcessingContext context)
        {
            if (parameters.Count < 5)
            {
                throw new Exception("Value expected");
            }

            VoltageControlledCurrentSource vccs = new VoltageControlledCurrentSource(name);
            context.CreateNodes(vccs, parameters);
            vccs.ParameterSets.SetProperty("gain", context.ParseDouble(parameters.GetString(4)));

            return vccs;
        }

        public Entity GenerateCurrentSource(string name, string type, ParameterCollection parameters, ProcessingContext context)
        {
            CurrentSource isrc = new CurrentSource(name);
            context.CreateNodes(isrc, parameters);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters.GetString(2).ToLower() == "dc")
                {
                    i++;
                    isrc.ParameterSets.SetProperty("dc", context.ParseDouble(parameters.GetString(i)));
                }
                else if (i == 2 && parameters[i] is ValueParameter v)
                {
                    isrc.ParameterSets.SetProperty("dc", context.ParseDouble(v.Image));
                }

                // AC specification
                else if (parameters.GetString(i).ToLower() == "ac")
                {
                    i++;
                    isrc.ParameterSets.SetProperty("acmag", context.ParseDouble(parameters.GetString(i)));

                    // Look forward for one more value
                    if (i + 1 < parameters.Count && parameters[i + 1] is ValueParameter v2)
                    {
                        i++;
                        isrc.ParameterSets.SetProperty("acphase", context.ParseDouble(v2.Image));
                    }
                }
                else if (parameters[i] is BracketParameter cp)
                {
                    isrc.ParameterSets.SetProperty("waveform", waveFormGenerator.Generate(cp, context));
                }
                else
                {
                    throw new Exception("Unrecognized parameter");
                }
            }

            return isrc;
        }

        public override List<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "i", "g", "f" };
        }
    }
}
