using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public class SpiceStatementsReader : ISpiceStatementsReader
    {
        public SpiceStatementsReader(
            IMapper<BaseControl> controlMapper,
            IMapper<IModelGenerator> modelMapper,
            IMapper<IComponentGenerator> entityMapper)
        {
            var modelReader = new ModelReader(modelMapper, new StochasticModelsGenerator());
            var componentReader = new ComponentReader(entityMapper);
            var controlReader = new ControlReader(controlMapper);
            var subcircuitDefinitionReader = new SubcircuitDefinitionReader();
            var commentReader = new CommentReader();

            Readers[typeof(Component)] = componentReader;
            Readers[typeof(Model)] = modelReader;
            Readers[typeof(Control)] = controlReader;
            Readers[typeof(SubCircuit)] = subcircuitDefinitionReader;
            Readers[typeof(CommentLine)] = commentReader;
        }

        /// <summary>
        /// Gets the readers.
        /// </summary>
        protected Dictionary<Type, IStatementReader> Readers { get; } = new Dictionary<Type, IStatementReader>();

        /// <summary>
        /// Reads a statement.
        /// </summary>
        /// <param name="statement">A statement.</param>
        /// <param name="circuitContext">A reading context.</param>
        public void Read(Statement statement, ICircuitContext circuitContext)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (circuitContext == null)
            {
                throw new ArgumentNullException(nameof(circuitContext));
            }

            if (Readers.ContainsKey(statement.GetType()))
            {
                try
                {
                    Readers[statement.GetType()].Read(statement, circuitContext);
                }
                catch (Exception e)
                {
                    circuitContext.Result.Validation.Add(
                        new ValidationEntry(
                            ValidationEntrySource.Reader,
                            ValidationEntryLevel.Warning,
                            $"There was a problem during reading statement: {statement.GetType()} : {e}",
                            statement.LineInfo));
                }
            }
            else
            {
                circuitContext.Result.Validation.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"There is no reader for the statement of type: {statement.GetType()}",
                        statement.LineInfo));
            }
        }
    }
}