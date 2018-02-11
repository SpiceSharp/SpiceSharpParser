using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    public class VoltageSourceGenerator : EntityGenerator
    {
        private readonly WaveformsGenerator waveFormGenerator;

        public VoltageSourceGenerator(WaveformsGenerator waveFormGenerator)
        {
            this.waveFormGenerator = waveFormGenerator;
        }

        public override Entity Generate(string name, string type, ParameterCollection parameters, NetList netlist)
        {
            switch (type)
            {
                case "v": return GenerateVoltageSource(name, type, parameters, netlist);
                case "h": return GenerateCurrentControlledVoltageSource(name, type, parameters, netlist);
                case "e": return GenerateVoltageControlledVoltageSource(name, type, parameters, netlist);
            }

            return null;
        }

        private Entity GenerateVoltageControlledVoltageSource(string name, string type, ParameterCollection parameters, NetList netlist)
        {
            var vcvs = new VoltageControlledVoltageSource(name);
            CreateNodes(parameters, vcvs);

            if (parameters.Count < 5)
            {
                throw new Exception("Value expected");
            }

            vcvs.ParameterSets.SetProperty("gain", netlist.ParseDouble((parameters[4] as SingleParameter).RawValue));
            return vcvs;
        }

        private Entity GenerateCurrentControlledVoltageSource(string name, string type, ParameterCollection parameters, NetList netlist)
        {
            var ccvs = new CurrentControlledVoltageSource(name);
            CreateNodes(parameters, ccvs);
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
            ccvs.ParameterSets.SetProperty("gain", netlist.ParseDouble((parameters[3] as SingleParameter).RawValue));

            return ccvs;
        }

        public Entity GenerateVoltageSource(string name, string type, ParameterCollection parameters, NetList netlist)
        {
            var vsrc = new VoltageSource(name);
            CreateNodes(parameters, vsrc);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i] is SingleParameter s && s.RawValue.ToLower() == "dc")
                {
                    i++;
                    vsrc.ParameterSets.SetProperty("dc", netlist.ParseDouble((parameters[i] as SingleParameter).RawValue));
                }
                else if (i == 2 && parameters[i] is ValueParameter vp)
                {
                    vsrc.ParameterSets.SetProperty("dc", netlist.ParseDouble((parameters[i] as SingleParameter).RawValue));
                }

                // AC specification
                else if (parameters[i] is SingleParameter s2 && s2.RawValue.ToLower() == "ac")
                {
                    i++;
                    vsrc.ParameterSets.SetProperty("acmag", netlist.ParseDouble((parameters[i] as SingleParameter).RawValue));

                    // Look forward for one more value
                    if (i + 1 < parameters.Count && parameters[i + 1] is ValueParameter vp2)
                    {
                        i++;
                        vsrc.ParameterSets.SetProperty("acphase", netlist.ParseDouble(vp2.RawValue));
                    }
                }

                // Waveforms
                else if (parameters[i] is ComplexParameter cp)
                {
                    vsrc.ParameterSets.SetProperty("waveform", waveFormGenerator.Generate(cp, netlist));
                }
                else
                    throw new Exception();
            }
            return vsrc;
        }

        public override List<string> GetGeneratedTypes()
        {
            return new List<string>() { "v", "h", "e" };
        }
    }
}
