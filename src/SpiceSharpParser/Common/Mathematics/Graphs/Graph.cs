using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Mathematics.Graphs
{
    public class Graph<TNode>
        where TNode : IEquatable<TNode>
    {
        public HashSet<TNode> Nodes { get; set; } = new HashSet<TNode>();

        public HashSet<Edge<TNode>> Edges { get; set; } = new HashSet<Edge<TNode>>();

        public Graph<TNode> Clone()
        {
            var graph = new Graph<TNode>();
            graph.Edges = new HashSet<Edge<TNode>>(Edges);
            graph.Nodes = new HashSet<TNode>(Nodes);

            return graph;
        }
    }
}
