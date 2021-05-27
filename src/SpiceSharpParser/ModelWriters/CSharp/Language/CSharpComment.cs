namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class CSharpComment : CSharpStatement
    {
        public CSharpComment(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
