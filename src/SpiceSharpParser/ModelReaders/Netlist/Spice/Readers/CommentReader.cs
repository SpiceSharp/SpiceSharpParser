using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all <see cref="CommentLine"/> from SPICE netlist object model.
    /// </summary>
    public class CommentReader : StatementReader<CommentLine>, ICommentReader
    {
        public CommentReader()
        {
        }

        /// <summary>
        /// Reads a comment statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(CommentLine statement, IReadingContext context)
        {
            context.Result.AddComment(statement);
        }
    }
}
