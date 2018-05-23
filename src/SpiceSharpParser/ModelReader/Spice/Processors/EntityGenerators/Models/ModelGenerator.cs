using System;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Exceptions;
using SpiceSharpParser.ModelReader.Spice.Extensions;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceSharpParser.ModelReader.Spice.Processors.EntityGenerators.Models
{
    public abstract class ModelGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IProcessingContext context)
        {
            var model = GenerateModel(id.ToString(), type);
            if (model == null)
            {
                throw new GeneralReaderException("Couldn't generate model");
            }

            context.SetParameters(model, parameters);
            return model;
        }

        internal abstract Entity GenerateModel(string name, string type);
    }
}
