using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharpParser.Postprocessors;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers.EntityGenerators.Components
{
    /// <summary>
    /// Generates subcircuits
    /// </summary>
    public class SubCircuitGenerator : EntityGenerator
    {
        private IComponentReader componentReader;
        private IModelReader modelReader;
        private IControlReader controlReader;
        private ISubcircuitDefinitionReader subcircuitDefinitionReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubCircuitGenerator"/> class.
        /// </summary>
        /// <param name="componentReader">A component reader</param>
        /// <param name="modelReader">A model reader</param>
        /// <param name="controlReader">A control reader</param>
        public SubCircuitGenerator(IComponentReader componentReader, IModelReader modelReader, IControlReader controlReader, ISubcircuitDefinitionReader subcircuitDefinitionReader)
        {
            this.subcircuitDefinitionReader = subcircuitDefinitionReader;
            this.controlReader = controlReader;
            this.componentReader = componentReader;
            this.modelReader = modelReader;
        }

        /// <summary>
        /// Generates a new subcircuit
        /// </summary>
        /// <param name="id">Identifier for subcircuit </param> // OMG!!!
        /// <param name="originalName">Name of subcircuit</param>
        /// <param name="type">Type (ignored)</param>
        /// <param name="parameters">Parameters of subcircuit</param>
        /// <param name="context">Reading context</param>
        /// <returns>
        /// A new instance of subcircuit
        /// </returns>
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            SubCircuit subCircuitDefiniton = FindSubcircuitDefinion(parameters, context);
            ReadingContext subCircuitContext = CreateSubcircuitContext(id.ToString(), originalName, subCircuitDefiniton, parameters, context);

            var ifPostprocessor = new IfPostprocessor(subCircuitContext.Evaluator);
            subCircuitDefiniton.Statements = ifPostprocessor.PostProcess(subCircuitDefiniton.Statements);

            ReadParamControl(subCircuitDefiniton, subCircuitContext);
            ReadSubcircuits(subCircuitDefiniton, subCircuitContext);
            CreateSubcircuitModels(subCircuitDefiniton, subCircuitContext); // TODO: Share models someday between instances of subcircuits
            CreateSubcircuitComponents(subCircuitDefiniton, subCircuitContext);

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
        /// Read .param controls
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit processing context</param>
        private void ReadParamControl(SubCircuit subCircuitDefiniton, ReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Control && ((Control)s).Name.ToLower() == "param"))
            {
                controlReader.Read((Control)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Read .subckt
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit processing context</param>
        private void ReadSubcircuits(SubCircuit subCircuitDefiniton, ReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is SubCircuit))
            {
                subcircuitDefinitionReader.Read((SubCircuit)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Create components for subcircuit
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit processing context</param>
        private void CreateSubcircuitComponents(SubCircuit subCircuitDefiniton, ReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Component))
            {
                componentReader.Read((Component)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Create models for subcircuit
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit processing context</param>
        private void CreateSubcircuitModels(SubCircuit subCircuitDefiniton, ReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Model.Netlist.Spice.Objects.Model))
            {
                modelReader.Read((Model.Netlist.Spice.Objects.Model)statement, subCircuitContext);
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
        /// Creates subcircuit context
        /// </summary>
        /// <param name="subcircuitFullName">Subcircuit full name</param>
        /// <param name="subcircuitName">Subcircuit name</param>
        /// <param name="subCircuitDefiniton">Subcircuit definion</param>
        /// <param name="parameters">Parameters and pins for subcircuit</param>
        /// <param name="context">Parent processing context</param>
        /// <returns>
        /// A new instance of processing
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

            var subcircuitEvaluator = context.Evaluator.CreateChildEvaluator();
            subcircuitEvaluator.SetParameters(CreateSubcircuitParameters(context, subCircuitDefiniton, subCktParameters));

            // setting node name generator
            var pinInstanceNames = new List<string>();
            for (var i = 0; i < parameters.Count - assigmentParametersCount - 1; i++)
            {
                pinInstanceNames.Add(context.NodeNameGenerator.Generate(parameters.GetString(i)));
            }
            var subcircuitNodeNameGenerator = new SubcircuitNodeNameGenerator(subcircuitFullName, subcircuitName, subCircuitDefiniton, pinInstanceNames, context.NodeNameGenerator.Globals);
            context.NodeNameGenerator.Children.Add(subcircuitNodeNameGenerator);

            // setting object name generator
            var subcircuitObjectNameGenerator = context.ObjectNameGenerator.CreateChildGenerator(subcircuitName);

            return new ReadingContext(subcircuitName, subcircuitEvaluator, context.Result, subcircuitNodeNameGenerator, subcircuitObjectNameGenerator, context);
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
        private Dictionary<string, double> CreateSubcircuitParameters(IReadingContext context, SubCircuit subCiruitDefiniton, List<AssignmentParameter> subcktParameters)
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