using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    public class VoltageSourceGenerator : EntityGenerator
    {
        private readonly WaveformsGenerator waveFormGenerator;

        public VoltageSourceGenerator(WaveformsGenerator waveFormGenerator)
        {
            this.waveFormGenerator = waveFormGenerator;
        }

        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, ProcessingContext context)
        {
            switch (type)
            {
                case "v": return GenerateVoltageSource(id.Name, type, parameters, context);
                case "h": return GenerateCurrentControlledVoltageSource(id.Name, type, parameters, context);
                case "e": return GenerateVoltageControlledVoltageSource(id.Name, type, parameters, context);
            }

            return null;
        }

        public Entity GenerateVoltageControlledVoltageSource(string name, string type, ParameterCollection parameters, ProcessingContext context)
        {
            var vcvs = new VoltageControlledVoltageSource(name);
            context.CreateNodes(parameters, vcvs);

            if (parameters.Count < 5)
            {
                throw new Exception("Value expected");
            }

            vcvs.ParameterSets.SetProperty("gain", context.ParseDouble((parameters[4] as SingleParameter).RawValue));
            return vcvs;
        }

        public Entity GenerateCurrentControlledVoltageSource(string name, string type, ParameterCollection parameters, ProcessingContext context)
        {
            var ccvs = new CurrentControlledVoltageSource(name);
            context.CreateNodes(parameters, ccvs);
            switch (parameters.Count)
            {
                case 2: throw new Exception("Voltage source expected");
                case 3: throw new Exception("Value expected");
            }

            if (!(parameters[2] is WordParameter))
            {
                throw new Exception("Component name expected");
            }

            ccvs.ControllingName = (parameters[3] as SingleParameter).RawValue;
            ccvs.ParameterSets.SetProperty("gain", context.ParseDouble((parameters[3] as SingleParameter).RawValue));

            return ccvs;
        }

        public Entity GenerateVoltageSource(string name, string type, ParameterCollection parameters, ProcessingContext context)
        {
            var vsrc = new VoltageSource(name);
            context.CreateNodes(parameters, vsrc);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i] is SingleParameter s && s.RawValue.ToLower() == "dc")
                {
                    i++;
                    vsrc.ParameterSets.SetProperty("dc", context.ParseDouble((parameters[i] as SingleParameter).RawValue));
                }
                else if (i == 2 && parameters[i] is ValueParameter vp)
                {
                    vsrc.ParameterSets.SetProperty("dc", context.ParseDouble((parameters[i] as SingleParameter).RawValue));
                }

                // AC specification
                else if (parameters[i] is SingleParameter s2 && s2.RawValue.ToLower() == "ac")
                {
                    i++;
                    vsrc.ParameterSets.SetProperty("acmag", context.ParseDouble((parameters[i] as SingleParameter).RawValue));

                    // Look forward for one more value
                    if (i + 1 < parameters.Count && parameters[i + 1] is ValueParameter vp2)
                    {
                        i++;
                        vsrc.ParameterSets.SetProperty("acphase", context.ParseDouble(vp2.RawValue));
                    }
                }

                // Waveforms
                else if (parameters[i] is ComplexParameter cp)
                {
                    vsrc.ParameterSets.SetProperty("waveform", waveFormGenerator.Generate(cp, context));
                }
                else
                {
                    throw new Exception();
                }
            }

            return vsrc;
        }

        public override List<string> GetGeneratedTypes()
        {
            return new List<string>() { "v", "h", "e" };
        }
    }
}
