using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    /// <summary>
    /// Generator for resistors, capacitors, inductors and mutual inductance
    /// </summary>
    public class RLCGenerator : ComponentGenerator
    {
        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes => new List<string> { "R", "L", "C", "K" };

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "r": return GenerateRes(componentIdentifier, parameters, context);
                case "l": return GenerateInd(componentIdentifier, parameters, context);
                case "c": return GenerateCap(componentIdentifier, parameters, context);
                case "k": return GenerateMut(componentIdentifier, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Generates a new mutual inductance.
        /// </summary>
        /// <param name="name">The name of generated mutual inductance.</param>
        /// <param name="parameters">Parameters and pins for mutual inductance.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of mutual inductance.
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

            context.SetParameter(mut, "k", parameters.GetString(2));

            return mut;
        }

        /// <summary>
        ///  Generates a new capacitor.
        /// </summary>
        /// <param name="name">Name of capacitor to generate.</param>
        /// <param name="parameters">Parameters and pins for capacitor.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of capacitor.
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
                    context.ModelsRegistry.SetModel<CapacitorModel>(
                        capacitor,
                        parameters.GetString(2),
                        $"Could not find model {parameters.GetString(2)} for capacitor {name}",
                        (CapacitorModel model) => capacitor.SetModel(model));

                    modelBased = true;
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
        /// Generates a new inductor.
        /// </summary>
        /// <param name="name">Name of inductor to generate.</param>
        /// <param name="parameters">Parameters and pins for inductor.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of inductor.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateInd(string name, ParameterCollection parameters, IReadingContext context)
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
        /// Generate resistor.
        /// </summary>
        /// <param name="name">Name of resistor to generate.</param>
        /// <param name="parameters">Parameters and pins for resistor.</param>
        /// <param name="context">Reading context.</param>
        /// <exception cref="GeneralReaderException">When there is wrong syntax.</exception>
        /// <returns>
        /// A new instance of resistor.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateRes(string name, ParameterCollection parameters, IReadingContext context)
        {
            Resistor res = new Resistor(name);

            var dynamicParameter = parameters.FirstOrDefault(p => p.Image == "dynamic");
            if (dynamicParameter != null)
            {
                parameters.Remove(parameters.ToList().IndexOf(dynamicParameter));
            }

            bool isDynamic = dynamicParameter != null || context.Result?.SimulationConfiguration?.DynamicResistors == true;

            if (isDynamic)
            {
                context.SimulationPreparations.ExecuteTemperatuteBehaviorBeforeLoad(res);
            }

            context.CreateNodes(res, parameters);

            if (parameters.Count == 3) 
            {
                // RName Node1 Node2 something
                var something = parameters[2];

                // Check if something is a model name
                if ((something is WordParameter || something is IdentifierParameter)  
                    && context.ModelsRegistry.FindModel<ResistorModel>(parameters.GetString(2)) != null)
                {
                    // RName Node1 Node2 modelName 
                    throw new GeneralReaderException("L parameter needs to be specified");
                }

                // Check if something can be resistance
                if ((something is WordParameter || something is IdentifierParameter || something is ValueParameter 
                    || something is ExpressionParameter) == false)
                {
                    throw new GeneralReaderException("Third parameter needs to represent resistance of resistor");
                }

                // Set resistance
                context.SetParameter(res, "resistance", something.Image, isDynamic);
            }
            else
            {
                var resistorParameters = new List<Parameter>(parameters.Skip(Resistor.ResistorPinCount).ToArray());

                // RName Node1 Node2 something param1 ...
                var something = resistorParameters[0];

                // Check if something is a model name
                bool hasModelSyntax = (something is WordParameter || something is IdentifierParameter)
                                      && context.ModelsRegistry.FindModel<ResistorModel>(something.Image) != null;
                bool hasTcParameter = parameters.Any(
                    p => p is AssignmentParameter ap && ap.Name.Equals(
                             "tc",
                             context.CaseSensitivity.IsEntityParameterNameCaseSensitive
                                 ? StringComparison.CurrentCulture
                                 : StringComparison.CurrentCultureIgnoreCase));

                AssignmentParameter tcParameter = null;

                if (hasTcParameter)
                {
                    tcParameter = (AssignmentParameter)parameters.Single(
                        p => p is AssignmentParameter ap && ap.Name.Equals(
                                 "tc",
                                 context.CaseSensitivity.IsEntityParameterNameCaseSensitive
                                     ? StringComparison.CurrentCulture
                                     : StringComparison.CurrentCultureIgnoreCase));
                    resistorParameters.Remove(tcParameter);
                }

                if (hasModelSyntax)
                {
                    var modelName = resistorParameters[0].Image;

                    // Ignore tc parameter on resistor ...
                    context.ModelsRegistry.SetModel<ResistorModel>(
                        res,
                        modelName,
                        $"Could not find model {modelName} for resistor {name}",
                        (ResistorModel model) => res.SetModel(model));

                    resistorParameters.RemoveAt(0);

                    if (resistorParameters.Count > 0 && (resistorParameters[0] is WordParameter
                                                         || resistorParameters[0] is IdentifierParameter
                                                         || resistorParameters[0] is ValueParameter
                                                         || resistorParameters[0] is ExpressionParameter))
                    {
                        context.SetParameter(res, "resistance", resistorParameters[0].Image, false);
                        resistorParameters.RemoveAt(0);
                    }
                }
                else
                {
                    if (hasTcParameter)
                    {
                        var model = new ResistorModel(res.Name + "_default_model");
                        if (tcParameter.Values.Count == 2)
                        {
                            context.SetParameter(model, "tc1", tcParameter.Values[0]);
                            context.SetParameter(model, "tc2", tcParameter.Values[1]);
                        }
                        else
                        {
                            context.SetParameter(model, "tc1", tcParameter.Value);
                        }

                        context.ModelsRegistry.RegisterModelInstance(model);
                        res.SetModel(model);
                    }

                    // Check if something can be resistance
                    var resistanceParameter = resistorParameters[0];

                    if ((resistanceParameter is WordParameter 
                         || resistanceParameter is IdentifierParameter 
                         || resistanceParameter is ValueParameter
                         || resistanceParameter is ExpressionParameter) == false)
                    {
                        throw new GeneralReaderException("Invalid value for resistance");
                    }

                    context.SetParameter(res, "resistance", resistanceParameter.Image, isDynamic);
                    resistorParameters.RemoveAt(0);
                }

                foreach (var parameter in resistorParameters)
                {
                    if (parameter is AssignmentParameter ap)
                    {
                        context.SetParameter(res, ap.Name, ap.Value);
                    }
                    else
                    {
                        throw new GeneralReaderException("Invalid parameter for resistor: " + parameter.Image);
                    }
                }
            }
            return res;
        }
    }
}
