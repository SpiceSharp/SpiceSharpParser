using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reades all <see cref="CommentLine"/> from spice netlist object model.
    /// </summary>
    public class CommentReader : StatementReader<CommentLine>
    {
        public CommentReader()
        {
        }

        /// <summary>
        /// Returns whether reader can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the reader can process given statement.
        /// </returns>
        public override bool CanRead(Statement statement)
        {
            return statement is CommentLine;
        }

        /// <summary>
        /// Reades a comment statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public override void Read(CommentLine statement, IReadingContext context)
        {
            context.Result.AddComment(statement);
        }
    }
}
