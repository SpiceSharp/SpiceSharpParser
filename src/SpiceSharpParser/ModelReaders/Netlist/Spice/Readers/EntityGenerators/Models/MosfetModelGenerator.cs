using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Components.Mosfets;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class MosfetModelGenerator : ModelGenerator
    {
        public MosfetModelGenerator()
        {
            // Default MOS levels
            Levels.Add(1, (name, type, _) =>
            {
                var m = new Mosfet1Model(name);
                switch (type.ToLower())
                {
                    case "nmos": m.SetParameter("nmos", true); break;
                    case "pmos": m.SetParameter("pmos", true); break;
                }

                return new Context.Models.Model(name, m, m.Parameters);
            });

            Levels.Add(2, (name, type, _) =>
            {
                var m = new Mosfet2Model(name);
                switch (type.ToLower())
                {
                    case "nmos": m.SetParameter("nmos", true); break;
                    case "pmos": m.SetParameter("pmos", true); break;
                }

                return new Context.Models.Model(name, m, m.Parameters);
            });

            Levels.Add(3, (name, type, _) =>
            {
                var m = new Mosfet3Model(name);
                switch (type.ToLower())
                {
                    case "nmos": m.SetParameter("nmos", true); break;
                    case "pmos": m.SetParameter("pmos", true); break;
                }

                return new Context.Models.Model(name, m, m.Parameters);
            });
        }

        /// <summary>
        /// Gets available model generators indexed by their LEVEL.
        /// The parameters passed are name, type (nmos or pmos) and the version.
        /// </summary>
        protected Dictionary<int, Func<string, string, string, Context.Models.Model>> Levels { get; } = new Dictionary<int, Func<string, string, string, Context.Models.Model>>();

        public void AddLevel<TModel, TParameters>(int level)
            where TModel : Entity<TParameters>
            where TParameters : ModelParameters, ICloneable<TParameters>, new()
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

        public void AddGenericLevel<TModel, TParameters>(int level)
            where TModel : Entity<TParameters>
            where TParameters : ParameterSet<TParameters>, new()
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