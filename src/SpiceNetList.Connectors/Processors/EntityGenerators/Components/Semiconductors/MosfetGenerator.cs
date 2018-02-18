using SpiceNetlist.SpiceObjects;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using System;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Semiconductors
{
    public class MosfetGenerator : EntityGenerator
    {
        public MosfetGenerator()
        {
            // MOS1
            Mosfets.Add(typeof(Mosfet1Model), (Identifier name, Entity model) =>
            {
                var m = new Mosfet1(name);
                m.SetModel((Mosfet1Model)model);
                return m;
            });

            // MOS2
            Mosfets.Add(typeof(Mosfet2Model), (Identifier name, Entity model) =>
            {
                var m = new Mosfet2(name);
                m.SetModel((Mosfet2Model)model);
                return m;
            });

            // MOS3
            Mosfets.Add(typeof(Mosfet3Model), (Identifier name, Entity model) =>
            {
                var m = new Mosfet3(name);
                m.SetModel((Mosfet3Model)model);
                return m;
            });
        }

        public override Entity Generate(Identifier entityName, string originalName, string type, ParameterCollection parameters, ProcessingContext context)
        {
            // Errors
            switch (parameters.Count)
            {
                case 0: throw new Exception($"Node expected for component {originalName}");
                case 1:
                case 2:
                case 3: throw new Exception("Node expected");
                case 4: throw new Exception("Model name expected");
            }

            // Get the model and generate a component for it
            Entity model = context.FindModel<Entity>(parameters.GetString(4));
            if (model == null)
            {
                throw new Exception("Can't find mosfet model");
            }

            SpiceSharp.Components.Component mosfet = null;
            if (Mosfets.ContainsKey(model.GetType()))
            {
                mosfet = Mosfets[model.GetType()].Invoke(entityName, model);
            }
            else
            {
                throw new Exception("Invalid model");
            }

            // The rest is all just parameters
            context.CreateNodes(mosfet, parameters);
            context.SetEntityParameters(mosfet, parameters, 5);
            return mosfet;
        }

        /// <summary>
        /// Generate a mosfet instance based on a model.
        /// The generator is passed the arguments name and model.
        /// </summary>
        public Dictionary<Type, Func<Identifier, Entity, SpiceSharp.Components.Component>> Mosfets { get; } = new Dictionary<Type, Func<Identifier, Entity, SpiceSharp.Components.Component>>();

        public override List<string> GetGeneratedTypes()
        {
            return new List<string>() { "m" };
        }
    }
}
