using System;
using SpiceNetlist.SpiceObjects;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators
{
    public abstract class ModelGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IProcessingContext context)
        {
            var model = GenerateModel(id.Name, type);
            if (model == null)
            {
                throw new Exception();
            }

            context.SetEntityParameters(model, parameters);
            return model;
        }

        internal abstract Entity GenerateModel(string name, string type);
    }
}
