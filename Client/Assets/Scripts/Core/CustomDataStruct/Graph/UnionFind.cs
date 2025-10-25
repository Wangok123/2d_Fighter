namespace Core.CustomDataStruct.Graph
{
    public class UnionFind
    {
        private int[] parent;
        private int[] rank;

        public UnionFind(int size)
        {
            parent = new int[size];
            rank = new int[size];
            for (int i = 0; i < size; i++)
            {
                parent[i] = i;
                rank[i] = 0;
            }
        }

        public int Find(int x)
        {
            if (parent[x] != x)
                parent[x] = Find(parent[x]); // 路径压缩
            return parent[x];
        }

        public void Union(int x, int y)
        {
            int rootX = Find(x);
            int rootY = Find(y);
        
            if (rootX == rootY) return;
        
            // 按秩合并
            if (rank[rootX] > rank[rootY])
            {
                parent[rootY] = rootX;
            }
            else
            {
                parent[rootX] = rootY;
                if (rank[rootX] == rank[rootY])
                    rank[rootY]++;
            }
        }
    }
}