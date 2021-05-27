using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;

namespace SpiceSharpParser.Common.Mathematics.Graphs
{
    public class TopologicalSort
    {
        public IEnumerable<T> GetSorted<T>(Graph<T> graph)
            where T : IEquatable<T>
        {
            if (graph is null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            var copy = graph.Clone();
            var l = new List<T>();
            var s = copy.Nodes.Where(node => !graph.Edges.Any(edge => edge.To.Equals(node))).ToList();

            while (s.Any())
            {
                var n = s.First();
                s.Remove(n);
                l.Add(n);

                var mEdges = graph.Edges.Where(edge => edge.From.Equals(n));
                copy.Edges.RemoveWhere(edge => mEdges.Contains(edge));
                var toNodes = mEdges.Select(edge => edge.To);
                s.AddRange(toNodes.Where(toNode => !copy.Edges.Any(edge => edge.To.Equals(toNode))));
            }

            if (copy.Edges.Count != 0)
            {
                throw new SpiceSharpException("Cyclic graph...");
            }

            return l;
        }
    }
}
