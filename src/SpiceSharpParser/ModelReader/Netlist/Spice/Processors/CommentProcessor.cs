using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.Model.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
{
    /// <summary>
    /// Processes all <see cref="CommentLine"/> from spice netlist object model.
    /// </summary>
    public class CommentProcessor : StatementProcessor<CommentLine>
    {
        public CommentProcessor()
        {
        }

        /// <summary>
        /// Processes a comment statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public override void Process(CommentLine statement, IProcessingContext context)
        {
            context.Result.AddComment(statement);
        }
    }
}
