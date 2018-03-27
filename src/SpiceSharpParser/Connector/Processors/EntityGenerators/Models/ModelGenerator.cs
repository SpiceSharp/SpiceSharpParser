using System;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharpParser.Connector.Extensions;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceSharpParser.Connector.Processors.EntityGenerators.Models
{
    public abstract class ModelGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IProcessingContext context)
        {
            var model = GenerateModel(id.ToString(), type);
            if (model == null)
            {
                throw new GeneralConnectorException("Couldn't generate model");
            }

            context.SetParameters(model, parameters);
            return model;
        }

        internal abstract Entity GenerateModel(string name, string type);
    }
}
