using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    /// <summary>
    /// Generator for resistors, capacitors, inductors and mutual inductance
    /// </summary>
    public class RLCGenerator : ComponentGenerator
    {
        public override SpiceSharp.Components.Component Generate(Identifier componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type)
            {
                case "r": return GenerateRes(componentIdentifier.ToString(), parameters, context);
                case "l": return GenerateInd(componentIdentifier.ToString(), parameters, context);
                case "c": return GenerateCap(componentIdentifier.ToString(), parameters, context);
                case "k": return GenerateMut(componentIdentifier.ToString(), parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes
        {
            get
            {
                return new List<string> { "r", "l", "c", "k" };
            }
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
        protected SpiceSharp.Components.Component GenerateMut(string name, ParameterCollection parameters, IReadingContext context)
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

            context.SetParameter(mut, "k", parameters.GetString(2), true);

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
        protected SpiceSharp.Components.Component GenerateCap(string name, ParameterCollection parameters, IReadingContext context)
        {
            var capacitor = new Capacitor(name);
            context.CreateNodes(capacitor, parameters);

            if (parameters.Count == 3)
            {
                // CXXXXXXX N1 N2 VALUE
                if (parameters[2] is ExpressionParameter || parameters[2] is ValueParameter)
                {
                    context.SetParameter(capacitor, "capacitance", parameters.GetString(2), true);
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
                    context.SetParameter(capacitor, "capacitance", parameters.GetString(2), true);
                }
                else
                {
                    context.ModelsRegistry.SetModel<CapacitorModel>(
                        capacitor,
                        parameters.GetString(2),
                        $"Could not find model {parameters.GetString(2)} for capacitor {name}",
                        (CapacitorModel model) => capacitor.SetModel(model));
                }

                SetParameters(context, capacitor, parameters.Skip(3), true);

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
        protected SpiceSharp.Components.Component GenerateInd(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count != 3)
            {
                throw new WrongParametersCountException("Inductor expects 3 parameters/pins");
            }

            var inductor = new Inductor(name);
            context.CreateNodes(inductor, parameters);

            context.SetParameter(inductor, "inductance", parameters.GetString(2), true);

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
        protected SpiceSharp.Components.Component GenerateRes(string name, ParameterCollection parameters, IReadingContext context)
        {
            var res = new Resistor(name);
            context.CreateNodes(res, parameters);

            if (parameters.Count == 3)
            {
                context.SetParameter(res, "resistance", parameters.GetString(2), true);
            }
            else
            {
                if (parameters[2] is SingleParameter == false)
                {
                    throw new WrongParameterTypeException(name, "Semiconductor resistor requires a valid model name");
                }

                context.ModelsRegistry.SetModel<ResistorModel>(
                    res,
                    parameters.GetString(2),
                    $"Could not find model {parameters.GetString(2)} for resistor {name}",
                    (ResistorModel model) => res.SetModel(model));

                foreach (var equal in parameters.Skip(3))
                {
                    if (equal is AssignmentParameter ap)
                    {
                        context.SetParameter(res, ap.Name, ap.Value, true);
                    }
                    else
                    {
                        throw new WrongParameterException("Only assigment parameters for semiconductor resistor are valid");
                    }
                }

                var lengthParameter = res.ParameterSets.GetParameter<double>("l") as GivenParameter<double>;
                if (lengthParameter == null || !lengthParameter.Given)
                {
                    throw new GeneralReaderException("l needs to be specified");
                }
            }

            return res;
        }
    }
}
