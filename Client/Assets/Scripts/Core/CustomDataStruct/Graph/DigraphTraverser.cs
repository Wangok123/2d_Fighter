using System;
using System.Collections.Generic;

namespace Core.CustomDataStruct.Graph
{
    public class DigraphTraverser
    {
        // 计算两个节点之间的路径值，现在是1和0代表有无
        private static int GetPathValue(DigraphNode from, DigraphNode to)
        {
            return (from.Value == 1 && to.Value == 1) ? 1 : 0;
        }

        // 深度优先遍历
        public static void DFS(DigraphNode node, Action<DigraphNode, int> visitAction, int pathValue = 0)
        {
            if (node == null || node.Visited) return;

            node.Visited = true;
            visitAction?.Invoke(node, pathValue);

            foreach (var neighbor in node.OutNeighbors)
            {
                int newPathValue = GetPathValue(node, neighbor);
                DFS(neighbor, visitAction, newPathValue);
            }
        }

        // 广度优先遍历
        public static void BFS(DigraphNode startNode, Action<DigraphNode, int> visitAction)
        {
            if (startNode == null) return;

            Queue<Tuple<DigraphNode, int>> queue = new Queue<Tuple<DigraphNode, int>>();
            startNode.Visited = true;
            queue.Enqueue(Tuple.Create(startNode, 0));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                DigraphNode currentNode = current.Item1;
                int currentPathValue = current.Item2;

                visitAction?.Invoke(currentNode, currentPathValue);

                foreach (var neighbor in currentNode.OutNeighbors)
                {
                    if (!neighbor.Visited)
                    {
                        neighbor.Visited = true;
                        int newPathValue = GetPathValue(currentNode, neighbor);
                        queue.Enqueue(Tuple.Create(neighbor, newPathValue));
                    }
                }
            }
        }
    }
}