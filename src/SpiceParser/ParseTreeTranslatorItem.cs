using NLex;
using SpiceNetlist;

namespace SpiceParser
{
    public struct ParseTreeTranslatorItem
    {
        public Token Token { get; set; }

        public SpiceObject SpiceObject { get; set; }

        public ParseTreeNode Node { get; set; }

        public bool IsToken {  get { return Token != null; } }

        public bool IsSpiceObject { get { return SpiceObject != null; } }
    }
}
