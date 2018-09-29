using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Processors;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    /// <summary>
    /// Generates subcircuits content.
    /// </summary>
    public class SubCircuitGenerator : ComponentGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubCircuitGenerator"/> class.
        /// </summary>
        public SubCircuitGenerator()
        {
        }

        public override SpiceSharp.Components.Component Generate(Identifier componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            SubCircuit subCircuitDefiniton = FindSubcircuitDefinion(parameters, context);
            ReadingContext subCircuitContext = CreateSubcircuitContext(componentIdentifier.ToString(), originalName, subCircuitDefiniton, parameters, context);

            var ifPreprocessor = new IfPreprocessor();
            ifPreprocessor.Evaluators = subCircuitContext.Evaluators;
            subCircuitDefiniton.Statements = ifPreprocessor.Process(subCircuitDefiniton.Statements);

            ReadParamControl(subCircuitDefiniton, subCircuitContext);
            ReadSubcircuits(subCircuitDefiniton, subCircuitContext);
            CreateSubcircuitModels(subCircuitDefiniton, subCircuitContext); // TODO: Share models someday between instances of subcircuits
            CreateSubcircuitComponents(subCircuitDefiniton, subCircuitContext);

            context.Children.Add(subCircuitContext);

            // null is intentional
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
                return new List<string>() { "x" };
            }
        }

        /// <summary>
        /// Read .param controls.
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit reading context</param>
        private void ReadParamControl(SubCircuit subCircuitDefiniton, IReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Control && ((Control)s).Name.ToLower() == "param"))
            {
                subCircuitContext.StatementsReader.Read(statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Reads .subckt statement.
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit reading context</param>
        private void ReadSubcircuits(SubCircuit subCircuitDefiniton, IReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is SubCircuit))
            {
                subCircuitContext.StatementsReader.Read((SubCircuit)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Creates components for subcircuit.
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit reading context</param>
        private void CreateSubcircuitComponents(SubCircuit subCircuitDefiniton, IReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Component))
            {
                subCircuitContext.StatementsReader.Read((Component)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Creates models for subcircuit.
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion.</param>
        /// <param name="subCircuitContext">A subcircuit reading context.</param>
        private void CreateSubcircuitModels(SubCircuit subCircuitDefiniton, IReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Model))
            {
                subCircuitContext.StatementsReader.Read(statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Finds subcircuit definion.
        /// </summary>
        /// <param name="parameters">Paramters of subcircuit instance.</param>
        /// <param name="context">A reading context.</param>
        /// <returns>
        /// A reference to subcircuit.
        /// </returns>
        private SubCircuit FindSubcircuitDefinion(ParameterCollection parameters, IReadingContext context)
        {
            // first step is to find subcircuit name in parameters, a=b paramters needs to be skipped
            int assigmentParametersCount = 0;
            while (parameters[parameters.Count - assigmentParametersCount - 1] is AssignmentParameter a)
            {
                assigmentParametersCount++;
            }

            string subCircuitDefinionName = parameters.GetString(parameters.Count - assigmentParametersCount - 1);
            var result = context.AvailableSubcircuits.ToList().Find(pred => pred.Name == subCircuitDefinionName);

            if (result == null)
            {
                throw new GeneralReaderException("Could't find " + subCircuitDefinionName + " subcircuit");
            }

            return result;
        }

        /// <summary>
        /// Creates subcircuit context.
        /// </summary>
        /// <param name="name">Subcircuit full name.</param>
        /// <param name="subCircuitDefiniton">Subcircuit definion.</param>
        /// <param name="parameters">Parameters and pins for subcircuit.</param>
        /// <param name="context">Parent reading context.</param>
        /// <returns>
        /// A new instance of reading context.
        /// </returns>
        private ReadingContext CreateSubcircuitContext(string subcircuitFullName, string subcircuitName, SubCircuit subCircuitDefiniton, ParameterCollection parameters, IReadingContext context)
        {
            int assigmentParametersCount = 0;

            // setting evaluator
            var subCktParameters = new List<AssignmentParameter>();
            while (parameters[parameters.Count - assigmentParametersCount - 1] is AssignmentParameter a)
            {
                subCktParameters.Add(a);
                assigmentParametersCount++;
            }

            var subcircuitEvaluatorContainer = context.Evaluators.CreateChildContainer(subcircuitFullName);
            var subcircuitParameters = CreateSubcircuitParameters(context.Evaluators, subCircuitDefiniton, subCktParameters);
            subcircuitEvaluatorContainer.SetParameters(subcircuitParameters);

            // setting node name generator
            var pinInstanceNames = new List<string>();
            for (var i = 0; i < parameters.Count - assigmentParametersCount - 1; i++)
            {
                var pinInstanceName = context.NodeNameGenerator.Generate(parameters.GetString(i));

                pinInstanceNames.Add(pinInstanceName);
            }

            var subcircuitNodeNameGenerator = new SubcircuitNodeNameGenerator(subcircuitFullName, subcircuitName, subCircuitDefiniton, pinInstanceNames, context.NodeNameGenerator.Globals, context.CaseSensitivity.IgnoreCaseForNodes);
            context.NodeNameGenerator.Children.Add(subcircuitNodeNameGenerator);

            // setting object name generator
            var subcircuitObjectNameGenerator = context.ObjectNameGenerator.CreateChildGenerator(subcircuitName);

            return new ReadingContext(subcircuitName, context.SimulationsParameters, subcircuitEvaluatorContainer, context.Result, subcircuitNodeNameGenerator, subcircuitObjectNameGenerator, context.StatementsReader, context.WaveformReader, context.CaseSensitivity, context);
        }

        /// <summary>
        /// Creates subcircuit parameters dictionary.
        /// </summary>
        /// <param name="subCiruitDefiniton">Subcircuit definion.</param>
        /// <param name="subcktParameters">Subcircuit parameters.</param>
        /// <returns>
        /// A dictionary of parameters.
        /// </returns>
        private Dictionary<string, string> CreateSubcircuitParameters(IEvaluatorsContainer parentContainer, SubCircuit subCiruitDefiniton, List<AssignmentParameter> subcktParameters)
        {
            var result = new Dictionary<string, string>();
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
                    result[parameterName] = parentContainer.EvaluateDouble(parameterName).ToString(CultureInfo.InvariantCulture);
                }
            }

            return result;
        }
    }
}