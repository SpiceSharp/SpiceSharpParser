using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.SpiceSharpParser.ModelsReaders.Netlist.Spice.Postprocessors;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    /// <summary>
    /// Generates subcircuits content.
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
        /// Generates a new subcircuit.
        /// </summary>
        /// <param name="id">Identifier for subcircuit.</param>
        /// <param name="originalName">Name of subcircuit.</param>
        /// <param name="type">Type (ignored).</param>
        /// <param name="parameters">Parameters of subcircuit.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// Null reference.
        /// </returns>
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            SubCircuit subCircuitDefiniton = FindSubcircuitDefinion(parameters, context);
            ReadingContext subCircuitContext = CreateSubcircuitContext(id.ToString(), originalName, subCircuitDefiniton, parameters, context);

            var ifPostprocessor = new IfPostprocessor(subCircuitContext.ReadingEvaluator);
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
        /// Gets generated Spice types by generator.
        /// </summary>
        /// <returns>
        /// Generated Spice types.
        /// </returns>
        public override IEnumerable<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "x" };
        }

        /// <summary>
        /// Read .param controls.
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit reading context</param>
        private void ReadParamControl(SubCircuit subCircuitDefiniton, ReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Control && ((Control)s).Name.ToLower() == "param"))
            {
                controlReader.Read((Control)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Reads .subckt statement.
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit reading context</param>
        private void ReadSubcircuits(SubCircuit subCircuitDefiniton, ReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is SubCircuit))
            {
                subcircuitDefinitionReader.Read((SubCircuit)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Creates components for subcircuit.
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion</param>
        /// <param name="subCircuitContext">A subcircuit reading context</param>
        private void CreateSubcircuitComponents(SubCircuit subCircuitDefiniton, ReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Component))
            {
                componentReader.Read((Component)statement, subCircuitContext);
            }
        }

        /// <summary>
        /// Creates models for subcircuit.
        /// </summary>
        /// <param name="subCircuitDefiniton">A subcircuit definion.</param>
        /// <param name="subCircuitContext">A subcircuit reading context.</param>
        private void CreateSubcircuitModels(SubCircuit subCircuitDefiniton, ReadingContext subCircuitContext)
        {
            foreach (Statement statement in subCircuitDefiniton.Statements.Where(s => s is Model))
            {
                modelReader.Read((Model)statement, subCircuitContext);
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
        /// <param name="subcircuitFullName">Subcircuit full name.</param>
        /// <param name="subcircuitName">Subcircuit name.</param>
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

            var subcircuitEvaluator = context.ReadingEvaluator.CreateChildEvaluator(subcircuitFullName);
            var subcircuitParameters = CreateSubcircuitParameters(context.ReadingEvaluator, subCircuitDefiniton, subCktParameters);
            subcircuitEvaluator.SetParameters(subcircuitParameters);

            foreach (var parameter in subcircuitParameters.Keys)
            {
                var expression = subcircuitParameters[parameter]; // {X}
                context.ReadingEvaluator.AddAction(subcircuitFullName + " - " + parameter + " - sub", expression,
                    (Simulation sim, double val) =>
                    {
                        var simulationEvaluator = context.GetSimulationEvaluator(sim);
                        var simulationSubcircuitEvaluator = simulationEvaluator.Search(subcircuitFullName);
                        simulationSubcircuitEvaluator.SetParameter(parameter, val, sim);
                    });
            }

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
        /// Creates subcircuit parameters dictionary.
        /// </summary>
        /// <param name="subCiruitDefiniton">Subcircuit definion.</param>
        /// <param name="subcktParameters">Subcircuit parameters.</param>
        /// <returns>
        /// A dictionary of parameters.
        /// </returns>
        private Dictionary<string, string> CreateSubcircuitParameters(IEvaluator parentEvaluator, SubCircuit subCiruitDefiniton, List<AssignmentParameter> subcktParameters)
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
                    result[parameterName] = parentEvaluator.EvaluateDouble(parameterName).ToString(CultureInfo.InvariantCulture);
                }
            }

            return result;
        }
    }
}