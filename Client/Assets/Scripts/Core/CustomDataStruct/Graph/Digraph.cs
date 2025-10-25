using System;
using System.Collections.Generic;

namespace Core.CustomDataStruct.Graph
{
    public class Digraph
    {
        public List<DigraphNode> Nodes { get; set; }

        public Digraph()
        {
            Nodes = new List<DigraphNode>();
        }

        // 添加节点
        public void AddNode(int value)
        {
            Nodes.Add(new DigraphNode(value));
        }

        // 添加有向边
        public void AddDirectedEdge(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= Nodes.Count || toIndex < 0 || toIndex >= Nodes.Count)
                throw new ArgumentOutOfRangeException();

            Nodes[fromIndex].OutNeighbors.Add(Nodes[toIndex]);
        }
        
        // 判断有向图是否是弱连通的
        public bool IsWeaklyConnected()
        {
            if (Nodes.Count == 0) return true;

            // 创建一个无向图来测试连通性
            Graph undirectedGraph = new Graph();
        
            // 添加所有节点
            foreach (var node in Nodes)
            {
                undirectedGraph.AddNode(node.Value);
            }

            // 添加无向边（双向）
            for (int i = 0; i < Nodes.Count; i++)
            {
                foreach (var neighbor in Nodes[i].OutNeighbors)
                {
                    int j = Nodes.IndexOf(neighbor);
                    undirectedGraph.AddUndirectedEdge(i, j);
                }
            }

            return undirectedGraph.IsConnectedDFS();
        }
        
        // 使用Kosaraju算法判断强连通性
        public bool IsStronglyConnected()
        {
            if (Nodes.Count == 0) return true;

            // 第一次DFS遍历
            ResetVisited();
            DFS(Nodes[0]);

            // 检查是否所有节点都被访问
            foreach (var node in Nodes)
            {
                if (!node.Visited)
                    return false;
            }

            // 反转图
            Digraph reversedGraph = ReverseGraph();

            // 重置访问状态
            reversedGraph.ResetVisited();

            // 在反转图上进行DFS
            reversedGraph.DFS(reversedGraph.Nodes[Nodes.IndexOf(Nodes[0])]);

            // 检查反转图是否所有节点都被访问
            foreach (var node in reversedGraph.Nodes)
            {
                if (!node.Visited)
                    return false;
            }

            return true;
        }

        // 反转有向图
        private Digraph ReverseGraph()
        {
            Digraph reversed = new Digraph();

            // 添加所有节点
            foreach (var node in Nodes)
            {
                reversed.AddNode(node.Value);
            }

            // 反转边
            for (int i = 0; i < Nodes.Count; i++)
            {
                foreach (var neighbor in Nodes[i].OutNeighbors)
                {
                    int j = Nodes.IndexOf(neighbor);
                    reversed.AddDirectedEdge(j, i);
                }
            }

            return reversed;
        }

        // DFS实现（用于强连通性判断）
        private void DFS(DigraphNode node)
        {
            if (node == null || node.Visited) return;

            node.Visited = true;
            foreach (var neighbor in node.OutNeighbors)
            {
                DFS(neighbor);
            }
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