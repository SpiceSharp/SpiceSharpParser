using SpiceSharp.Components;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;

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

        public override void AddGenericLevel<TModel, TParameters>(int level)
        {
            base.AddGenericLevel<TModel, TParameters>(level);
        }

        public override void AddLevel<TModel, TParameters>(int level)
        {
            Levels[level] = (name, type, _) =>
            {
                var mosfet = (TModel)Activator.CreateInstance(typeof(TModel), name);
                switch (type.ToLower())
                {
                    case "nmos": mosfet.SetParameter("nmos", true); break;
                    case "pmos": mosfet.SetParameter("pmos", true); break;
                }

                return new Context.Models.Model(name, mosfet, mosfet.Parameters);
            };
        }


        public override Context.Models.Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context)
        {
            var clonedParameters = (ParameterCollection)parameters.Clone();

            int level = 1;
            string version = null;
            int lindex = -1, vindex = -1;
            for (int i = 0; i < clonedParameters.Count; i++)
            {
                if (clonedParameters[i] is AssignmentParameter ap)
                {
                    if (ap.Name.ToLower() == "level")
                    {
                        lindex = i;
                        level = (int)Math.Round(context.Evaluator.EvaluateDouble(ap.Value));
                    }

                    if (ap.Name.ToLower() == "version")
                    {
                        vindex = i;
                        version = ap.Value.ToLower();
                    }

                    if (vindex >= 0 && lindex >= 0)
                    {
                        break;
                    }
                }
            }

            if (lindex >= 0)
            {
                clonedParameters.RemoveAt(lindex);
            }

            if (vindex >= 0)
            {
                clonedParameters.RemoveAt(vindex < lindex ? vindex : vindex - 1);
            }

            // Generate the model
            Context.Models.Model model;
            if (Levels.ContainsKey(level))
            {
                model = Levels[level].Invoke(id, type, version);
            }
            else
            {
                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"Unknown mosfet model level {level}", parameters.LineInfo);
                return null;
            }

            // Read all the parameters
            SetParameters(context, model.Entity, clonedParameters);

            return model;
        }

    }
}