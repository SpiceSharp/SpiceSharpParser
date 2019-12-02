using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public class RLCKGenerator : ComponentGenerator
    {
        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
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
        protected SpiceSharp.Components.Component GenerateMut(string name, ParameterCollection parameters, ICircuitContext context)
        {
            var mut = new MutualInductance(name);

            switch (parameters.Count)
            {
                case 0: throw new WrongParametersCountException($"Inductor name expected for mutual inductance \"{name}\"", parameters.LineNumber);
                case 1: throw new WrongParametersCountException("Inductor name expected", parameters.LineNumber);
                case 2: throw new WrongParametersCountException("Coupling factor expected", parameters.LineNumber);
            }

            if (!(parameters[0] is SingleParameter))
            {
                throw new WrongParameterTypeException("Component name expected", parameters.LineNumber);
            }

            if (!(parameters[1] is SingleParameter))
            {
                throw new WrongParameterTypeException("Component name expected", parameters.LineNumber);
            }

            mut.InductorName1 = parameters.Get(0).Image;
            mut.InductorName2 = parameters.Get(1).Image;

            context.SetParameter(mut, "k", parameters.Get(2));

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
        protected SpiceSharp.Components.Component GenerateCap(string name, ParameterCollection parameters, ICircuitContext context)
        {
            var capacitor = new Capacitor(name);
            context.CreateNodes(capacitor, parameters);

            // Get TC Parameter
            Parameter tcParameter = parameters.FirstOrDefault(
                p => p is AssignmentParameter ap && ap.Name.Equals(
                         "tc",
                         context.CaseSensitivity.IsEntityParameterNameCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));

            if (tcParameter != null)
            {
                parameters.Remove(tcParameter);
            }

            bool modelBased = false;

            if (parameters.Count == 3)
            {
                // CXXXXXXX N1 N2 VALUE
                if (parameters[2] is ExpressionParameter || parameters[2] is ValueParameter)
                {
                    context.SetParameter(capacitor, "capacitance", parameters.Get(2));
                }
                else
                {
                    throw new WrongParameterTypeException("Wrong parameter value for capacitance", parameters.LineNumber);
                }
            }
            else
            {
                // CXXXXXXX N1 N2 <VALUE> <MNAME> <L=LENGTH> <W=WIDTH> <IC=VAL>

                // Examples:
                // CMOD 3 7 CMODEL L = 10u W = 1u
                // CMOD 3 7 CMODEL L = 10u W = 1u IC=1
                // CMOD 3 7 1.3 IC=1
                if (parameters[2] is ExpressionParameter || parameters[2] is ValueParameter)
                {
                    context.SetParameter(capacitor, "capacitance", parameters.Get(2));
                }
                else
                {
                    context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                    {
                        context.ModelsRegistry.SetModel(
                            capacitor,
                            simulation,
                            parameters.Get(2),
                            $"Could not find model {parameters.Get(2)} for capacitor {name}",
                            (CapacitorModel model) => capacitor.Model = model.Name,
                            context.Result);
                    });

                    modelBased = true;
                }

                SetParameters(context, capacitor, parameters.Skip(3), true);

                if (modelBased)
                {
                    var bp = capacitor.ParameterSets[typeof(SpiceSharp.Components.CapacitorBehaviors.BaseParameters)] as SpiceSharp.Components.CapacitorBehaviors.BaseParameters;
                    if (bp == null || !bp.Length.Given)
                    {
                        throw new ReadingException("L needs to be specified");
                    }
                }
            }

            if (tcParameter != null)
            {
                var tcParameterAssignment = tcParameter as AssignmentParameter;

                if (tcParameterAssignment == null)
                {
                    throw new ReadingException("TC needs to be assignment parameter", tcParameter.LineNumber);
                }

                if (modelBased)
                {
                    var model = context.ModelsRegistry.FindModel<CapacitorModel>(parameters.Get(2).Image);

                    if (tcParameterAssignment.Values.Count == 2)
                    {
                        context.SetParameter(model, "tc1", tcParameterAssignment.Values[0]);
                        context.SetParameter(model, "tc2", tcParameterAssignment.Values[1]);
                    }
                    else
                    {
                        context.SetParameter(model, "tc1", tcParameterAssignment.Value);
                    }

                    context.Result.AddEntity(model);
                    capacitor.Model = model.Name;
                }
                else
                {
                    var model = new CapacitorModel(capacitor.Name + "_default_model");
                    if (tcParameterAssignment.Values.Count == 2)
                    {
                        context.SetParameter(model, "tc1", tcParameterAssignment.Values[0]);
                        context.SetParameter(model, "tc2", tcParameterAssignment.Values[1]);
                    }
                    else
                    {
                        context.SetParameter(model, "tc1", tcParameterAssignment.Value);
                    }

                    context.ModelsRegistry.RegisterModelInstance(model);
                    context.Result.AddEntity(model);
                    capacitor.Model = model.Name;
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
        protected SpiceSharp.Components.Component GenerateInd(string name, ParameterCollection parameters, ICircuitContext context)
        {
            if (parameters.Count != 3)
            {
                throw new WrongParametersCountException("Inductor expects 3 parameters/pins", parameters.LineNumber);
            }

            var inductor = new Inductor(name);
            context.CreateNodes(inductor, parameters);
            context.SetParameter(inductor, "inductance", parameters.Get(2));

            return inductor;
        }

        /// <summary>
        /// Generate resistor.
        /// </summary>
        /// <param name="name">Name of resistor to generate.</param>
        /// <param name="parameters">Parameters and pins for resistor.</param>
        /// <param name="context">Reading context.</param>
        /// <exception cref="ReadingException">When there is wrong syntax.</exception>
        /// <returns>
        /// A new instance of resistor.
        /// </returns>
        protected SpiceSharp.Components.Component GenerateRes(string name, ParameterCollection parameters, ICircuitContext context)
        {
            Resistor res = new Resistor(name);

            var dynamicParameter = parameters.FirstOrDefault(p => p.Image == "dynamic");
            if (dynamicParameter != null)
            {
                parameters.Remove(dynamicParameter);
            }

            bool isDynamic = dynamicParameter != null || context.Result?.SimulationConfiguration?.DynamicResistors == true;

            if (isDynamic)
            {
                context.SimulationPreparations.ExecuteTemperatureBehaviorBeforeLoad(res);
            }

            context.CreateNodes(res, parameters);

            if (parameters.Count == 3)
            {
                // RName Node1 Node2 something
                var something = parameters[2];

                // Check if something is a model name
                if ((something is WordParameter || something is IdentifierParameter)
                    && context.ModelsRegistry.FindModel<ResistorModel>(parameters.Get(2).Image) != null)
                {
                    // RName Node1 Node2 modelName
                    throw new ReadingException("L parameter needs to be specified", something.LineNumber);
                }

                // Check if something can be resistance
                if (!(something is WordParameter
                     || something is IdentifierParameter
                     || something is ValueParameter
                     || something is ExpressionParameter
                     || (something is AssignmentParameter ap && (ap.Name.ToLower() == "r" || ap.Name.ToLower() == "resistance"))))
                {
                    throw new ReadingException("Third parameter needs to represent resistance of resistor", something.LineNumber);
                }

                // Set resistance
                if (something is AssignmentParameter asp)
                {
                    context.SetParameter(res, "resistance", asp, true, isDynamic);
                }
                else
                {
                    context.SetParameter(res, "resistance", something, true, isDynamic);
                }
            }
            else
            {
                var resistorParameters = new List<Parameter>(parameters.Skip(Resistor.ResistorPinCount).ToArray());

                // RName Node1 Node2 something param1 ...
                if (resistorParameters.Count == 0)
                {
                    throw new WrongParametersCountException("Resistor doesn't have at least 3 parameters", parameters.LineNumber);
                }

                var something = resistorParameters[0];

                // Check if something is a model name
                bool hasModelSyntax = (something is WordParameter || something is IdentifierParameter)
                                      && context.ModelsRegistry.FindModel<ResistorModel>(something.Image) != null;
                bool hasTcParameter = parameters.Any(
                    p => p is AssignmentParameter ap && ap.Name.Equals(
                             "tc",
                             context.CaseSensitivity.IsEntityParameterNameCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));

                AssignmentParameter tcParameter = null;

                if (hasTcParameter)
                {
                    tcParameter = (AssignmentParameter)parameters.Single(
                        p => p is AssignmentParameter ap && ap.Name.Equals(
                                 "tc",
                                 context.CaseSensitivity.IsEntityParameterNameCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));
                    resistorParameters.Remove(tcParameter);
                }

                if (hasModelSyntax)
                {
                    var modelNameParameter = resistorParameters[0];

                    // Ignore tc parameter on resistor ...
                    context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                    {
                        context.ModelsRegistry.SetModel(
                            res,
                            simulation,
                            modelNameParameter,
                            $"Could not find model {modelNameParameter} for resistor {name}",
                            (ResistorModel model) => res.Model = model.Name,
                            context.Result);
                    });

                    resistorParameters.RemoveAt(0);

                    if (resistorParameters.Count > 0 && (resistorParameters[0] is WordParameter
                                                         || resistorParameters[0] is IdentifierParameter
                                                         || resistorParameters[0] is ValueParameter
                                                         || resistorParameters[0] is ExpressionParameter))
                    {
                        context.SetParameter(res, "resistance", resistorParameters[0].Image, true);
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
                        res.Model = model.Name;
                        context.Result.AddEntity(model);
                    }

                    // Check if something can be resistance
                    var resistanceParameter = resistorParameters[0];

                    if (!(resistanceParameter is WordParameter
                         || resistanceParameter is IdentifierParameter
                         || resistanceParameter is ValueParameter
                         || resistanceParameter is ExpressionParameter
                         || (resistanceParameter is AssignmentParameter ap && !(ap.Name.ToLower() == "r" || ap.Name.ToLower() == "resistance"))))
                    {
                        throw new ReadingException("Invalid value for resistance", resistanceParameter.LineNumber);
                    }

                    if (resistanceParameter is AssignmentParameter asp)
                    {
                        context.SetParameter(res, "resistance", asp.Value, true, isDynamic);
                    }
                    else
                    {
                        context.SetParameter(res, "resistance", resistanceParameter.Image, true, isDynamic);
                    }

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
                        throw new ReadingException("Invalid parameter for resistor: " + parameter.Image);
                    }
                }
            }

            return res;
        }
    }
}