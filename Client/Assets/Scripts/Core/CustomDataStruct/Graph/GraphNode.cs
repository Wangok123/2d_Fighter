using System.Collections.Generic;

namespace Core.CustomDataStruct.Graph
{
    public class GraphNode
    {
        public int Value { get; set; }
        public List<GraphNode> Neighbors { get; set; }
        public bool Visited { get; set; }

        public GraphNode(int value)
        {
            Value = value;
            Neighbors = new List<GraphNode>();
            Visited = false;
        }
    }
}