using System;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Extensions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public abstract class ModelGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            var model = GenerateModel(id.ToString(), type);
            if (model == null)
            {
                throw new GeneralReaderException("Couldn't generate model");
            }

            context.StochasticModelsRegistry.RegisterModel(model);

            ParameterCollection filteredParameters = FilterAndRegisterDevAndLot(parameters, context, model, (string name) =>
            {
                var newModel = GenerateModel(name, type);
                context.SetParameters(newModel, FilerDevAndLot(parameters), true);
                return newModel;
            });
            context.SetParameters(model, filteredParameters, true);
            return model;
        }

        protected abstract Entity GenerateModel(string name, string type);

        private static ParameterCollection FilterAndRegisterDevAndLot(ParameterCollection parameters, IReadingContext context, Entity model, Func<string, Entity> generator)
        {
            var filteredParameters = new ParameterCollection();

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Image.ToUpper() == "DEV")
                {
                    context.StochasticModelsRegistry.RegisterModelDev(model, generator, parameters[i - 1], (Parameter)parameters[i + 1]);
                    i++;
                }
                else if (parameters[i].Image.ToUpper() == "LOT")
                {
                    context.StochasticModelsRegistry.RegisterModelLot(model, generator, parameters[i - 1], (Parameter)parameters[i + 1]);
                    i++;
                }
                else
                {
                    filteredParameters.Add(parameters[i]);
                }
            }

            return filteredParameters;
        }

        private static ParameterCollection FilerDevAndLot(ParameterCollection parameters)
        {
            var filteredParameters = new ParameterCollection();

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Image.ToUpper() == "DEV")
                {
                    i++;
                }
                else if (parameters[i].Image.ToUpper() == "LOT")
                {
                    i++;
                }
                else
                {
                    filteredParameters.Add(parameters[i]);
                }
            }

            return filteredParameters;
        }
    }
}
