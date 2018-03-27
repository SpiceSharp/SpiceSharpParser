using System;
using System.Collections.Generic;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Evaluation;
using SpiceNetlist.SpiceSharpConnector.Exceptions;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    /// <summary>
    /// Generates subcircuits
    /// </summary>
    public class SubCircuitGenerator : EntityGenerator
    {
        private IComponentProcessor componentProcessor;
        private IModelProcessor modelProcessor;
        private IControlProcessor controlProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubCircuitGenerator"/> class.
        /// </summary>
        /// <param name="componentProcessor">A component processor</param>
        /// <param name="modelProcessor">A model processor</param>
        /// <param name="controlProcessor">A control processor</param>
        public SubCircuitGenerator(IComponentProcessor componentProcessor, IModelProcessor modelProcessor, IControlProcessor controlProcessor)
        {
            this.controlProcessor = controlProcessor;
            this.componentProcessor = componentProcessor;
            this.modelProcessor = modelProcessor;
        }

        /// <summary>
        /// Generates a new subcircuit
        /// </summary>
        /// <param name="id">Identifier for subcircuit (ignored)</param>
        /// <param name="originalName">Name of subcircuit</param>
        /// <param name="type">Type (ignored)</param>
        /// <param name="parameters">Parameters of subcircuit</param>
        /// <param name="context">Processing context</param>
        /// <returns>
        /// A new instance of subcircuit
        /// </returns>
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IProcessingContext context)
        {
            SubCircuit subCircuitDefiniton = FindSubcircuitDefinion(parameters, context);
            ProcessingContext subCircuitContext = CreateSubcircuitContext(originalName, subCircuitDefiniton, parameters, context);

            CreateSubcircuitModels(subCircuitDefiniton, subCircuitContext); // TODO: Share models someday between instances of subcircuits
            CreateSubcircuitComponents(subCircuitDefiniton, subCircuitContext);
            ProcessParamControl(subCircuitDefiniton, subCircuitContext);

            context.Children.Add(subCircuitContext);

            // TODO: null is intentional
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
            return new List<string>() { "x" };
        }

        /// <summary>
        /// Process .param controls
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit processing context</param>
        private void ProcessParamControl(SubCircuit subCircuitDefiniton, ProcessingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Control && ((Control)s).Name.ToLower() == "param"))
            {
                controlProcessor.Process((Control)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Create components for subcircuit
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit processing context</param>
        private void CreateSubcircuitComponents(SubCircuit subCircuitDefiniton, ProcessingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Component))
            {
                componentProcessor.Process((Component)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Create models for subcircuit
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit processing context</param>
        private void CreateSubcircuitModels(SubCircuit subCircuitDefiniton, ProcessingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Model))
            {
                modelProcessor.Process((Model)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Finds subcircuit definion
        /// </summary>
        /// <param name="parameters">Paramters of subcircuit instance</param>
        /// <param name="context">A processing context</param>
        /// <returns>
        /// A reference to subcircuit
        /// </returns>
        private SubCircuit FindSubcircuitDefinion(ParameterCollection parameters, IProcessingContext context)
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
                throw new GeneralConnectorException("Could't find " + subCircuitDefinionName + " subcircuit");
            }

            return result;
        }

        /// <summary>
        /// Creates subcircuit context
        /// </summary>
        /// <param name="subcircuitName">Subcircuit name</param>
        /// <param name="subCircuitDefiniton">Subcircuit definion</param>
        /// <param name="parameters">Parameters and pins for subcircuit</param>
        /// <param name="context">Parent processing context</param>
        /// <returns>
        /// A new instance of processing
        /// </returns>
        private ProcessingContext CreateSubcircuitContext(string subcircuitName, SubCircuit subCircuitDefiniton, ParameterCollection parameters, IProcessingContext context)
        {
            int assigmentParametersCount = 0;

            var subCktParameters = new List<AssignmentParameter>();
            while (parameters[parameters.Count - assigmentParametersCount - 1] is AssignmentParameter a)
            {
                subCktParameters.Add(a);
                assigmentParametersCount++;
            }

            var pinInstanceNames = new List<string>();
            for (var i = 0; i < parameters.Count - assigmentParametersCount - 1; i++)
            {
                pinInstanceNames.Add(parameters.GetString(i));
            }

            var newEvaluator = new Evaluator(context.Evaluator);
            newEvaluator.SetParameters(CreateSubcircuitParameters(context, subCircuitDefiniton, subCktParameters));

            var subcircuitNodeNameGenerator = new NodeNameGenerator(subcircuitName, subCircuitDefiniton, pinInstanceNames);
            var subcircuitObjectNameGenerator = context.ObjectNameGenerator.CreateChildGenerator(subcircuitName);

            return new ProcessingContext(subcircuitName, newEvaluator, context.Result, subcircuitNodeNameGenerator, subcircuitObjectNameGenerator, context);
        }

        /// <summary>
        /// Creates subcircuit parameters dictionary
        /// </summary>
        /// <param name="context">Subcircuit context</param>
        /// <param name="subCiruitDefiniton">Subcircuit definion</param>
        /// <param name="subcktParameters">Subcircuit parameters</param>
        /// <returns>
        /// A dictionary of parameters
        /// </returns>
        private Dictionary<string, double> CreateSubcircuitParameters(IProcessingContext context, SubCircuit subCiruitDefiniton, List<AssignmentParameter> subcktParameters)
        {
            var subcircuitParameters = new Dictionary<string, double>();
            foreach (var defaultParameter in subCiruitDefiniton.DefaultParameters)
            {
                subcircuitParameters[defaultParameter.Name] = context.ParseDouble(defaultParameter.Value);
            }

            foreach (var instanceParameter in subcktParameters)
            {
                subcircuitParameters[instanceParameter.Name] = context.ParseDouble(instanceParameter.Value);
            }

            return subcircuitParameters;
        }
    }
}