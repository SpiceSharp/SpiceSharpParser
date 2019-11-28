using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
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
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Model model) => mosfet.Model = model.Name };
            });

            // MOS2
            Mosfets.Add(typeof(Mosfet2Model), (string name) =>
            {
                var mosfet = new Mosfet2(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Model model) => mosfet.Model = model.Name };
            });

            // MOS3
            Mosfets.Add(typeof(Mosfet3Model), (string name) =>
            {
                var mosfet = new Mosfet3(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Model model) => mosfet.Model = model.Name };
            });
        }

        /// <summary>
        /// Generate a mosfet instance based on a model.
        /// The generator is passed the arguments name and model.
        /// </summary>
        protected Dictionary<Type, Func<string, MosfetDetails>> Mosfets { get; } = new Dictionary<Type, Func<string, MosfetDetails>>();

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            // Errors
            switch (parameters.Count)
            {
                case 0: throw new WrongParametersCountException($"Node expected for component {componentIdentifier}");
                case 1:
                case 2:
                case 3: throw new WrongParametersCountException("Node expected");
                case 4: throw new WrongParametersCountException("Model name expected");
            }

            // Get the model and generate a component for it
            SpiceSharp.Components.Component mosfet = null;
            string modelName = parameters.Get(4).Image;
            Model model = context.ModelsRegistry.FindModel<Model>(modelName);
            if (model == null)
            {
                throw new ModelNotFoundException($"Could not find model {modelName} for mosfet {originalName}", parameters.LineNumber);
            }

            if (Mosfets.ContainsKey(model.GetType()))
            {
                var mosfetDetails = Mosfets[model.GetType()].Invoke(componentIdentifier);
                mosfet = mosfetDetails.Mosfet;

                context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                {
                    context.ModelsRegistry.SetModel(
                        mosfetDetails.Mosfet,
                        simulation,
                        modelName,
                        $"Could not find model {modelName} for mosfet {componentIdentifier}",
                        mosfetDetails.SetModelAction,
                        context.Result);
                });
            }
            else
            {
                throw new ReadingException("Invalid model");
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