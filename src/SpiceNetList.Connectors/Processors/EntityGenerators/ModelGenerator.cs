using System;
using SpiceNetlist.SpiceObjects;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators
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

            base.SetParameters(model, parameters, currentNetList);
            return model;
        }

        internal abstract Entity GenerateModel(string name, string type);
    }
}
