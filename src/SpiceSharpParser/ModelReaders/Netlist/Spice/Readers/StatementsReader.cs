using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads <see cref="Statement"/>s from SPICE netlist object model.
    /// </summary>
    public class StatementsReader : IStatementsReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatementsReader"/> class.
        /// </summary>
        public StatementsReader(
            IStatementReader[] readers,
            IRegistry[] registries,
            IStatementsOrderer orderer)
        {
            Registries = registries;
            Readers = readers;
            Orderer = orderer;
        }

        /// <summary>
        /// Gets the orderer
        /// </summary>
        protected IStatementsOrderer Orderer { get; }

        /// <summary>
        /// Gets the readers
        /// </summary>
        protected IStatementReader[] Readers { get; }

        /// <summary>
        /// Gets the registries
        /// </summary>
        protected IRegistry[] Registries { get; }

        /// <summary>
        /// Gets the registry of given type
        /// </summary>
        /// <typeparam name="T">Type of registry</typeparam>
        /// <returns>
        /// A registry
        /// </returns>
        public T GetRegistry<T>()
        {
            for (var i = 0; i < Registries.Length; i++)
            {
                if (Registries[i] is T)
                {
                    return (T)Registries[i];
                }
            }

            return default(T);
        }

        /// <summary>
        /// Reads statemets and modifes the context.
        /// </summary>
        /// <param name="statements">The statements to process.</param>
        /// <param name="context">The context to modify.</param>
        public void Read(Statements statements, IReadingContext context)
        {
            foreach (Statement statement in Orderer.Order(statements))
            {
                var reader = GetReader(statement);
                if (reader != null)
                {
                    reader.Read(statement, context);
                }
            }
        }

        private IStatementReader GetReader(Statement statement)
        {
            for (var i = 0; i < Readers.Length; i++)
            {
                if (Readers[i].CanRead(statement))
                {
                    return Readers[i];
                }
            }

            return null;
        }
    }
}
