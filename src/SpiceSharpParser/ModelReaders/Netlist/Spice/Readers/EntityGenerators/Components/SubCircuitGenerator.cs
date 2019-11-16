using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Processors;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.Parsers.Expression;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;
using Model = SpiceSharpParser.Models.Netlist.Spice.Objects.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    /// <summary>
    /// Generates subcircuits content.
    /// </summary>
    public class SubCircuitGenerator : ComponentGenerator
    {
        public class CustomComponentInstanceData : ComponentInstanceData
        {
            static CustomComponentInstanceData()
            {
                Utility.Separator = ".";
            }

            public CustomComponentInstanceData(Circuit subcircuit) : base(subcircuit)
            {
            }

            public CustomComponentInstanceData(Circuit subckt, string name) : base(subckt, name)
            {
            }
        }

        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public override IEnumerable<string> GeneratedTypes => new List<string>() { "X" };

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            SubCircuit subCircuitDefinition = FindSubcircuitDefinition(parameters, context);
            ReadingContext subCircuitContext = CreateSubcircuitContext(componentIdentifier, originalName, subCircuitDefinition, parameters, context);

            var ifPreprocessor = new IfPreprocessor();
            ifPreprocessor.Evaluator = context.ReadingEvaluator;
            ifPreprocessor.CaseSettings = subCircuitContext.CaseSensitivity;
            ifPreprocessor.ExpressionContext = subCircuitContext.ReadingExpressionContext;
            subCircuitDefinition.Statements = ifPreprocessor.Process(subCircuitDefinition.Statements);

            ReadParamControl(subCircuitDefinition, subCircuitContext);
            ReadFuncControl(subCircuitDefinition, subCircuitContext);
            ReadSubcircuits(subCircuitDefinition, subCircuitContext);
            CreateSubcircuitModels(subCircuitDefinition, subCircuitContext); // TODO: Share models someday between instances of subcircuits
            CreateSubcircuitComponents(subCircuitDefinition, subCircuitContext);

            context.Children.Add(subCircuitContext);

            // null is intentional
            return null;
        }

        /// <summary>
        /// Read .PARAM statements.
        /// </summary>
        /// <param name="subCircuitDefinition">A subcircuit definition.</param>
        /// <param name="subCircuitContext">A subcircuit reading context.</param>
        private void ReadParamControl(SubCircuit subCircuitDefinition, IReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefinition.Statements.Where(s => s is Control && ((Control)s).Name.ToLower() == "param"))
            {
                subCircuitContext.StatementsReader.Read(statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Read .FUNC statements.
        /// </summary>
        /// <param name="subCircuitDefinition">A subcircuit definition.</param>
        /// <param name="subCircuitContext">A subcircuit reading context.</param>
        private void ReadFuncControl(SubCircuit subCircuitDefinition, IReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefinition.Statements.Where(s => s is Control && ((Control)s).Name.ToLower() == "func"))
            {
                subCircuitContext.StatementsReader.Read(statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Reads .SUBCKT statements.
        /// </summary>
        /// <param name="subCircuitDefinition">A subcircuit definition.</param>
        /// <param name="subCircuitContext">A subcircuit reading context.</param>
        private void ReadSubcircuits(SubCircuit subCircuitDefinition, IReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefinition.Statements.Where(s => s is SubCircuit))
            {
                subCircuitContext.StatementsReader.Read((SubCircuit)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Creates components for subcircuit.
        /// </summary>
        /// <param name="subCircuitDefinition">A subcircuit definition.</param>
        /// <param name="subCircuitContext">A subcircuit reading context.</param>
        private void CreateSubcircuitComponents(SubCircuit subCircuitDefinition, IReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefinition.Statements.Where(s => s is Component))
            {
                subCircuitContext.StatementsReader.Read((Component)statement, subCircuitContext);

                var lastEntity = subCircuitContext.Result.Circuit.Last();

                if (lastEntity.ParameterSets.TryGet(out BaseParameters bp))
                {
                    bp.Instance = subCircuitContext.InstanceData;
                }
            }
        }

        /// <summary>
        /// Creates models for subcircuit.
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definition.</param>
        /// <param name="subCircuitContext">A subcircuit reading context.</param>
        private void CreateSubcircuitModels(SubCircuit subCircuitDefiniton, IReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Model))
            {
                subCircuitContext.StatementsReader.Read(statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Finds subcircuit definition.
        /// </summary>
        /// <param name="parameters">Parameters of subcircuit instance.</param>
        /// <param name="context">A reading context.</param>
        /// <returns>
        /// A reference to subcircuit.
        /// </returns>
        private SubCircuit FindSubcircuitDefinition(ParameterCollection parameters, IReadingContext context)
        {
            // first step is to find subcircuit name in parameters, a=b parameters needs to be skipped
            int skipCount = 0;
            while (parameters[parameters.Count - skipCount - 1] is AssignmentParameter || parameters[parameters.Count - skipCount - 1].Image.ToLower() == "params:")
            {
                skipCount++;
            }

            string subCircuitDefinitionName = parameters.Get(parameters.Count - skipCount - 1).Image;
            var result = context.AvailableSubcircuits.ToList().Find(subCkt => subCkt.Name == subCircuitDefinitionName);

            if (result == null)
            {
                throw new GeneralReaderException("Could not find " + subCircuitDefinitionName + " subcircuit");
            }

            return result;
        }

        /// <summary>
        /// Creates subcircuit context.
        /// </summary>
        /// <param name="subcircuitFullName">Subcircuit full name.</param>
        /// <param name="subcircuitName">Subcircuit name.</param>
        /// <param name="subCircuitDefiniton">Subcircuit definition.</param>
        /// <param name="parameters">Parameters and pins for subcircuit.</param>
        /// <param name="context">Parent reading context.</param>
        /// <returns>
        /// A new instance of reading context.
        /// </returns>
        private ReadingContext CreateSubcircuitContext(string subcircuitFullName, string subcircuitName, SubCircuit subCircuitDefiniton, ParameterCollection parameters, IReadingContext context)
        {
            int parameterParameters = 0;

            // setting evaluator
            var subCktParameters = new List<AssignmentParameter>();
            while (true)
            {
                if (parameters[parameters.Count - parameterParameters - 1].Image.ToLower() == "params:")
                {
                    parameterParameters++;
                }

                if (!(parameters[parameters.Count - parameterParameters - 1] is AssignmentParameter a))
                {
                    break;
                }
                else
                {
                    subCktParameters.Add(a);
                    parameterParameters++;
                }
            }

            var subcircuitParameters = CreateSubcircuitParameters(
                context.ReadingExpressionContext,
                subCircuitDefiniton,
                subCktParameters,
                context.CaseSensitivity,
                context.ReadingEvaluator,
                context);

            var subCircuitExpressionContext = context.ReadingExpressionContext.CreateChildContext(subcircuitFullName, true);

            var pp = new Dictionary<string, ICollection<string>>();
            foreach (var sp in subcircuitParameters)
            {
                pp[sp.Key] = ExpressionParserHelpers.GetExpressionParameters(sp.Value, subCircuitExpressionContext, context,context.CaseSensitivity, false);
            }

            subCircuitExpressionContext.SetParameters(subcircuitParameters, pp);

            // setting node name generator
            var pinInstanceIdentifiers = new List<string>();
            for (var i = 0; i < parameters.Count - parameterParameters - 1; i++)
            {
                var nodeName  = parameters.Get(i).Image;
                var pinInstanceName = context.NodeNameGenerator.Generate(nodeName);
                pinInstanceIdentifiers.Add(pinInstanceName);
            }

            var subcircuitNodeNameGenerator = new SubcircuitNodeNameGenerator(subcircuitFullName, subcircuitName, subCircuitDefiniton, pinInstanceIdentifiers, context.NodeNameGenerator.Globals, context.CaseSensitivity.IsNodeNameCaseSensitive);
            context.NodeNameGenerator.Children.Add(subcircuitNodeNameGenerator);

            ComponentInstanceData instanceData = new CustomComponentInstanceData(context.Result.Circuit, subcircuitFullName);

            foreach (var pin in subCircuitDefiniton.Pins)
            {
                instanceData.NodeMap[pin] = subcircuitNodeNameGenerator.Generate(pin);
            }

            var subcircuitComponentNameGenerator = context.ComponentNameGenerator.CreateChildGenerator(subcircuitName);
            var subcircuitModelNameGenerator = context.ModelNameGenerator.CreateChildGenerator(subcircuitName);

            var subcircuitContext = new ReadingContext(
                subcircuitName,
                context.SimulationPreparations,
                context.SimulationEvaluators,
                context.SimulationExpressionContexts,
                context.Result,
                subcircuitNodeNameGenerator,
                subcircuitComponentNameGenerator,
                subcircuitModelNameGenerator,
                context.StatementsReader,
                context.WaveformReader,
                context.ReadingEvaluator,
                subCircuitExpressionContext,
                context.CaseSensitivity,
                context,
                context.Exporters,
                context.WorkingDirectory);

            subcircuitContext.InstanceData = instanceData;

            return subcircuitContext;
        }

        /// <summary>
        /// Creates subcircuit parameters dictionary.
        /// </summary>
        /// <param name="rootExpressionContext">Expression context.</param>
        /// <param name="subCiruitDefiniton">Subcircuit definition.</param>
        /// <param name="subcktParameters">Subcircuit parameters.</param>
        /// <param name="caseSettings">Case settings.</param>
        /// <param name="evaluator">Evaluator.</param>
        /// <returns>
        /// A dictionary of parameters.
        /// </returns>
        private Dictionary<string, string> CreateSubcircuitParameters(ExpressionContext rootExpressionContext, SubCircuit subCiruitDefiniton, List<AssignmentParameter> subcktParameters, SpiceNetlistCaseSensitivitySettings caseSettings, IEvaluator evaluator, IReadingContext readingContext)
        {
            var result = new Dictionary<string, string>(StringComparerProvider.Get(caseSettings.IsParameterNameCaseSensitive));

            foreach (var defaultParameter in subCiruitDefiniton.DefaultParameters)
            {
                result[defaultParameter.Name] = defaultParameter.Value;
            }

            foreach (var instanceParameter in subcktParameters)
            {
                result[instanceParameter.Name] = instanceParameter.Value;
            }

            foreach (var parameterName in result.Keys.ToList())
            {
                if (parameterName == result[parameterName])
                {
                    result[parameterName] = evaluator.Evaluate(new DynamicExpression(parameterName), rootExpressionContext,null, readingContext).ToString(CultureInfo.InvariantCulture);
                }
            }

            return result;
        }
    }
}