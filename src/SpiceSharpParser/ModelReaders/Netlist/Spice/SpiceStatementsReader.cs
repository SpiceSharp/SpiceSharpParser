using System;
using System.Collections.Generic;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public class SpiceStatementsReader : ISpiceStatementsReader
    {
        public SpiceStatementsReader(
            IMapper<BaseControl> controlMapper,
            IMapper<ModelGenerator> modelMapper,
            IMapper<EntityGenerator> entityMapper)
        {
            var modelReader = new ModelReader(modelMapper);
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
        /// <param name="readingContext">A reading context.</param>
        public void Read(Statement statement, IReadingContext readingContext)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (readingContext == null)
            {
                throw new ArgumentNullException(nameof(readingContext));
            }

            if (Readers.ContainsKey(statement.GetType()))
            {
                Readers[statement.GetType()].Read(statement, readingContext);
            }
            else
            {
                throw new Exception("There is no reader for the statement of type: " + statement.GetType());
            }
        }
    }
}
