using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    public class RLCGenerator : EntityGenerator
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

        public Entity GenerateMut(string name, ParameterCollection parameters, NetList currentNetList)
        {
            var mut = new MutualInductance(name);

            switch (parameters.Count)
            {
                case 0: throw new Exception($"Inductor name expected for mutual inductance \"{name}\"");
                case 1: throw new Exception("Inductor name expected");
                case 2: throw new Exception("Coupling factor expected");
            }

            if (!(parameters[0] is SingleParameter)) //TODO
            {
                throw new Exception("Component name expected");
            }
            if (!(parameters[1] is SingleParameter)) //TODO
            {
                throw new Exception("Component name expected");
            }
            mut.InductorName1 = (parameters[0] as SingleParameter).RawValue;
            mut.InductorName2 = (parameters[1] as SingleParameter).RawValue;
            mut.ParameterSets.SetProperty("k", currentNetList.ParseDouble((parameters[2] as SingleParameter).RawValue));

            return mut;
        }

        public Entity GenerateCap(string name, ParameterCollection parameters, NetList currentNetList)
        {
            var capacitor = new Capacitor(name);
            CreateNodes(parameters, capacitor);

            if (parameters.Count == 3)
            {
                var capacitance = (parameters[2] as SingleParameter).RawValue;
                capacitor.ParameterSets.SetProperty("capacitance", currentNetList.ParseDouble(capacitance));

                return capacitor;
            }
            else
            {
                //TODO !!!!!
                throw new System.Exception();
            }
        }

        public Entity GenerateInd(string name, ParameterCollection parameters, NetList currentNetList)
        {
            if (parameters.Count != 3)
            {
                throw new Exception();
            }

            var inductor = new Inductor(name);
            CreateNodes(parameters, inductor);
           
            var inductance = (parameters[2] as SingleParameter).RawValue;
            inductor.ParameterSets.SetProperty("inductance", currentNetList.ParseDouble(inductance));
            return inductor;
        }

        public Entity GenerateRes(string name, ParameterCollection parameters, NetList netlist)
        {
            var res = new Resistor(name);
            CreateNodes(parameters, res);

            if (parameters.Count == 3)
            {
                var value = (parameters[2] as SingleParameter).RawValue;
                res.ParameterSets.SetProperty("resistance", netlist.ParseDouble(value));
            }
            else
            {
                var modelName = (parameters[2] as SingleParameter).RawValue;
                res.SetModel(netlist.FindModel<ResistorModel>(modelName));

                foreach (var equal in parameters.Skip(2))
                {
                    if (equal is AssignmentParameter ap)
                    {
                        res.ParameterSets.SetProperty(ap.Name, netlist.ParseDouble(ap.Value));
                    }
                }

                SpiceSharp.Components.ResistorBehaviors.BaseParameters bp = res.ParameterSets[typeof(SpiceSharp.Components.ResistorBehaviors.BaseParameters)] as SpiceSharp.Components.ResistorBehaviors.BaseParameters;
                if (!bp.Length.Given)
                {
                    throw new System.Exception("L needs to be specified");
                }
            }

            return res;
        }

        public override List<string> GetGeneratedTypes()
        {
            return new List<string> { "r", "l", "c", "k" };
        }
    }
}
