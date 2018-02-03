﻿using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models;
using SpiceSharp.Circuits;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    class ModelProcessor : StatementProcessor
    {
        protected List<EntityGenerator> Generators = new List<EntityGenerator>();

        public override void Init()
        {
            Generators.Add(new RLCModelGenerator());
            Generators.Add(new DiodeModelGenerator());
            Generators.Add(new BipolarModelGenerator());
            Generators.Add(new SwitchModelGenerator());
        }

        public override void Process(Statement statement, NetList netlist)
        {
            Model model = statement as Model;
            string name = model.Name.ToLower();
            if (model.Parameters.Values.Count > 0)
            {
                if (model.Parameters.Values[0] is ComplexParameter complex)
                {
                    var type = complex.Name;

                    foreach (var generator in Generators)
                    {
                        if (generator.GetGeneratedTypes().Contains(type))
                        {
                            Entity spiceSharpModel = generator.Generate(name, type, complex.Parameters, netlist);

                            if (model != null)
                            {
                                netlist.Circuit.Objects.Add(spiceSharpModel);
                            }
                        }
                    }
                }
                if (model.Parameters.Values[0] is SingleParameter single)
                {
                    var type = single.RawValue;


                    foreach (var generator in Generators)
                    {
                        if (generator.GetGeneratedTypes().Contains(type))
                        {
                            model.Parameters.Values.RemoveAt(0);

                            Entity spiceSharpModel = generator.Generate(name, type, model.Parameters, netlist);

                            if (model != null)
                            {
                                netlist.Circuit.Objects.Add(spiceSharpModel);
                            }
                        }
                    }
                }


            }
        }
    }
}
