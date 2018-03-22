using System.Collections.Generic;

namespace SpiceParser.Parsing
{
    /// <summary>
    /// Allows to enumerate parse tree nodes in specific orders
    /// </summary>
    public class ParseTreeTravelsal
    {
        /// <summary>
        /// Returns an enumerable of parse tree nodes in post order
        /// </summary>
        /// <param name="rootNode">
        /// The root node of tree
        /// </param>
        /// <returns>
        /// An enumrable of parse tree nodes in post order
        /// </returns>
        public IEnumerable<ParseTreeNode> GetIterativePostOrder(ParseTreeNode rootNode)
        {
            if (rootNode == null)
            {
                throw new System.ArgumentNullException(nameof(rootNode));
            }

            var visitedNodes = new HashSet<ParseTreeNode>();
            var stack = new Stack<ParseTreeNode>();
            stack.Push(rootNode);

            while (stack.Count > 0)
            {
                var node = stack.Peek();

                if (node is ParseTreeNonTerminalNode nt)
                {
                    if (!visitedNodes.Contains(nt))
                    {
                        int count = nt.Children.Count - 1;

                        for (int i = count; i >= 0; i--)
                        {
                            stack.Push(nt.Children[i]);
                        }

                        visitedNodes.Add(nt);
                    }
                    else
                    {
                        yield return stack.Pop();
                    }
                }
                else
                {
                    yield return stack.Pop();
                }
            }
        }
    }
}
