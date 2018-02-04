using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    class VoltageSourceGenerator : EntityGenerator
    {
        public override Entity Generate(string name, string type, ParameterCollection parameters, NetList netlist)
        {
            var vsrc = new VoltageSource(name);
            CreateNodes(parameters, vsrc);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Values.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters.Values[i] is SingleParameter s && s.RawValue.ToLower() == "dc")
                {
                    i++;
                    vsrc.ParameterSets.SetProperty("dc", netlist.ParseDouble((parameters.Values[i] as SingleParameter).RawValue));
                }
                else if (i == 2 && parameters.Values[i] is ValueParameter vp)
                {
                    vsrc.ParameterSets.SetProperty("dc", netlist.ParseDouble((parameters.Values[i] as SingleParameter).RawValue));
                }

                // AC specification
                else if (parameters.Values[i] is SingleParameter s2 && s2.RawValue.ToLower() == "ac")
                {
                    i++;
                    vsrc.ParameterSets.SetProperty("acmag", netlist.ParseDouble((parameters.Values[i] as SingleParameter).RawValue));

                    // Look forward for one more value
                    if (i + 1 < parameters.Values.Count && parameters.Values[i + 1] is ValueParameter vp2)
                    {
                        i++;
                        vsrc.ParameterSets.SetProperty("acphase", netlist.ParseDouble(vp2.RawValue));
                    }
                }

                // Waveforms
                else if (parameters.Values[i] is ComplexParameter cp)
                {
                    WaveformsGenerator wG = new WaveformsGenerator();
                    vsrc.ParameterSets.SetProperty("waveform", wG.Generate(cp, netlist));
                }
                else
                    throw new Exception();
            }
            return vsrc;
        }

        public override List<string> GetGeneratedTypes()
        {
            return new List<string>() { "v" };
        }
    }
}
