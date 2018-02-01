using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetList.Connectors.SpiceSharp.Expressions;
using SpiceNetList.Connectors.SpiceSharp.Processors;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System.Collections.Generic;

namespace SpiceNetList.Connectors.SpiceSharp.Generators.Components
{
    public class RLCGenerator : Generator
    {
        public override Entity Generate(string name, string type, ParameterCollection parameters, NetList currentNetList)
        {
            switch (type)
            {
                case "r": return GenerateRes(name, parameters, currentNetList);
                case "l": return GenerateInd(name, parameters, currentNetList);
                case "c": return GenerateCap(name, parameters, currentNetList);
                case "k": return GenerateMut(name, parameters, currentNetList);
            }
            return null;
        }

        private Entity GenerateMut(string name, ParameterCollection parameters, NetList currentNetList)
        {
            return new MutualInductance(name);
        }

        private Entity GenerateCap(string name, ParameterCollection parameters, NetList currentNetList)
        {
            return new Capacitor(name);
        }

        private Entity GenerateInd(string name, ParameterCollection parameters, NetList currentNetList)
        {
            return new Inductor(name);
        }

        private Entity GenerateRes(string name, ParameterCollection parameters, NetList currentNetList)
        {
            if (parameters.Values.Count == 3)
            {
                var res = new Resistor(name);

                Identifier[] nodes = new Identifier[2];
                nodes[0] = (parameters.Values[0] as SingleParameter).RawValue;
                nodes[1] = (parameters.Values[1] as SingleParameter).RawValue;

                var value = (parameters.Values[2] as SingleParameter).RawValue;
                SpiceExpression expression = new SpiceExpression();
                res.Parameters.SetProperty("resistance", expression.Parse(value));
                res.Connect(nodes);

                return res;
            }
            else
            {
                throw new System.Exception();
            }
        }

        public override List<string> GetGeneratedTypes()
        {
            return new List<string> { "r", "l", "c", "k" };
        }
    }
}
