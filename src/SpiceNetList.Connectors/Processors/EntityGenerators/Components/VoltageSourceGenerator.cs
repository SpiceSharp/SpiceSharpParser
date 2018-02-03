using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    class VoltageSourceGenerator : EntityGenerator
    {
        public override Entity Generate(string name, string type, ParameterCollection parameters, NetList currentNetList)
        {
            var vs = new VoltageSource(name,
                (parameters.Values[0] as SingleParameter).RawValue,
                (parameters.Values[1] as SingleParameter).RawValue,
                10);

            return vs;

        }

        public override List<string> GetGeneratedTypes()
        {
            return new List<string>() { "v" };
        }
    }
}
