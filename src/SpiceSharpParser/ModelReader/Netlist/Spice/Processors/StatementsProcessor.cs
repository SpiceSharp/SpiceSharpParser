using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
{
    /// <summary>
    /// Processes <see cref="Statement"/>s from spice netlist object model.
    /// </summary>
    public class StatementsProcessor : IStatementsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatementsProcessor"/> class.
        /// </summary>
        public StatementsProcessor(
            IStatementProcessor[] processors,
            IRegistry[] registries,
            IStatementsOrderer orderer)
        {
            Registries = registries;
            Processors = processors;
            Orderer = orderer;
        }

        /// <summary>
        /// Gets the orderer
        /// </summary>
        protected IStatementsOrderer Orderer { get; }

        /// <summary>
        /// Gets the processors
        /// </summary>
        protected IStatementProcessor[] Processors { get; }

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
        /// Processes statemets and modifes the context.
        /// </summary>
        /// <param name="statements">The statements to process.</param>
        /// <param name="context">The context to modify.</param>
        public void Process(Statements statements, IProcessingContext context)
        {
            foreach (Statement statement in Orderer.Order(statements))
            {
                var processor = GetProcessor(statement);
                if (processor != null)
                {
                    processor.Process(statement, context);
                }
            }
        }

        private IStatementProcessor GetProcessor(Statement statement)
        {
            for (var i = 0; i < Processors.Length; i++)
            {
                if (Processors[i].CanProcess(statement))
                {
                    return Processors[i];
                }
            }

            return null;
        }
    }
}
