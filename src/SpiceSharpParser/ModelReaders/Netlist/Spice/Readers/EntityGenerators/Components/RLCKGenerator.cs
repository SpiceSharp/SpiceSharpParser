using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common.Validation;
using SpiceSharp.Components.Capacitors;
using SpiceSharp.Entities;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public class RLCKGenerator : ComponentGenerator
    {
        public override IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
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
        protected IEntity GenerateMut(string name, ParameterCollection parameters, ICircuitContext context)
        {
            var mut = new MutualInductance(name);

            switch (parameters.Count)
            {
                case 0:
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"Inductor name expected for mutual inductance \"{name}\"", parameters.LineInfo));
                    return null;
                case 1:
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"Inductor name expected", parameters.LineInfo));
                    return null;

                case 2:
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"Coupling factor expected", parameters.LineInfo));
                    return null;
            }

            if (!(parameters[0] is SingleParameter))
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                    ValidationEntryLevel.Warning,
                    $"Component name expected", parameters.LineInfo));
                return null;
            }

            if (!(parameters[1] is SingleParameter))
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                    ValidationEntryLevel.Warning,
                    $"Component name expected", parameters.LineInfo));
                return null;
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
        protected SpiceSharp.Components.IComponent GenerateCap(string name, ParameterCollection parameters, ICircuitContext context)
        {
            var capacitor = new Capacitor(name);
            context.CreateNodes(capacitor, parameters);

            // Get TC Parameter
            Parameter tcParameter = parameters.FirstOrDefault(
                p => p is AssignmentParameter ap && ap.Name.Equals(
                         "tc",
                         false ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));

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
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"Wrong parameter value for capacitance", parameters.LineInfo));
                    return null;
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
                            (Context.Models.Model model) => capacitor.Model = model.Name,
                            context.Result);
                    });

                    modelBased = true;
                }

                SetParameters(context, capacitor, parameters.Skip(3), true);

                if (modelBased)
                {
                    var bp = capacitor.GetParameterSet<ModelParameters>();
                    /*if (bp == null || !bp.Length.Given)
                    {
                        context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                            ValidationEntryLevel.Warning,
                            $"L needs to be specified", parameters.LineInfo));
                        return null;
                    }*/
                }
            }

            if (tcParameter != null)
            {
                var tcParameterAssignment = tcParameter as AssignmentParameter;

                if (tcParameterAssignment == null)
                {
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"TC needs to be assignment parameter", parameters.LineInfo));
                    return null;
                }

                if (modelBased)
                {
                    var model = context.ModelsRegistry.FindModel(parameters.Get(2).Image);

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

                    context.ModelsRegistry.RegisterModelInstance(new Context.Models.Model(model.Name, model, model.Parameters));
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
        protected IEntity GenerateInd(string name, ParameterCollection parameters, ICircuitContext context)
        {
            if (parameters.Count != 3)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                    ValidationEntryLevel.Warning,
                    $"Inductor expects 3 parameters/pins", parameters.LineInfo));
                return null;
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
        /// <returns>
        /// A new instance of resistor.
        /// </returns>
        protected IEntity GenerateRes(string name, ParameterCollection parameters, ICircuitContext context)
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
                    && context.ModelsRegistry.FindModel(parameters.Get(2).Image) != null)
                {
                    // RName Node1 Node2 modelName
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"R parameter needs to be specified", parameters.LineInfo));
                    return null;
                }

                // Check if something can be resistance
                if (!(something is WordParameter
                     || something is IdentifierParameter
                     || something is ValueParameter
                     || something is ExpressionParameter
                     || (something is AssignmentParameter ap && (ap.Name.ToLower() == "r" || ap.Name.ToLower() == "resistance"))))
                {
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"Third parameter needs to represent resistance of resistor", parameters.LineInfo));
                    return null;
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
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"Resistor doesn't have at least 3 parameters", parameters.LineInfo));
                    return null;
                }

                var something = resistorParameters[0];

                // Check if something is a model name
                bool hasModelSyntax = (something is WordParameter || something is IdentifierParameter)
                                      && context.ModelsRegistry.FindModel(something.Image) != null;
                bool hasTcParameter = parameters.Any(
                    p => p is AssignmentParameter ap && ap.Name.Equals(
                             "tc",
                             false ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));

                AssignmentParameter tcParameter = null;

                if (hasTcParameter)
                {
                    tcParameter = (AssignmentParameter)parameters.Single(
                        p => p is AssignmentParameter ap && ap.Name.Equals(
                                 "tc",
                                 false ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));
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
                            (Context.Models.Model model) => res.Model = model.Name,
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

                        context.ModelsRegistry.RegisterModelInstance(new Context.Models.Model(model.Name, model, model.Parameters));
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
                        context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                            ValidationEntryLevel.Warning,
                            $"Invalid value for resistance", parameters.LineInfo));
                        return null;
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
                        try
                        {
                            context.SetParameter(res, ap.Name, ap.Value);
                        }
                        catch (Exception e)
                        {
                            context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                                ValidationEntryLevel.Warning,
                                $"Can't set parameter for resistor: '{parameter.Image}'", parameters.LineInfo));
                            return null;
                        }
                        
                    }
                    else
                    {
                        context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                            ValidationEntryLevel.Warning,
                            $"Invalid parameter for resistor: '{parameter.Image}'", parameters.LineInfo));
                        return null;
                    }
                }
            }

            return res;
        }
    }
}