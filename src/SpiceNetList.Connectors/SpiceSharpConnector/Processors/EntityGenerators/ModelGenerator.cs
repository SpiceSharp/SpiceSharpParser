using System;
using SpiceNetlist.SpiceObjects;
using SpiceSharp.Circuits;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors.EntityGenerators.Models
{
    abstract class ModelGenerator : EntityGenerator
    {
        public override Entity Generate(string name, string type, ParameterCollection parameters, NetList currentNetList)
        {
            var model = GenerateModel(name, type);
            if (model == null)
            {
                throw new Exception();
            }

            base.SetParameters(model, parameters);
            return model;
        }

        internal abstract Entity GenerateModel(string name, string type);
    }
}
