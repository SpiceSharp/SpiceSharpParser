using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using Model = SpiceSharp.Components.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class MosfetGenerator : ComponentGenerator
    {
        public MosfetGenerator()
        {
            // MOS1
            Mosfets.Add(typeof(Mosfet1Model), (string name) =>
            {
                var mosfet = new Mosfet1(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Model model) => mosfet.SetModel((Mosfet1Model)model) };
            });

            // MOS2
            Mosfets.Add(typeof(Mosfet2Model), (string name) =>
            {
                var mosfet = new Mosfet2(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Model model) => mosfet.SetModel((Mosfet2Model)model) };
            });

            // MOS3
            Mosfets.Add(typeof(Mosfet3Model), (string name) =>
            {
                var mosfet = new Mosfet3(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Model model) => mosfet.SetModel((Mosfet3Model)model) };
            });
        }

        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes => new List<string>() { "M" };

        /// <summary>
        /// Generate a mosfet instance based on a model.
        /// The generator is passed the arguments name and model.
        /// </summary>
        protected Dictionary<Type, Func<string, MosfetDetails>> Mosfets { get; } = new Dictionary<Type, Func<string, MosfetDetails>>();

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            // Errors
            switch (parameters.Count)
            {
                case 0: throw new Exception($"Node expected for component {componentIdentifier}");
                case 1:
                case 2:
                case 3: throw new Exception("Node expected");
                case 4: throw new Exception("Model name expected");
            }

            // Get the model and generate a component for it
            SpiceSharp.Components.Component mosfet = null;
            string modelName = parameters.GetString(4);
            SpiceSharp.Components.Model model = context.ModelsRegistry.FindModel<SpiceSharp.Components.Model>(modelName);
            if (model == null)
            {
                throw new ModelNotFoundException($"Could not find model {modelName} for mosfet {originalName}");
            }

            if (Mosfets.ContainsKey(model.GetType()))
            {
                var mosfetDetails = Mosfets[model.GetType()].Invoke(componentIdentifier);
                mosfet = mosfetDetails.Mosfet;

                context.ModelsRegistry.SetModel<SpiceSharp.Components.Model>(
                    mosfetDetails.Mosfet,
                    modelName,
                    $"Could not find model {modelName} for mosfet {componentIdentifier}",
                    mosfetDetails.SetModelAction);
            }
            else
            {
                throw new Exception("Invalid model");
            }

            // The rest is all just parameters
            context.CreateNodes(mosfet, parameters);
            SetParameters(context, mosfet, parameters.Skip(5), true);
            return mosfet;
        }

        protected class MosfetDetails
        {
            public SpiceSharp.Components.Component Mosfet { get; set; }

            public Action<Model> SetModelAction { get; set; }
        }
    }
}
