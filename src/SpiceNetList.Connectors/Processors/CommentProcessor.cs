using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class CommentProcessor : StatementProcessor<CommentLine>
    {
        public CommentProcessor()
        {
        }

        public override void Process(CommentLine statement, ProcessingContext context)
        {
            context.AddComment(statement);
        }
    }
}
