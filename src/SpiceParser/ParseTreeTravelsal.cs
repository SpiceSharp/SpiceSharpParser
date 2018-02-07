using System.Collections.Generic;
using System.Linq;

namespace SpiceParser
{
    public class ParseTreeTravelsal
    {
        public IEnumerable<ParseTreeNode> GetIterativePostOrder(ParseTreeNode currentNode)
        {
            var visitedNodes = new HashSet<ParseTreeNode>();
            var stack = new Stack<ParseTreeNode>();
            stack.Push(currentNode);

            while (stack.TryPeek(out ParseTreeNode node))
            {
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
