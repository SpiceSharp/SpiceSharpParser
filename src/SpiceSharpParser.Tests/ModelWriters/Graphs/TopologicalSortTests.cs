using System.Linq;
using SpiceSharpParser.Common.Mathematics.Graphs;
using Xunit;

namespace SpiceSharpParser.Tests.ModelWriters.Graphs
{
    public class TopologicalSortTests
    {
        [Fact]
        public void Test01()
        {
            var graph = new Graph<int>();
            graph.Nodes.Add(1);
            graph.Nodes.Add(2);
            graph.Nodes.Add(3);
            graph.Nodes.Add(4);

            graph.Edges.Add(new Edge<int>(1, 2));
            graph.Edges.Add(new Edge<int>(4, 1));
            graph.Edges.Add(new Edge<int>(2, 3));

            var tSort = new TopologicalSort();
            var sorted = tSort.GetSorted<int>(graph);
            var sortedList = sorted.ToList();
            Assert.NotNull(sortedList);
        }
    }
}
