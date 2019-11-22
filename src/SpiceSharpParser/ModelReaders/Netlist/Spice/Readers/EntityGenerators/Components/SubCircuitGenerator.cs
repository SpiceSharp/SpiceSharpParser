using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Processors;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;
using Model = SpiceSharpParser.Models.Netlist.Spice.Objects.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    /// <summary>
    /// Generates subcircuits content.
    /// </summary>
    public class SubCircuitGenerator : ComponentGenerator
    {
        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            SubCircuit subCircuitDefinition = FindSubcircuitDefinition(parameters, context);
            CircuitContext subCircuitContext = CreateSubcircuitContext(componentIdentifier, originalName, subCircuitDefinition, parameters, context);

            var ifPreprocessor = new IfPreprocessor();
            ifPreprocessor.CaseSettings = subCircuitContext.CaseSensitivity;
            ifPreprocessor.EvaluationContext = subCircuitContext.CircuitEvaluator.GetContext();
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
        /// <param name="subCircuitDefinition">Subcircuit definition.</param>
        /// <param name="parameters">Parameters and pins for subcircuit.</param>
        /// <param name="context">Parent reading context.</param>
        /// <returns>
        /// A new instance of reading context.
        /// </returns>
        private CircuitContext CreateSubcircuitContext(string subcircuitFullName, string subcircuitName, SubCircuit subCircuitDefinition, ParameterCollection parameters, ICircuitContext context)
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


            var subcircuitParameters = CreateSubcircuitParameters(subCircuitDefinition, subCktParameters, context);




            // setting node name generator
            var pinInstanceIdentifiers = new List<string>();
            for (var i = 0; i < parameters.Count - parameterParameters - 1; i++)
            {
                var nodeName = parameters.Get(i).Image;
                var pinInstanceName = context.NameGenerator.GenerateNodeName(nodeName);
                pinInstanceIdentifiers.Add(pinInstanceName);
            }

            var subcircuitNodeNameGenerator = new SubcircuitNodeNameGenerator(subcircuitFullName, subcircuitName, subCircuitDefinition, pinInstanceIdentifiers, context.NameGenerator.Globals, context.CaseSensitivity.IsNodeNameCaseSensitive);
            var subcircuitObjectNameGenerator = context.NameGenerator.CreateChildNameGenerator(subcircuitName);
            var subcircuitNameGenerator = new NameGenerator(subcircuitNodeNameGenerator, subcircuitObjectNameGenerator);
            context.NameGenerator.AddChild(subcircuitNodeNameGenerator);
            ComponentInstanceData instanceData = new CustomComponentInstanceData(context.Result.Circuit, subcircuitFullName);

            foreach (var pin in subCircuitDefinition.Pins)
            {
                instanceData.NodeMap[pin] = subcircuitNodeNameGenerator.Generate(pin);
            }

            var subcircuitEvaluationContext = context.CircuitEvaluator.CreateChildContext(subcircuitFullName, true);
            subcircuitEvaluationContext.SetParameters(subcircuitParameters);
            subcircuitEvaluationContext.NameGenerator = subcircuitNameGenerator;

            var subcircuitEvaluator = new CircuitEvaluator(new SimulationEvaluationContexts(subcircuitEvaluationContext), subcircuitEvaluationContext);

            var subcircuitContext = new CircuitContext(
                subcircuitName,
                context,
                subcircuitEvaluator,
                context.SimulationPreparations,
                context.Result,
                subcircuitNameGenerator,
                context.StatementsReader,
                context.WaveformReader,
                context.CaseSensitivity,
                context.Exporters,
                context.WorkingDirectory,
                instanceData);

            return subcircuitContext;
        }

        /// <summary>
        /// Creates subcircuit parameters dictionary.
        /// </summary>
        /// <param name="subCiruitDefiniton">Subcircuit definition.</param>
        /// <param name="subcktParameters">Subcircuit parameters.</param>
        /// <param name="circuitContext">Reading context.</param>
        /// <returns>
        /// A dictionary of parameters.
        /// </returns>
        private Dictionary<string, string> CreateSubcircuitParameters(SubCircuit subCiruitDefiniton, List<AssignmentParameter> subcktParameters, ICircuitContext circuitContext)
        {
            var result = new Dictionary<string, string>(StringComparerProvider.Get(circuitContext.CaseSensitivity.IsParameterNameCaseSensitive));

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
                    result[parameterName] = circuitContext.CircuitEvaluator.EvaluateDouble(parameterName).ToString(CultureInfo.InvariantCulture);
                }
            }

            return result;
        }

        public class CustomComponentInstanceData : ComponentInstanceData
        {
            static CustomComponentInstanceData()
            {
                Utility.Separator = ".";
            }

            public CustomComponentInstanceData(Circuit subcircuit)
                : base(subcircuit)
            {
            }

            public CustomComponentInstanceData(Circuit subckt, string name)
                : base(subckt, name)
            {
            }
        }
    }
}