using System.Collections.Generic;

namespace Core.CustomDataStruct.Graph
{
    public class DigraphNode
    {
        public int Value { get; set; }
        public List<DigraphNode> OutNeighbors { get; set; } // 出边邻居
        public bool Visited { get; set; }

        public DigraphNode(int value)
        {
            Value = value;
            OutNeighbors = new List<DigraphNode>();
            Visited = false;
        }
    }
}