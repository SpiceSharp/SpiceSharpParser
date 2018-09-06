using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Extensions;
using System;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Models
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

            ParameterCollection filteredParameters = FilerAndRegisterDevAndLot(parameters, context, model, (string name) =>
            {
                var newModel = GenerateModel(name, type);
                context.SetParameters(newModel, FilerDevAndLot(parameters));
                return newModel;
            });
            context.SetParameters(model, filteredParameters);
            return model;
        }

        private static ParameterCollection FilerAndRegisterDevAndLot(ParameterCollection parameters, IReadingContext context, Entity model, Func<string, Entity> generator)
        {
            var filteredParameters = new ParameterCollection();

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Image.ToUpper() == "DEV")
                {
                    context.RegisterModelDev(model, generator, parameters[i - 1], (Parameter)parameters[i + 1]);
                    i++;
                }
                else if (parameters[i].Image.ToUpper() == "LOT")
                {
                    context.RegisterModelLot(model, generator, parameters[i - 1], (Parameter)parameters[i + 1]);
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


        protected abstract Entity GenerateModel(string name, string type);
    }
}
