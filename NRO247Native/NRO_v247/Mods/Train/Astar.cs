using System;
using System.Collections.Generic;

namespace NRO_v247.Mods
{
    internal static class Astar
    {
        private static int Heuristic(int x1, int y1, int x2, int y2)
        {
            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);
            return 10 * (dx + dy) - 6 * Math.Min(dx, dy);
        }

        private static int TieBreak(int x, int y, int gx, int gy, int sx, int sy)
        {
            int dx1 = x - gx;
            int dy1 = y - gy;
            int dx2 = sx - gx;
            int dy2 = sy - gy;
            return Math.Abs(dx1 * dy2 - dx2 * dy1) >> 3;
        }

        private static int Key(int x, int y)
        {
            return x * TileMap.tmh + y;
        }

        public static List<Point> FindPath(Point start, Point goal)
        {
            start = FindNearestWalkable(start.x, start.y);
            goal = FindNearestWalkable(goal.x, goal.y);
            if (start == null || goal == null) return null;

            int sx = start.x, sy = start.y;
            int gx = goal.x, gy = goal.y;

            var openList = new MinHeap();
            var closedSet = new Dictionary<int, Point>();
            var openMap = new Dictionary<int, Point>();

            var startNode = new Point(sx, sy);
            startNode.gCost = 0;
            startNode.hCost = Heuristic(sx, sy, gx, gy);
            openList.Add(startNode);
            openMap[Key(sx, sy)] = startNode;

            int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };
            int[] dy = { -1, 0, 1, 0, -1, 1, 1, -1 };
            int[] costs = { 10, 10, 10, 10, 14, 14, 14, 14 };

            while (!openList.IsEmpty())
            {
                Point current = openList.Poll();
                int ck = Key(current.x, current.y);
                openMap.Remove(ck);

                if (current.x == gx && current.y == gy)
                    return ReconstructPath(current);

                closedSet[ck] = current;

                for (int i = 0; i < 8; i++)
                {
                    int nx = current.x + dx[i];
                    int ny = current.y + dy[i];

                    if (nx < 0 || ny < 0 || nx >= TileMap.tmw || ny >= TileMap.tmh) continue;
                    if (!CanWalk(nx, ny)) continue;

                    if (i >= 4)
                    {
                        if (!CanWalk(current.x + dx[i], current.y)) continue;
                        if (!CanWalk(current.x, current.y + dy[i])) continue;
                    }

                    int nk = Key(nx, ny);
                    if (closedSet.ContainsKey(nk)) continue;

                    int newG = current.gCost + costs[i];

                    if (!openMap.TryGetValue(nk, out Point existing))
                    {
                        var node = new Point(nx, ny);
                        node.gCost = newG;
                        node.hCost = Heuristic(nx, ny, gx, gy) + TieBreak(nx, ny, gx, gy, sx, sy);
                        node.parent = current;
                        openList.Add(node);
                        openMap[nk] = node;
                    }
                    else if (newG < existing.gCost)
                    {
                        existing.gCost = newG;
                        existing.parent = current;
                        openList.Update(existing);
                    }
                }
            }

            return null;
        }

        private static List<Point> ReconstructPath(Point end)
        {
            var path = new List<Point>();
            for (Point n = end; n != null; n = n.parent)
                path.Insert(0, new Point(n.x, n.y));
            return path;
        }

        private static bool CanWalk(int x, int y)
        {
            if (x < 0 || y < 0 || x >= TileMap.tmw || y >= TileMap.tmh) return false;
            if (TileMap.tileTypeAt(x, y) != 0) return false;
            if (y > 0 && TileMap.tileTypeAt(x, y - 1) != 0) return false;
            return true;
        }

        private static Point FindNearestWalkable(int x, int y)
        {
            if (CanWalk(x, y)) return new Point(x, y);
            for (int r = 1; r <= 5; r++)
            {
                for (int ox = -r; ox <= r; ox++)
                {
                    for (int oy = -r; oy <= r; oy++)
                    {
                        if (Math.Abs(ox) == r || Math.Abs(oy) == r)
                        {
                            if (CanWalk(x + ox, y + oy))
                                return new Point(x + ox, y + oy);
                        }
                    }
                }
            }
            return null;
        }

        internal sealed class Point
        {
            public int x, y;
            public int gCost, hCost;
            public Point parent;
            internal int heapIndex = -1;

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public int FCost() => gCost + hCost;

            public override bool Equals(object obj)
            {
                if (obj is Point o) return x == o.x && y == o.y;
                return false;
            }

            public override int GetHashCode() => Key(x, y);
        }

        private sealed class MinHeap
        {
            private readonly List<Point> _items = new List<Point>();

            public void Add(Point p)
            {
                p.heapIndex = _items.Count;
                _items.Add(p);
                SiftUp(p.heapIndex);
            }

            public Point Poll()
            {
                if (_items.Count == 0) return null;
                Point top = _items[0];
                int last = _items.Count - 1;
                Point tail = _items[last];
                _items.RemoveAt(last);
                if (_items.Count > 0)
                {
                    tail.heapIndex = 0;
                    _items[0] = tail;
                    SiftDown(0);
                }
                top.heapIndex = -1;
                return top;
            }

            public void Update(Point p)
            {
                if (p.heapIndex >= 0 && p.heapIndex < _items.Count)
                    SiftUp(p.heapIndex);
            }

            public bool IsEmpty() => _items.Count == 0;

            private void SiftUp(int i)
            {
                while (i > 0)
                {
                    int parent = (i - 1) / 2;
                    if (Cmp(_items[i], _items[parent]) < 0)
                    {
                        Swap(i, parent);
                        i = parent;
                    }
                    else break;
                }
            }

            private void SiftDown(int i)
            {
                int size = _items.Count;
                while (true)
                {
                    int l = 2 * i + 1, r = 2 * i + 2, best = i;
                    if (l < size && Cmp(_items[l], _items[best]) < 0) best = l;
                    if (r < size && Cmp(_items[r], _items[best]) < 0) best = r;
                    if (best == i) break;
                    Swap(i, best);
                    i = best;
                }
            }

            private void Swap(int a, int b)
            {
                Point pa = _items[a], pb = _items[b];
                pa.heapIndex = b;
                pb.heapIndex = a;
                _items[a] = pb;
                _items[b] = pa;
            }

            private static int Cmp(Point a, Point b)
            {
                int fa = a.FCost(), fb = b.FCost();
                if (fa != fb) return fa - fb;
                return a.hCost - b.hCost;
            }
        }
    }
}