using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class ComplexMosfetModelGenerator : MosfetModelGenerator, ICustomModelGenerator
    {
        public ComplexMosfetModelGenerator()
        {
        }

        public Context.Models.Model Process(Context.Models.Model model, IModelsRegistry models)
        {

            if (model.Entity is BSIM3Model bsim3Model)
            {

                if (model.Name.Contains("."))
                {
                    var aggregateName = bsim3Model.Name.Substring(0, bsim3Model.Name.IndexOf('.'));
                    var aggregate = models.FindModel(aggregateName);
                    if (aggregate != null)
                    {
                        var aggrategeEntity = aggregate.Entity as BSIM3AggregateModel;
                        aggrategeEntity.Models.Add(bsim3Model);

                        return aggregate;
                    }
                    else
                    {
                        var aggrategeEntity = new BSIM3AggregateModel(aggregateName);
                        aggrategeEntity.Models.Add(bsim3Model);

                        return new Context.Models.Model(aggregateName, aggrategeEntity, null);
                    }

                }

            }
            return model;
        }
    }
}