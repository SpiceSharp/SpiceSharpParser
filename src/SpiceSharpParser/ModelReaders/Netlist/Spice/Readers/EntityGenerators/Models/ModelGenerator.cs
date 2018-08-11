using System;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Extensions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharp;
using SpiceSharp.Circuits;

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

            context.SetParameters(model, parameters);
            return model;
        }

        protected abstract Entity GenerateModel(string name, string type);
    }
}
