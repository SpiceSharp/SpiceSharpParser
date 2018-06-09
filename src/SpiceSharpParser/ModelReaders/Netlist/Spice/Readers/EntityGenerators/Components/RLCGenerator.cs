using System;
using System.Collections.Generic;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Extensions;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    /// <summary>
    /// Generator for resistors, capacitors, inductors and mutual inductance
    /// </summary>
    public class RLCGenerator : EntityGenerator
    {
        /// <summary>
        /// Generates a new resistor, capacitor, inductor or mutual inductance
        /// </summary>
        /// <param name="id">The identifier for identity</param>
        /// <param name="originalName">Original name of entity</param>
        /// <param name="type">The type of entity</param>
        /// <param name="parameters">Parameters for entity</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of entity
        /// </returns>
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type)
            {
                case "r": return GenerateRes(id.ToString(), parameters, context);
                case "l": return GenerateInd(id.ToString(), parameters, context);
                case "c": return GenerateCap(id.ToString(), parameters, context);
                case "k": return GenerateMut(id.ToString(), parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Gets generated Spice types by generator
        /// </summary>
        /// <returns>
        /// Generated Spice types
        /// </returns>
        public override IEnumerable<string> GetGeneratedSpiceTypes()
        {
            return new List<string> { "r", "l", "c", "k" };
        }

        /// <summary>
        /// Generates a new mutual inductance
        /// </summary>
        /// <param name="name">The name of generated mutual inductance</param>
        /// <param name="parameters">Parameters and pins for mutual inductance</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of mutual inductance
        /// </returns>
        protected Entity GenerateMut(string name, ParameterCollection parameters, IReadingContext context)
        {
            var mut = new MutualInductance(name);

            switch (parameters.Count)
            {
                case 0: throw new WrongParametersCountException(name, $"Inductor name expected for mutual inductance \"{name}\"");
                case 1: throw new WrongParametersCountException(name, "Inductor name expected");
                case 2: throw new WrongParametersCountException(name, "Coupling factor expected");
            }

            if (!(parameters[0] is SingleParameter))
            {
                throw new WrongParameterTypeException(name, "Component name expected");
            }

            if (!(parameters[1] is SingleParameter))
            {
                throw new WrongParameterTypeException(name, "Component name expected");
            }

            mut.InductorName1 = parameters.GetString(0);
            mut.InductorName2 = parameters.GetString(1);

            context.SetParameter(mut, "k", parameters.GetString(2));

            return mut;
        }

        /// <summary>
        ///  Generates a new capacitor
        /// </summary>
        /// <param name="name">Name of capacitor to generate</param>
        /// <param name="parameters">Parameters and pins for capacitor</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of capacitor
        /// </returns>
        protected Entity GenerateCap(string name, ParameterCollection parameters, IReadingContext context)
        {
            var capacitor = new Capacitor(name);
            context.CreateNodes(capacitor, parameters);

            if (parameters.Count == 3)
            {
                // CXXXXXXX N1 N2 VALUE
                if (parameters[2] is ExpressionParameter || parameters[2] is ValueParameter)
                {
                    context.SetParameter(capacitor, "capacitance", parameters.GetString(2));
                }
                else
                {
                    throw new WrongParameterTypeException(name, "Wrong parameter value for capacitance");
                }
            }
            else
            {
                // CXXXXXXX N1 N2 <VALUE> <MNAME> <L=LENGTH> <W=WIDTH> <IC=VAL>

                // Examples:
                // CMOD 3 7 CMODEL L = 10u W = 1u
                // CMOD 3 7 CMODEL L = 10u W = 1u IC=1
                // CMOD 3 7 1.3 IC=1

                bool modelBased = false;
                if (parameters[2] is ExpressionParameter || parameters[2] is ValueParameter)
                {
                    context.SetParameter(capacitor, "capacitance", parameters.GetString(2));
                }
                else
                {
                    var model = context.FindModel<CapacitorModel>(parameters.GetString(2));
                    if (model != null)
                    {
                        modelBased = true;
                        capacitor.SetModel(model);
                    }
                    else
                    {
                        throw new ModelNotFoundException($"Could not find model {parameters.GetString(2)} for capacitor {name}");
                    }
                }

                context.SetParameters(capacitor, parameters.Skip(3));

                if (modelBased)
                {
                    var bp = capacitor.ParameterSets[typeof(SpiceSharp.Components.CapacitorBehaviors.BaseParameters)] as SpiceSharp.Components.CapacitorBehaviors.BaseParameters;
                    if (!bp.Length.Given)
                    {
                        throw new GeneralReaderException("L needs to be specified");
                    }
                }
            }

            return capacitor;
        }

        /// <summary>
        /// Generates a new inductor
        /// </summary>
        /// <param name="name">Name of inductor to generate</param>
        /// <param name="parameters">Parameters and pins for inductor</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of inductor
        /// </returns>
        protected Entity GenerateInd(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count != 3)
            {
                throw new WrongParametersCountException("Inductor expects 3 parameters/pins");
            }

            var inductor = new Inductor(name);
            context.CreateNodes(inductor, parameters);

            context.SetParameter(inductor, "inductance", parameters.GetString(2));

            return inductor;
        }

        /// <summary>
        /// Generate resistor
        /// </summary>
        /// <param name="name">Name of resistor to generate</param>
        /// <param name="parameters">Parameters and pins for resistor</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of resistor
        /// </returns>
        protected Entity GenerateRes(string name, ParameterCollection parameters, IReadingContext context)
        {
            var res = new Resistor(name);
            context.CreateNodes(res, parameters);

            if (parameters.Count == 3)
            {
                context.SetParameter(res, "resistance", parameters.GetString(2));
            }
            else
            {
                if (parameters[2] is SingleParameter == false)
                {
                    throw new WrongParameterTypeException(name, "Semiconductor resistor requires a valid model name");
                }

                var model = context.FindModel<ResistorModel>(parameters.GetString(2));
                if (model == null)
                {
                    throw new ModelNotFoundException($"Could not find model {parameters.GetString(2)} for resistor {name}");
                }

                res.SetModel(model);

                foreach (var equal in parameters.Skip(3))
                {
                    if (equal is AssignmentParameter ap)
                    {
                        context.SetParameter(res, ap.Name, ap.Value);
                    }
                    else
                    {
                        throw new WrongParameterException("Only assigment parameters for semiconductor resistor are valid");
                    }
                }

                var lengthParameter = res.ParameterSets.GetParameter("l") as GivenParameter;
                if (lengthParameter == null || !lengthParameter.Given)
                {
                    throw new GeneralReaderException("l needs to be specified");
                }
            }

            return res;
        }
    }
}
