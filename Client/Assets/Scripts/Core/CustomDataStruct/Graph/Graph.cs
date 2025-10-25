using System;
using System.Collections.Generic;

namespace Core.CustomDataStruct.Graph
{
    /// <summary>
    /// 无向图
    /// </summary>
    public class Graph
    {
        public List<GraphNode> Nodes { get; set; }

        public Graph()
        {
            Nodes = new List<GraphNode>();
        }

        // 添加节点
        public void AddNode(int value)
        {
            Nodes.Add(new GraphNode(value));
        }
        
        // 添加边（无向图）
        public void AddUndirectedEdge(int index1, int index2)
        {
            if (index1 < 0 || index1 >= Nodes.Count || index2 < 0 || index2 >= Nodes.Count)
                throw new ArgumentOutOfRangeException();

            Nodes[index1].Neighbors.Add(Nodes[index2]);
            Nodes[index2].Neighbors.Add(Nodes[index1]);
        }
        
        // 判断图是否连通 - DFS方法
        public bool IsConnectedDFS()
        {
            if (Nodes.Count == 0) return true; // 空图视为连通

            ResetVisited();
            DFS(Nodes[0]);

            // 检查是否所有节点都被访问过
            foreach (var node in Nodes)
            {
                if (!node.Visited)
                    return false;
            }
            return true;
        }

        // 判断图是否连通 - BFS方法
        public bool IsConnectedBFS()
        {
            if (Nodes.Count == 0) return true; // 空图视为连通

            ResetVisited();
            BFS(Nodes[0]);

            // 检查是否所有节点都被访问过
            foreach (var node in Nodes)
            {
                if (!node.Visited)
                    return false;
            }
            return true;
        }

        // DFS实现
        private void DFS(GraphNode node)
        {
            if (node == null || node.Visited) return;

            node.Visited = true;
            foreach (var neighbor in node.Neighbors)
            {
                DFS(neighbor);
            }
        }

        // BFS实现
        private void BFS(GraphNode startNode)
        {
            if (startNode == null) return;

            Queue<GraphNode> queue = new Queue<GraphNode>();
            startNode.Visited = true;
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();
                foreach (var neighbor in currentNode.Neighbors)
                {
                    if (!neighbor.Visited)
                    {
                        neighbor.Visited = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
        
        // 使用并查集判断连通性
        public bool IsConnectedUnionFind()
        {
            if (Nodes.Count == 0) return true;

            UnionFind uf = new UnionFind(Nodes.Count);
        
            // 遍历所有边，合并相连的节点
            foreach (var node in Nodes)
            {
                int u = Nodes.IndexOf(node);
                foreach (var neighbor in node.Neighbors)
                {
                    int v = Nodes.IndexOf(neighbor);
                    uf.Union(u, v);
                }
            }

            // 检查是否所有节点都在同一个集合中
            int root = uf.Find(0);
            for (int i = 1; i < Nodes.Count; i++)
            {
                if (uf.Find(i) != root)
                    return false;
            }
            return true;
        }

        // 重置所有节点的访问状态
        public void ResetVisited()
        {
            foreach (var node in Nodes)
            {
                node.Visited = false;
            }
        }
    }
}