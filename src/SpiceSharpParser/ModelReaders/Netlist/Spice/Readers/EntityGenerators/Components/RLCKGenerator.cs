using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Components.Capacitors;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public class RLCKGenerator : ComponentGenerator
    {
        private const int CapacitorPinCount = 2;

        public override IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "r": return GenerateRes(componentIdentifier, originalName, parameters, context);
                case "l": return GenerateInd(componentIdentifier, originalName, parameters, context);
                case "c": return GenerateCap(componentIdentifier, originalName, parameters, context);
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
        protected IEntity GenerateMut(string name, ParameterCollection parameters, IReadingContext context)
        {
            var mut = new MutualInductance(name);

            switch (parameters.Count)
            {
                case 0:
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Inductor name expected for mutual inductance \"{name}\"",
                        parameters.LineInfo);

                    return null;
                case 1:
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Inductor name expected",
                        parameters.LineInfo);
                    return null;

                case 2:
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Coupling factor expected",
                        parameters.LineInfo);
                    return null;
            }

            if (!(parameters[0] is SingleParameter))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Component name expected",
                    parameters.LineInfo);
                return null;
            }

            if (!(parameters[1] is SingleParameter))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Component name expected",
                    parameters.LineInfo);
                return null;
            }

            mut.InductorName1 = parameters.Get(0).Value;
            mut.InductorName2 = parameters.Get(1).Value;

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
        protected IComponent GenerateCap(string name, string originalName, ParameterCollection parameters, IReadingContext context)
        {
            var externalParameters = parameters;
            parameters = PrepareLtspiceCapacitorParameters(name, originalName, parameters, context, out var parasiticOptions);

            if (parameters.Count >= 3 && LTspiceParameterClassifier.TryRejectUnsupportedPassiveValue(context, name, "C", parameters[2]))
            {
                return null;
            }

            if (parameters.Count >= 3)
            {
                // CXXXXXXX N1 N2 VALUE
                var evalContext = context.EvaluationContext;

                var something = parameters[2];
                string expression;

                if (something is AssignmentParameter asp)
                {
                    expression = $"({asp.Value}) * x";
                }
                else
                {
                    expression = $"({something.Value}) * x";
                }

                if (evalContext.HaveSpiceProperties(expression))
                {
                    BehavioralCapacitor behavioralCapacitor = new BehavioralCapacitor(name);
                    context.CreateNodes(behavioralCapacitor, parameters.Take(BehavioralCapacitor.BehavioralCapacitorPinCount));

                    var mParameter = parameters.FirstOrDefault(p => p is AssignmentParameter p1 && p1.Name.ToLower() == "m");
                    var nParameter = parameters.FirstOrDefault(p => p is AssignmentParameter p1 && p1.Name.ToLower() == "n");

                    // the reverse order is intended. Expression defines the capacitance.
                    expression = MultiplyIfNeeded(expression, ((AssignmentParameter)nParameter)?.Value, ((AssignmentParameter)mParameter)?.Value);

                    behavioralCapacitor.Parameters.Expression = expression;
                    behavioralCapacitor.Parameters.ParseAction = (expression) =>
                    {
                        var parser = context.CreateExpressionResolver(null);
                        return parser.Resolve(expression);
                    };

                    evalContext.Parameters.Add("x", new SpiceSharpParser.Common.Evaluation.Expressions.ConstantExpression(1));

                    if (evalContext.HaveFunctions(expression))
                    {
                        context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                        {
                            behavioralCapacitor.Parameters.Expression = expression;

                            behavioralCapacitor.Parameters.ParseAction = (expression) =>
                            {
                                var parser = context.CreateExpressionResolver(simulation);
                                return parser.Resolve(expression);
                            };
                        });
                    }

                    evalContext.Parameters.Remove("x");

                    return ApplyLtspiceCapacitorParasitics(name, externalParameters, context, parasiticOptions, behavioralCapacitor);
                }
            }

            var capacitor = new Capacitor(name);
            context.CreateNodes(capacitor, parameters);

            // Get TC Parameter
            Parameter tcParameter = parameters.FirstOrDefault(
                p => p is AssignmentParameter ap && ap.Name.Equals(
                         "tc",
                         context.ReaderSettings.CaseSensitivity.IsParameterNameCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));

            if (tcParameter != null)
            {
                parameters.Remove(tcParameter);
            }

            bool modelBased = false;

            if (parameters.Count == 3)
            {
                // CXXXXXXX N1 N2 VALUE
                if (parameters[2] is ValueParameter || parameters[2] is ExpressionParameter || parameters[2] is WordParameter)
                {
                    context.SetParameter(capacitor, "capacitance", parameters.Get(2), true);
                }
                else
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Wrong parameter value for capacitance",
                        parameters.LineInfo);
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
                if (parameters[2] is ValueParameter)
                {
                    context.SetParameter(capacitor, "capacitance", parameters.Get(2), true);
                }
                else
                {
                    context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                    {
                        double? l = GetAssignmentParameterValue("l", parameters, context);
                        double? w = GetAssignmentParameterValue("w", parameters, context);

                        context.ModelsRegistry.SetModel(
                            capacitor,
                            CreateRangePredicate(("l", l), ("w", w)),
                            simulation,
                            parameters.Get(2),
                            $"Could not find model {parameters.Get(2)} for capacitor {name}",
                            (Context.Models.Model model) => capacitor.Model = model.Name,
                            context);
                    });

                    modelBased = true;
                }

                SetParameters(context, capacitor, parameters.Skip(3), "C", name);

                if (modelBased)
                {
                    var length = capacitor.Parameters.Length;
                    if (!length.Given)
                    {
                        context.Result.ValidationResult.AddError(
                            ValidationEntrySource.Reader,
                            $"L needs to be specified",
                            parameters.LineInfo);

                        return null;
                    }
                }
            }

            if (tcParameter != null)
            {
                var tcParameterAssignment = tcParameter as AssignmentParameter;

                if (tcParameterAssignment == null)
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"TC needs to be assignment parameter",
                        parameters.LineInfo);
                    return null;
                }

                if (modelBased)
                {
                    double? l = GetAssignmentParameterValue("l", parameters, context);
                    double? w = GetAssignmentParameterValue("w", parameters, context);
                    var model = context.ModelsRegistry.FindModelEntity(parameters.Get(2).Value, CreateRangePredicate(("l", l), ("w", w)));

                    if (tcParameterAssignment.Values.Count == 2)
                    {
                        context.SetParameter(model, "tc1", tcParameterAssignment.Values[0], true);
                        context.SetParameter(model, "tc2", tcParameterAssignment.Values[1], true);
                    }
                    else
                    {
                        context.SetParameter(model, "tc1", tcParameterAssignment.Value);
                    }

                    context.ContextEntities.Add(model);
                    capacitor.Model = model.Name;
                }
                else
                {
                    var model = new CapacitorModel(capacitor.Name + "_default_model");
                    if (tcParameterAssignment.Values.Count == 2)
                    {
                        context.SetParameter(model, "tc1", tcParameterAssignment.Values[0], true);
                        context.SetParameter(model, "tc2", tcParameterAssignment.Values[1], true);
                    }
                    else
                    {
                        context.SetParameter(model, "tc1", tcParameterAssignment.Value);
                    }

                    context.ModelsRegistry.RegisterModelInstance(new Context.Models.Model(model.Name, model, model.Parameters));
                    context.ContextEntities.Add(model);
                    capacitor.Model = model.Name;
                }
            }

            return ApplyLtspiceCapacitorParasitics(name, externalParameters, context, parasiticOptions, capacitor);
        }

        private ParameterCollection PrepareLtspiceCapacitorParameters(
            string capacitorName,
            string originalName,
            ParameterCollection parameters,
            IReadingContext context,
            out LtspiceCapacitorParasiticOptions parasiticOptions)
        {
            var capacitorParameters = new ParameterCollection(parameters.ToList());
            parasiticOptions = ExtractLtspiceCapacitorParasitics(capacitorParameters, context);

            if ((parasiticOptions.HasSeriesResistance || parasiticOptions.HasSeriesInductance)
                && capacitorParameters.Count >= CapacitorPinCount)
            {
                var baseName = originalName ?? capacitorName;
                var capacitorPositiveNodeName = string.Empty;

                if (parasiticOptions.HasSeriesResistance)
                {
                    parasiticOptions.SeriesResistanceNodeName = "__ltspice_" + baseName + "_rser";
                    capacitorPositiveNodeName = parasiticOptions.SeriesResistanceNodeName;
                }

                if (parasiticOptions.HasSeriesInductance)
                {
                    parasiticOptions.SeriesInductanceNodeName = "__ltspice_" + baseName + "_lser";
                    capacitorPositiveNodeName = parasiticOptions.SeriesInductanceNodeName;
                }

                capacitorParameters.RemoveAt(0);
                capacitorParameters.Insert(0, new IdentifierParameter(capacitorPositiveNodeName, parameters[0].LineInfo));
            }

            return capacitorParameters;
        }

        private IComponent ApplyLtspiceCapacitorParasitics(
            string capacitorName,
            ParameterCollection externalParameters,
            IReadingContext context,
            LtspiceCapacitorParasiticOptions parasiticOptions,
            IComponent component)
        {
            if (component == null || !parasiticOptions.HasAny || externalParameters.Count < CapacitorPinCount)
            {
                return component;
            }

            Parameter seriesInputNode = externalParameters[0];

            if (parasiticOptions.HasSeriesResistance)
            {
                var seriesOutputNode = new IdentifierParameter(parasiticOptions.SeriesResistanceNodeName, externalParameters[0].LineInfo);
                var seriesResistor = new Resistor(capacitorName + "_rser");
                context.CreateNodes(seriesResistor, CreateNodeParameters(seriesInputNode, seriesOutputNode));
                context.SetParameter(seriesResistor, "resistance", GetLtspiceParasiticValue(parasiticOptions.SeriesResistance), true);
                context.ContextEntities?.Add(seriesResistor);
                seriesInputNode = seriesOutputNode;
            }

            if (parasiticOptions.HasSeriesInductance)
            {
                var seriesOutputNode = new IdentifierParameter(parasiticOptions.SeriesInductanceNodeName, externalParameters[0].LineInfo);
                var seriesInductor = new Inductor(capacitorName + "_lser");
                context.CreateNodes(seriesInductor, CreateNodeParameters(seriesInputNode, seriesOutputNode));
                context.SetParameter(seriesInductor, "inductance", GetLtspiceParasiticValue(parasiticOptions.SeriesInductance), true);
                context.ContextEntities?.Add(seriesInductor);
            }

            if (parasiticOptions.HasParallelResistance)
            {
                var parallelResistor = new Resistor(capacitorName + "_rpar");
                context.CreateNodes(parallelResistor, CreateNodeParameters(externalParameters[0], externalParameters[1]));
                context.SetParameter(parallelResistor, "resistance", GetLtspiceParasiticValue(parasiticOptions.ParallelResistance), true);
                context.ContextEntities?.Add(parallelResistor);
            }

            if (parasiticOptions.HasParallelCapacitance)
            {
                var parallelCapacitor = new Capacitor(capacitorName + "_cpar");
                context.CreateNodes(parallelCapacitor, CreateNodeParameters(externalParameters[0], externalParameters[1]));
                context.SetParameter(parallelCapacitor, "capacitance", GetLtspiceParasiticValue(parasiticOptions.ParallelCapacitance), true);
                context.ContextEntities?.Add(parallelCapacitor);
            }

            return component;
        }

        private static LtspiceCapacitorParasiticOptions ExtractLtspiceCapacitorParasitics(
            ParameterCollection parameters,
            IReadingContext context)
        {
            var result = new LtspiceCapacitorParasiticOptions();

            if (!context.ReaderSettings.Compatibility.IsLTspice || parameters.Count < CapacitorPinCount + 1)
            {
                return result;
            }

            for (var i = parameters.Count - 1; i >= CapacitorPinCount + 1; i--)
            {
                if (parameters[i] is AssignmentParameter assignment)
                {
                    if (assignment.Name.Equals("rser", StringComparison.OrdinalIgnoreCase))
                    {
                        result.SeriesResistance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                    else if (assignment.Name.Equals("lser", StringComparison.OrdinalIgnoreCase))
                    {
                        result.SeriesInductance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                    else if (assignment.Name.Equals("rpar", StringComparison.OrdinalIgnoreCase))
                    {
                        result.ParallelResistance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                    else if (assignment.Name.Equals("cpar", StringComparison.OrdinalIgnoreCase))
                    {
                        result.ParallelCapacitance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                }
            }

            return result;
        }

        private sealed class LtspiceCapacitorParasiticOptions
        {
            public Parameter SeriesResistance { get; set; }

            public Parameter SeriesInductance { get; set; }

            public Parameter ParallelResistance { get; set; }

            public Parameter ParallelCapacitance { get; set; }

            public string SeriesResistanceNodeName { get; set; }

            public string SeriesInductanceNodeName { get; set; }

            public bool HasSeriesResistance => SeriesResistance != null;

            public bool HasSeriesInductance => SeriesInductance != null;

            public bool HasParallelResistance => ParallelResistance != null;

            public bool HasParallelCapacitance => ParallelCapacitance != null;

            public bool HasAny => HasSeriesResistance || HasSeriesInductance || HasParallelResistance || HasParallelCapacitance;
        }

        /// <summary>
        /// Generates a new inductor.
        /// </summary>
        /// <param name="name">Name of inductor to generate.</param>
        /// <param name="originalName">Original component name before reader scoping.</param>
        /// <param name="parameters">Parameters and pins for inductor.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of inductor.
        /// </returns>
        protected IEntity GenerateInd(string name, string originalName, ParameterCollection parameters, IReadingContext context)
        {
            var externalParameters = parameters;
            parameters = PrepareLtspiceInductorParameters(name, originalName, parameters, context, out var parasiticOptions);

            if (parameters.Count < 3)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Inductor expects at least 3 parameters",
                    parameters.LineInfo);

                return null;
            }

            if (LTspiceParameterClassifier.TryRejectUnsupportedPassiveValue(context, name, "L", parameters[2]))
            {
                return null;
            }

            var inductor = new Inductor(name);
            context.CreateNodes(inductor, parameters.Take(Inductor.InductorPinCount));
            context.SetParameter(inductor, "inductance", parameters.Get(2), true);
            SetParameters(context, inductor, parameters.Skip(Inductor.InductorPinCount + 1), "L", name);

            return ApplyLtspiceInductorParasitics(name, externalParameters, context, parasiticOptions, inductor);
        }

        private ParameterCollection PrepareLtspiceInductorParameters(
            string inductorName,
            string originalName,
            ParameterCollection parameters,
            IReadingContext context,
            out LtspiceInductorParasiticOptions parasiticOptions)
        {
            var inductorParameters = new ParameterCollection(parameters.ToList());
            parasiticOptions = ExtractLtspiceInductorParasitics(inductorParameters, context);

            if ((parasiticOptions.HasSeriesResistance || parasiticOptions.HasSeriesInductance)
                && inductorParameters.Count >= Inductor.InductorPinCount)
            {
                var baseName = originalName ?? inductorName;
                var inductorPositiveNodeName = string.Empty;

                if (parasiticOptions.HasSeriesResistance)
                {
                    parasiticOptions.SeriesResistanceNodeName = "__ltspice_" + baseName + "_rser";
                    inductorPositiveNodeName = parasiticOptions.SeriesResistanceNodeName;
                }

                if (parasiticOptions.HasSeriesInductance)
                {
                    parasiticOptions.SeriesInductanceNodeName = "__ltspice_" + baseName + "_lser";
                    inductorPositiveNodeName = parasiticOptions.SeriesInductanceNodeName;
                }

                inductorParameters.RemoveAt(0);
                inductorParameters.Insert(0, new IdentifierParameter(inductorPositiveNodeName, parameters[0].LineInfo));
            }

            return inductorParameters;
        }

        private IEntity ApplyLtspiceInductorParasitics(
            string inductorName,
            ParameterCollection externalParameters,
            IReadingContext context,
            LtspiceInductorParasiticOptions parasiticOptions,
            IEntity entity)
        {
            if (entity == null || !parasiticOptions.HasAny || externalParameters.Count < Inductor.InductorPinCount)
            {
                return entity;
            }

            Parameter seriesInputNode = externalParameters[0];

            if (parasiticOptions.HasSeriesResistance)
            {
                var seriesOutputNode = new IdentifierParameter(parasiticOptions.SeriesResistanceNodeName, externalParameters[0].LineInfo);
                var seriesResistor = new Resistor(inductorName + "_rser");
                context.CreateNodes(seriesResistor, CreateNodeParameters(seriesInputNode, seriesOutputNode));
                context.SetParameter(seriesResistor, "resistance", GetLtspiceParasiticValue(parasiticOptions.SeriesResistance), true);
                context.ContextEntities?.Add(seriesResistor);
                seriesInputNode = seriesOutputNode;
            }

            if (parasiticOptions.HasSeriesInductance)
            {
                var seriesOutputNode = new IdentifierParameter(parasiticOptions.SeriesInductanceNodeName, externalParameters[0].LineInfo);
                var seriesInductor = new Inductor(inductorName + "_lser");
                context.CreateNodes(seriesInductor, CreateNodeParameters(seriesInputNode, seriesOutputNode));
                context.SetParameter(seriesInductor, "inductance", GetLtspiceParasiticValue(parasiticOptions.SeriesInductance), true);
                context.ContextEntities?.Add(seriesInductor);
            }

            if (parasiticOptions.HasParallelResistance)
            {
                var parallelResistor = new Resistor(inductorName + "_rpar");
                context.CreateNodes(parallelResistor, CreateNodeParameters(externalParameters[0], externalParameters[1]));
                context.SetParameter(parallelResistor, "resistance", GetLtspiceParasiticValue(parasiticOptions.ParallelResistance), true);
                context.ContextEntities?.Add(parallelResistor);
            }

            if (parasiticOptions.HasShuntResistance)
            {
                var shuntResistor = new Resistor(inductorName + "_rlshunt");
                context.CreateNodes(shuntResistor, CreateNodeParameters(externalParameters[0], externalParameters[1]));
                context.SetParameter(shuntResistor, "resistance", GetLtspiceParasiticValue(parasiticOptions.ShuntResistance), true);
                context.ContextEntities?.Add(shuntResistor);
            }

            if (parasiticOptions.HasParallelCapacitance)
            {
                var parallelCapacitor = new Capacitor(inductorName + "_cpar");
                context.CreateNodes(parallelCapacitor, CreateNodeParameters(externalParameters[0], externalParameters[1]));
                context.SetParameter(parallelCapacitor, "capacitance", GetLtspiceParasiticValue(parasiticOptions.ParallelCapacitance), true);
                context.ContextEntities?.Add(parallelCapacitor);
            }

            return entity;
        }

        private static LtspiceInductorParasiticOptions ExtractLtspiceInductorParasitics(
            ParameterCollection parameters,
            IReadingContext context)
        {
            var result = new LtspiceInductorParasiticOptions();

            if (!context.ReaderSettings.Compatibility.IsLTspice || parameters.Count < Inductor.InductorPinCount + 1)
            {
                return result;
            }

            for (var i = parameters.Count - 1; i >= Inductor.InductorPinCount + 1; i--)
            {
                if (parameters[i] is AssignmentParameter assignment)
                {
                    if (assignment.Name.Equals("rser", StringComparison.OrdinalIgnoreCase))
                    {
                        result.SeriesResistance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                    else if (assignment.Name.Equals("lser", StringComparison.OrdinalIgnoreCase))
                    {
                        result.SeriesInductance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                    else if (assignment.Name.Equals("rpar", StringComparison.OrdinalIgnoreCase))
                    {
                        result.ParallelResistance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                    else if (assignment.Name.Equals("rlshunt", StringComparison.OrdinalIgnoreCase))
                    {
                        result.ShuntResistance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                    else if (assignment.Name.Equals("cpar", StringComparison.OrdinalIgnoreCase))
                    {
                        result.ParallelCapacitance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                }
            }

            return result;
        }

        private sealed class LtspiceInductorParasiticOptions
        {
            public Parameter SeriesResistance { get; set; }

            public Parameter SeriesInductance { get; set; }

            public Parameter ParallelResistance { get; set; }

            public Parameter ShuntResistance { get; set; }

            public Parameter ParallelCapacitance { get; set; }

            public string SeriesResistanceNodeName { get; set; }

            public string SeriesInductanceNodeName { get; set; }

            public bool HasSeriesResistance => SeriesResistance != null;

            public bool HasSeriesInductance => SeriesInductance != null;

            public bool HasParallelResistance => ParallelResistance != null;

            public bool HasShuntResistance => ShuntResistance != null;

            public bool HasParallelCapacitance => ParallelCapacitance != null;

            public bool HasAny => HasSeriesResistance || HasSeriesInductance || HasParallelResistance || HasShuntResistance || HasParallelCapacitance;
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
        protected IEntity GenerateRes(string name, string originalName, ParameterCollection parameters, IReadingContext context)
        {
            var externalParameters = parameters;
            parameters = PrepareLtspiceResistorParameters(name, originalName, parameters, context, out var parasiticOptions);

            if (parameters.Count >= 3 && LTspiceParameterClassifier.TryRejectUnsupportedPassiveValue(context, name, "R", parameters[2]))
            {
                return null;
            }

            if (parameters.Count >= 3)
            {
                var evalContext = context.EvaluationContext;

                // RName Node1 Node2 something
                var something = parameters[2];
                string expression = null;

                if (something is AssignmentParameter asp)
                {
                    expression = asp.Value;
                }
                else
                {
                    expression = something.Value;
                }

                if (evalContext.HaveSpiceProperties(expression))
                {
                    BehavioralResistor behavioralResistor = new BehavioralResistor(name);
                    context.CreateNodes(behavioralResistor, parameters.Take(BehavioralResistor.BehavioralResistorPinCount));

                    var mParameter = parameters.FirstOrDefault(p => p is AssignmentParameter p1 && p1.Name.ToLower() == "m");
                    var nParameter = parameters.FirstOrDefault(p => p is AssignmentParameter p1 && p1.Name.ToLower() == "n");

                    expression = MultiplyIfNeeded(expression, ((AssignmentParameter)mParameter)?.Value, ((AssignmentParameter)nParameter)?.Value);

                    behavioralResistor.Parameters.Expression = expression;
                    behavioralResistor.Parameters.ParseAction = (expression) =>
                    {
                        var parser = context.CreateExpressionResolver(null);
                        return parser.Resolve(expression);
                    };

                    if (evalContext.HaveFunctions(expression))
                    {
                        context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                        {
                            behavioralResistor.Parameters.Expression = expression.ToString();

                            behavioralResistor.Parameters.ParseAction = (expression) =>
                            {
                                var parser = context.CreateExpressionResolver(simulation);
                                return parser.Resolve(expression);
                            };
                        });
                    }

                    return ApplyLtspiceResistorParasitics(name, externalParameters, context, parasiticOptions, behavioralResistor);
                }
            }

            Resistor res = new Resistor(name);
            context.CreateNodes(res, parameters.Take(Resistor.ResistorPinCount));

            if (parameters.Count == 3)
            {
                // RName Node1 Node2 something
                var something = parameters[2];

                bool modelBased = false;
                bool resistanceBased = false;

                // Check if something is a model name
                if ((something is WordParameter || something is IdentifierParameter)
                    && context.ModelsRegistry.FindModel(something.Value) != null)
                {
                    modelBased = true;
                }

                // Check if something can be resistance
                if (!modelBased && (something is WordParameter
                                    || something is IdentifierParameter
                                    || something is ValueParameter
                                    || something is ExpressionParameter
                                    || (something is AssignmentParameter ap &&
                                        (ap.Name.ToLower() == "r" || ap.Name.ToLower() == "resistance"))))
                {
                    resistanceBased = true;
                }

                if (resistanceBased)
                {
                    context.SetParameter(res, "resistance", something, beforeTemperature: true);
                    return ApplyLtspiceResistorParasitics(name, externalParameters, context, parasiticOptions, res);
                }

                if (modelBased)
                {
                    context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                    {
                        double? l = GetAssignmentParameterValue("l", parameters, context);
                        double? w = GetAssignmentParameterValue("w", parameters, context);

                        context.ModelsRegistry.SetModel(
                            res,
                            CreateRangePredicate(("l", l), ("w", w)),
                            simulation,
                            something,
                            $"Could not find model {something} for resistor {name}",
                            (Context.Models.Model model) => res.Model = model.Name,
                            context);
                    });

                    return ApplyLtspiceResistorParasitics(name, externalParameters, context, parasiticOptions, res);
                }

                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Resistance or model name needs to be specified for resistor",
                    parameters.LineInfo);
                return null;
            }
            else
            {
                var resistorParameters = new List<Parameter>(parameters.Skip(Resistor.ResistorPinCount).ToArray());

                // RName Node1 Node2 something param1 ...
                if (resistorParameters.Count == 0)
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Resistor doesn't have at least 3 parameters",
                        parameters.LineInfo);
                    return null;
                }

                var something = resistorParameters[0];

                // Check if something is a model name
                bool hasModelSyntax = (something is WordParameter || something is IdentifierParameter)
                                      && context.ModelsRegistry.FindModel(something.Value) != null;
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
                        double? l = GetAssignmentParameterValue("l", parameters, context);
                        double? w = GetAssignmentParameterValue("w", parameters, context);

                        context.ModelsRegistry.SetModel(
                            res,
                            CreateRangePredicate(("l", l), ("w", w)),
                            simulation,
                            modelNameParameter,
                            $"Could not find model {modelNameParameter} for resistor {name}",
                            (Context.Models.Model model) => res.Model = model.Name,
                            context);
                    });

                    resistorParameters.RemoveAt(0);

                    if (resistorParameters.Count > 0 && (resistorParameters[0] is WordParameter
                                                         || resistorParameters[0] is IdentifierParameter
                                                         || resistorParameters[0] is ValueParameter
                                                         || resistorParameters[0] is ExpressionParameter))
                    {
                        context.SetParameter(res, "resistance", resistorParameters[0].Value, true);
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
                        context.ContextEntities.Add(model);
                    }

                    // Check if something can be resistance
                    var resistanceParameter = resistorParameters[0];

                    if (!(resistanceParameter is WordParameter
                         || resistanceParameter is IdentifierParameter
                         || resistanceParameter is ValueParameter
                         || resistanceParameter is ExpressionParameter
                         || (resistanceParameter is AssignmentParameter ap && (ap.Name.ToLower() == "r" || ap.Name.ToLower() == "resistance"))))
                    {
                        context.Result.ValidationResult.AddError(
                            ValidationEntrySource.Reader,
                            $"Invalid value for resistance",
                            parameters.LineInfo);
                        return null;
                    }

                    if (resistanceParameter is AssignmentParameter asp)
                    {
                        context.SetParameter(res, "resistance", asp.Value, true);
                    }
                    else
                    {
                        context.SetParameter(res, "resistance", resistanceParameter.Value, true);
                    }

                    resistorParameters.RemoveAt(0);
                }

                foreach (var parameter in resistorParameters)
                {
                    if (parameter is AssignmentParameter ap)
                    {
                        if (LTspiceParameterClassifier.TryHandleComponentParameter(context, res, name, "R", ap))
                        {
                            continue;
                        }

                        try
                        {
                            context.SetParameter(res, ap.Name, ap.Value);
                        }
                        catch (Exception e) when (ReaderExceptionClassifier.IsRecoverableInputException(e))
                        {
                            context.Result.ValidationResult.AddError(
                                ValidationEntrySource.Reader,
                                $"Can't set parameter for resistor: '{parameter}'",
                                parameters.LineInfo,
                                exception: e);

                            return null;
                        }
                    }
                    else
                    {
                        context.Result.ValidationResult.AddError(
                            ValidationEntrySource.Reader,
                            $"Invalid parameter for resistor: '{parameter}'",
                            parameters.LineInfo);

                        return null;
                    }
                }
            }

            return ApplyLtspiceResistorParasitics(name, externalParameters, context, parasiticOptions, res);
        }

        private ParameterCollection PrepareLtspiceResistorParameters(
            string resistorName,
            string originalName,
            ParameterCollection parameters,
            IReadingContext context,
            out LtspiceResistorParasiticOptions parasiticOptions)
        {
            var resistorParameters = new ParameterCollection(parameters.ToList());
            parasiticOptions = ExtractLtspiceResistorParasitics(resistorParameters, context);

            if (parasiticOptions.HasSeriesResistance && resistorParameters.Count >= Resistor.ResistorPinCount)
            {
                parasiticOptions.SeriesNodeName = "__ltspice_" + (originalName ?? resistorName) + "_rser";
                resistorParameters.RemoveAt(0);
                resistorParameters.Insert(0, new IdentifierParameter(parasiticOptions.SeriesNodeName, parameters[0].LineInfo));
            }

            return resistorParameters;
        }

        private IEntity ApplyLtspiceResistorParasitics(
            string resistorName,
            ParameterCollection externalParameters,
            IReadingContext context,
            LtspiceResistorParasiticOptions parasiticOptions,
            IEntity entity)
        {
            if (entity == null || !parasiticOptions.HasAny || externalParameters.Count < Resistor.ResistorPinCount)
            {
                return entity;
            }

            if (parasiticOptions.HasSeriesResistance)
            {
                var seriesResistor = new Resistor(resistorName + "_rser");
                context.CreateNodes(
                    seriesResistor,
                    CreateNodeParameters(
                        externalParameters[0],
                        new IdentifierParameter(parasiticOptions.SeriesNodeName, externalParameters[0].LineInfo)));
                context.SetParameter(seriesResistor, "resistance", GetLtspiceParasiticValue(parasiticOptions.SeriesResistance), true);
                context.ContextEntities?.Add(seriesResistor);
            }

            if (parasiticOptions.HasParallelResistance)
            {
                var parallelResistor = new Resistor(resistorName + "_rpar");
                context.CreateNodes(parallelResistor, CreateNodeParameters(externalParameters[0], externalParameters[1]));
                context.SetParameter(parallelResistor, "resistance", GetLtspiceParasiticValue(parasiticOptions.ParallelResistance), true);
                context.ContextEntities?.Add(parallelResistor);
            }

            if (parasiticOptions.HasParallelCapacitance)
            {
                var parallelCapacitor = new Capacitor(resistorName + "_cpar");
                context.CreateNodes(parallelCapacitor, CreateNodeParameters(externalParameters[0], externalParameters[1]));
                context.SetParameter(parallelCapacitor, "capacitance", GetLtspiceParasiticValue(parasiticOptions.ParallelCapacitance), true);
                context.ContextEntities?.Add(parallelCapacitor);
            }

            return entity;
        }

        private static LtspiceResistorParasiticOptions ExtractLtspiceResistorParasitics(
            ParameterCollection parameters,
            IReadingContext context)
        {
            var result = new LtspiceResistorParasiticOptions();

            if (!context.ReaderSettings.Compatibility.IsLTspice || parameters.Count < Resistor.ResistorPinCount)
            {
                return result;
            }

            for (var i = parameters.Count - 1; i >= Resistor.ResistorPinCount; i--)
            {
                if (parameters[i] is AssignmentParameter assignment)
                {
                    if (assignment.Name.Equals("rser", StringComparison.OrdinalIgnoreCase))
                    {
                        result.SeriesResistance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                    else if (assignment.Name.Equals("rpar", StringComparison.OrdinalIgnoreCase))
                    {
                        result.ParallelResistance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                    else if (assignment.Name.Equals("cpar", StringComparison.OrdinalIgnoreCase))
                    {
                        result.ParallelCapacitance ??= assignment;
                        parameters.RemoveAt(i);
                    }
                }
            }

            return result;
        }

        private static ParameterCollection CreateNodeParameters(Parameter firstNode, Parameter secondNode)
        {
            return new ParameterCollection(new List<Parameter>())
            {
                firstNode,
                secondNode,
            };
        }

        private static string GetLtspiceParasiticValue(Parameter parameter)
        {
            return parameter is AssignmentParameter assignment ? assignment.Value : parameter.Value;
        }

        private sealed class LtspiceResistorParasiticOptions
        {
            public Parameter SeriesResistance { get; set; }

            public Parameter ParallelResistance { get; set; }

            public Parameter ParallelCapacitance { get; set; }

            public string SeriesNodeName { get; set; }

            public bool HasSeriesResistance => SeriesResistance != null;

            public bool HasParallelResistance => ParallelResistance != null;

            public bool HasParallelCapacitance => ParallelCapacitance != null;

            public bool HasAny => HasSeriesResistance || HasParallelResistance || HasParallelCapacitance;
        }

        private string MultiplyIfNeeded(string expression, string mExpression, string nExpression)
        {
            if (!string.IsNullOrEmpty(mExpression) && !string.IsNullOrEmpty(nExpression))
            {
                return $"({expression} / {mExpression}) * {nExpression}";
            }

            if (!string.IsNullOrEmpty(mExpression))
            {
                return $"({expression} / {mExpression})";
            }

            if (!string.IsNullOrEmpty(nExpression))
            {
                return $"({expression} * {nExpression})";
            }

            return expression;
        }

    }
}
