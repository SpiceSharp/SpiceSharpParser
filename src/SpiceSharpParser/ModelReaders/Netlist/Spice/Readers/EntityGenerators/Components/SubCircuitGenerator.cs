using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Processors;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;
using Model = SpiceSharpParser.Models.Netlist.Spice.Objects.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    /// <summary>
    /// Generates subcircuits content.
    /// </summary>
    public class SubCircuitGenerator : ComponentGenerator
    {
        public override IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            SubCircuit subCircuit = FindSubcircuitDefinition(parameters, context);
            bool hasParameters = GetAssigmentParametersCount(parameters, out _) != 0;

            if (context.ExpandSubcircuits || hasParameters)
            {
                ICircuitContext subCircuitContext = CreateExpandedSubcircuitContext(componentIdentifier, originalName, subCircuit, parameters, context);
                context.Children.Add(subCircuitContext);
                CreateSubcircuitContent(context, subCircuit, subCircuitContext);
                subCircuitContext.Evaluator.SetEntites(subCircuitContext.ContextEntities);

                foreach (var entity in subCircuitContext.ContextEntities)
                {
                    context.ContextEntities.Add(entity);
                }

                return null;
            }
            else
            {
                ICircuitContext subCircuitContext = CreateLocalSubcircuitContext(componentIdentifier, originalName, subCircuit, parameters, context);
                context.Children.Add(subCircuitContext);

                if (!subCircuitContext.AvailableSubcircuitDefinitions.TryGetValue(subCircuit.Name, out var subCircuitDefinition))
                {
                    CreateSubcircuitContent(context, subCircuit, subCircuitContext);
                    subCircuitContext.Evaluator.SetEntites(subCircuitContext.ContextEntities);
                    subCircuitDefinition = new SubcircuitDefinition(subCircuitContext.ContextEntities, subCircuit.Pins.Select(p => p.ToString()).ToArray());
                    context.AvailableSubcircuitDefinitions[subCircuit.Name] = subCircuitDefinition;
                }

                var pinNames = GetPinNames(parameters);
                var subCircuitEntity = new Subcircuit(componentIdentifier, subCircuitDefinition);
                subCircuitEntity.Connect(pinNames.ToArray());
                subCircuitEntity.Parameters.LocalSolver = context.SimulationConfiguration.LocalSolver;
                return subCircuitEntity;
            }
        }

        private void CreateSubcircuitContent(ICircuitContext context, SubCircuit subCircuit, ICircuitContext subCircuitContext)
        {
            var ifPreprocessor = new IfPreprocessor();
            ifPreprocessor.CaseSettings = subCircuitContext.CaseSensitivity;
            ifPreprocessor.Validation = new SpiceParserValidationResult()
            {
                Reading = context.Result.ValidationResult,
            };
            ifPreprocessor.EvaluationContext = subCircuitContext.Evaluator.GetEvaluationContext();
            subCircuit.Statements = ifPreprocessor.Process(subCircuit.Statements);

            ReadParamControl(subCircuit, subCircuitContext);
            ReadFuncControl(subCircuit, subCircuitContext);
            ReadSubcircuits(subCircuit, subCircuitContext);
            CreateSubcircuitModels(subCircuit, subCircuitContext); // TODO: Share models someday between instances of subcircuits
            CreateSubcircuitComponents(subCircuit, subCircuitContext);
        }

        /// <summary>
        /// Read .PARAM statements.
        /// </summary>
        /// <param name="subCircuitDefinition">A subcircuit definition.</param>
        /// <param name="subCircuitContext">A subcircuit reading context.</param>
        private void ReadParamControl(SubCircuit subCircuitDefinition, ICircuitContext subCircuitContext)
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
        private void ReadFuncControl(SubCircuit subCircuitDefinition, ICircuitContext subCircuitContext)
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
        private void ReadSubcircuits(SubCircuit subCircuitDefinition, ICircuitContext subCircuitContext)
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
        private void CreateSubcircuitComponents(SubCircuit subCircuitDefinition, ICircuitContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefinition.Statements.Where(s => s is Component))
            {
                subCircuitContext.StatementsReader.Read((Component)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Creates models for subcircuit.
        /// </summary>
        /// <param name="subCircuitDefinition">A subcircuit definition.</param>
        /// <param name="subCircuitContext">A subcircuit reading context.</param>
        private void CreateSubcircuitModels(SubCircuit subCircuitDefinition, ICircuitContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefinition.Statements.Where(s => s is Model))
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
        private SubCircuit FindSubcircuitDefinition(ParameterCollection parameters, ICircuitContext context)
        {
            // first step is to find subcircuit name in parameters, a=b parameters needs to be skipped
            int skipCount = 0;
            while (parameters[parameters.Count - skipCount - 1] is AssignmentParameter || parameters[parameters.Count - skipCount - 1].Value.ToLower() == "params:")
            {
                skipCount++;
            }

            string subCircuitDefinitionName = parameters.Get(parameters.Count - skipCount - 1).Value;
            var result = context.AvailableSubcircuits.ToList().Find(subCkt => subCkt.Name == subCircuitDefinitionName);

            if (result == null)
            {
                context.Result.ValidationResult.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Error,
                        $"Could not find '{subCircuitDefinitionName}' subcircuit",
                        parameters.LineInfo));
            }

            return result;
        }

        private List<string> GetPinNames(ParameterCollection parameters)
        {
            var result = new List<string>();

            // setting evaluator
            var parameterParameters = GetAssigmentParametersCount(parameters, out _);

            // setting node name generator
            for (var i = 0; i < parameters.Count - parameterParameters - 1; i++)
            {
                var nodeName = parameters.Get(i).Value;

                result.Add(nodeName);
            }

            return result;
        }

        private static int GetAssigmentParametersCount(ParameterCollection parameters, out List<AssignmentParameter> subCktParameters)
        {
            var parameterParameters = 0;
            subCktParameters = new List<AssignmentParameter>();
            while (true)
            {
                if (parameters[parameters.Count - parameterParameters - 1].Value.ToLower() == "params:")
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

            return parameterParameters;
        }

        /// <summary>
        /// Creates subcircuit parameters dictionary.
        /// </summary>
        /// <param name="subCiruitDefiniton">Subcircuit definition.</param>
        /// <param name="subCircuitParameters">Subcircuit parameters.</param>
        /// <param name="circuitContext">Reading context.</param>
        /// <returns>
        /// A dictionary of parameters.
        /// </returns>
        private Dictionary<string, string> CreateSubcircuitParameters(SubCircuit subCiruitDefiniton, List<AssignmentParameter> subCircuitParameters, ICircuitContext circuitContext)
        {
            var result = new Dictionary<string, string>(StringComparerProvider.Get(circuitContext.CaseSensitivity.IsParameterNameCaseSensitive));

            foreach (var defaultParameter in subCiruitDefiniton.DefaultParameters)
            {
                result[defaultParameter.Name] = defaultParameter.Value;
            }

            foreach (var instanceParameter in subCircuitParameters)
            {
                result[instanceParameter.Name] = instanceParameter.Value;
            }

            foreach (var parameterName in result.Keys.ToList())
            {
                if (parameterName == result[parameterName])
                {
                    result[parameterName] = circuitContext.Evaluator.EvaluateDouble(parameterName).ToString(CultureInfo.InvariantCulture);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates subcircuit context.
        /// </summary>
        /// <param name="subcircuitFullName">Subcircuit full name.</param>
        /// <param name="subcircuitName">Subcircuit name.</param>
        /// <param name="subCircuitDefinition">Subcircuit definition.</param>
        /// <param name="parameters">Parameters and pins for subcircuit.</param>
        /// <param name="context">Parent reading context.</param>
        /// <returns>
        /// A new instance of reading context.
        /// </returns>
        private ICircuitContext CreateExpandedSubcircuitContext(string subcircuitFullName, string subcircuitName, SubCircuit subCircuitDefinition, ParameterCollection parameters, ICircuitContext context)
        {
            var pinNames = GetPinNames(parameters);

            // setting node name generator
            var pinInstanceIdentifiers = new List<string>();
            for (var i = 0; i < pinNames.Count; i++)
            {
                var nodeName = pinNames[i];
                var pinInstanceName = context.NameGenerator.GenerateNodeName(nodeName);
                pinInstanceIdentifiers.Add(pinInstanceName);
            }

            GetAssigmentParametersCount(parameters, out var subCktParameters);
            var subcircuitParameters = CreateSubcircuitParameters(subCircuitDefinition, subCktParameters, context);
            var subcircuitNodeNameGenerator = new SubcircuitNodeNameGenerator(
                subcircuitFullName,
                subcircuitName,
                subCircuitDefinition,
                pinInstanceIdentifiers,
                context.NameGenerator.Globals,
                context.CaseSensitivity.IsEntityNamesCaseSensitive,
                context.Separator);

            var subcircuitObjectNameGenerator = context.NameGenerator.CreateChildNameGenerator(subcircuitName);
            var subcircuitNameGenerator = new NameGenerator(
                subcircuitNodeNameGenerator,
                subcircuitObjectNameGenerator);

            context.NameGenerator.AddChild(subcircuitNodeNameGenerator);
            var subcircuitEvaluationContext = context.Evaluator.CreateChildContext(subcircuitFullName, true);
            subcircuitEvaluationContext.SetParameters(subcircuitParameters);
            subcircuitEvaluationContext.NameGenerator = subcircuitNameGenerator;

            var subcircuitEvaluator = new CircuitEvaluator(new SimulationEvaluationContexts(subcircuitEvaluationContext), subcircuitEvaluationContext);

            var subcircuitContext = new CircuitContext(
                subcircuitName,
                context,
                subcircuitEvaluator,
                context.SimulationPreparations,
                subcircuitNameGenerator,
                context.StatementsReader,
                context.WaveformReader,
                context.CaseSensitivity,
                context.Exporters,
                context.WorkingDirectory,
                true,
                context.SimulationConfiguration,
                context.Result,
                context.Separator);

            subcircuitEvaluationContext.CircuitContext = subcircuitContext;

            return subcircuitContext;
        }

        /// <summary>
        /// Creates subcircuit context.
        /// </summary>
        /// <param name="subcircuitFullName">Subcircuit full name.</param>
        /// <param name="subcircuitName">Subcircuit name.</param>
        /// <param name="subCircuitDefinition">Subcircuit definition.</param>
        /// <param name="parameters">Parameters and pins for subcircuit.</param>
        /// <param name="context">Parent reading context.</param>
        /// <returns>
        /// A new instance of reading context.
        /// </returns>
        private ICircuitContext CreateLocalSubcircuitContext(string subcircuitFullName, string subcircuitName, SubCircuit subCircuitDefinition, ParameterCollection parameters, ICircuitContext context)
        {
            GetAssigmentParametersCount(parameters, out var subCktParameters);
            var subcircuitParameters = CreateSubcircuitParameters(subCircuitDefinition, subCktParameters, context);

            var subcircuitEvaluationContext = context.Evaluator.CreateChildContext(subcircuitFullName, true);
            subcircuitEvaluationContext.SetParameters(subcircuitParameters);

            var subcircuitEvaluator = 
                new CircuitEvaluator(new SimulationEvaluationContexts(subcircuitEvaluationContext), subcircuitEvaluationContext);

            var subcircuitContext = new CircuitContext(
                subcircuitName,
                context,
                subcircuitEvaluator,
                context.SimulationPreparations,
                context.NameGenerator,
                context.StatementsReader,
                context.WaveformReader,
                context.CaseSensitivity,
                context.Exporters,
                context.WorkingDirectory,
                false,
                context.SimulationConfiguration,
                context.Result,
                context.Separator);

            subcircuitEvaluationContext.CircuitContext = subcircuitContext;

            return subcircuitContext;
        }
    }
}