using System;
using System.Collections.Generic;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors.Generators.Models
{
    abstract class ModelGenerator : Generator
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
