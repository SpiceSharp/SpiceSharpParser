using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    public class RLCGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, ProcessingContextBase context)
        {
            switch (type)
            {
                case "r": return GenerateRes(id.Name, parameters, context);
                case "l": return GenerateInd(id.Name, parameters, context);
                case "c": return GenerateCap(id.Name, parameters, context);
                case "k": return GenerateMut(id.Name, parameters, context);
            }

            return null;
        }

        public Entity GenerateMut(string name, ParameterCollection parameters, ProcessingContextBase context)
        {
            var mut = new MutualInductance(name);

            switch (parameters.Count)
            {
                case 0: throw new Exception($"Inductor name expected for mutual inductance \"{name}\"");
                case 1: throw new Exception("Inductor name expected");
                case 2: throw new Exception("Coupling factor expected");
            }

            if (!(parameters[0] is SingleParameter))
            {
                throw new Exception("Component name expected");
            }

            if (!(parameters[1] is SingleParameter))
            {
                throw new Exception("Component name expected");
            }

            mut.InductorName1 = parameters.GetString(0);
            mut.InductorName2 = parameters.GetString(1);

            context.SetParameter(mut, "k", parameters.GetString(2));

            return mut;
        }

        public Entity GenerateCap(string name, ParameterCollection parameters, ProcessingContextBase context)
        {
            var capacitor = new Capacitor(name);
            context.CreateNodes(capacitor, parameters);
            var clonedParameters = parameters.Clone();

            for (int i = 3; i < clonedParameters.Count; i++)
            {
                if (parameters[i] is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        context.SetParameter(capacitor, "ic", asg.Value);
                        clonedParameters.Remove(i);
                        break;
                    }
                }
            }

            if (parameters.Count == 3)
            {
                context.SetParameter(capacitor, "capacitance", parameters.GetString(2));
            }
            else
            {
                capacitor.SetModel(context.FindModel<CapacitorModel>(parameters.GetString(2)));
                context.SetParameters(capacitor, clonedParameters, 2);

                var bp = capacitor.ParameterSets[typeof(SpiceSharp.Components.CapacitorBehaviors.BaseParameters)] as SpiceSharp.Components.CapacitorBehaviors.BaseParameters;
                if (!bp.Length.Given)
                {
                    throw new Exception("L needs to be specified");
                }
            }

            return capacitor;
        }

        public Entity GenerateInd(string name, ParameterCollection parameters, ProcessingContextBase context)
        {
            if (parameters.Count != 3)
            {
                throw new Exception();
            }

            var inductor = new Inductor(name);
            context.CreateNodes(inductor, parameters);

            context.SetParameter(inductor, "inductance", parameters.GetString(2));

            return inductor;
        }

        public Entity GenerateRes(string name, ParameterCollection parameters, ProcessingContextBase context)
        {
            var res = new Resistor(name);
            context.CreateNodes(res, parameters);

            if (parameters.Count == 3)
            {
                context.SetParameter(res, "resistance", parameters.GetString(2));
            }
            else
            {
                res.SetModel(context.FindModel<ResistorModel>(parameters.GetString(2)));

                foreach (var equal in parameters.Skip(2))
                {
                    if (equal is AssignmentParameter ap)
                    {
                        context.SetParameter(res, ap.Name, ap.Value);
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

        public override List<string> GetGeneratedSpiceTypes()
        {
            return new List<string> { "r", "l", "c", "k" };
        }
    }
}
