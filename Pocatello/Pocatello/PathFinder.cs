using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Pocatello
{
    class PathFinder
    {
        public delegate void AlgoDelegate(Node c);
        public delegate int HeuristicDelegate(int[] n);

        private List<Node> open = new List<Node>();
        private List<Node> closed = new List<Node>();
        private byte[,] obstacles;
        private int[] end;
        private Node pathNode = null;
        private bool done = false;

        private HeuristicDelegate h;

        public Stack<int[]> run(byte[,] obstacles, int[] start, int[] end, AlgoDelegate alg, HeuristicDelegate h, BackgroundWorker bgw)
        {
            this.h = h;
            this.end = end;
            this.obstacles = obstacles;

            for (int i = 0; i < obstacles.GetLength(0); i++)
            {
                obstacles[i, 0] = 1;
                obstacles[i, obstacles.GetLength(1) - 1] = 1;
            }
            for (int i = 0; i < obstacles.GetLength(1); i++)
            {
                obstacles[0, i] = 1;
                obstacles[obstacles.GetLength(0) - 1, i] = 1;
            }

            open.Add(new Node(start, h(start)));
            if (obstacles[open[0].Pos[0], open[0].Pos[1]] == 1 || obstacles[end[0], end[1]] == 1)
            {
                open.RemoveAt(0);
            }
            
            while (open.Count() != 0 && !done)
            {
                bgw.ReportProgress(0, open[0].Pos);
                closed.Add(open[0]);
                open.RemoveAt(0);
                alg(closed[closed.Count() - 1]);
            }

            var pathStack = new Stack<int[]>();
            if (!done)
            {
                pathStack.Push(new int[] { -1, -1 });
            }
            else
            {
                while (pathNode.Parent != pathNode)
                {
                    pathStack.Push(pathNode.Pos);
                    pathNode = pathNode.Parent;
                }
                pathStack.Push(pathNode.Pos);
            }
            return pathStack;
        }

        public void lookForNeighboursJPS(Node c)
        {
            int[] n = c.Pos;
            int[] d = c.D;
            int g = c.G;
            if (Math.Abs(d[0]) + Math.Abs(d[1]) == 1)
            {
                lookForNodeJPS(c, d, g + 10, new int[] { n[0] + d[0], n[1] + d[1] });
                if ((obstacles[n[0] + d[1], n[1] + d[0]] == 1) && (obstacles[n[0] + d[1] + d[0], n[1] + d[0] + d[1]] == 0))
                {
                    lookForNodeJPS(c, new int[] { d[1] + d[0], d[1] + d[0] }, g + 14, new int[] { n[0] + d[0] + d[1], n[1] + d[0] + d[1] });
                }
                if ((obstacles[n[0] - d[1], n[1] - d[0]] == 1) && (obstacles[n[0] - d[1] + d[0], n[1] - d[0] + d[1]] == 0))
                {
                    lookForNodeJPS(c, new int[] { d[0] - d[1], d[1] - d[0] }, g + 14, new int[] { n[0] + d[0] - d[1], n[1] + d[1] - d[0] });
                }
            }
            else if (Math.Abs(d[0]) + Math.Abs(d[1]) == 2)
            {
                lookForNodeJPS(c, d, g + 14, new int[] { n[0] + d[0], n[1] + d[1] });
                if (obstacles[n[0] - d[0], n[1]] == 1 && obstacles[n[0] - d[0], n[1] + d[1]] == 0)
                {
                    lookForNodeJPS(c, new int[] { -d[0], d[1] }, g + 14, new int[] { n[0] - d[0], n[1] + d[1] });
                }
                if (obstacles[n[0], n[1] - d[1]] == 1 && obstacles[n[0] + d[0], n[1] - d[1]] == 0)
                {
                    lookForNodeJPS(c, new int[] { d[0], -d[1] }, g + 14, new int[] { n[0] + d[0], n[1] - d[1] });
                }
            }
            else
            {
                int[] p = c.Pos;
                lookForNodeJPS(c, new int[] { 1, 0 }, 10, new int[] { p[0] + 1, p[1] });
                lookForNodeJPS(c, new int[] { 0, 1 }, 10, new int[] { p[0], p[1] + 1 });
                lookForNodeJPS(c, new int[] { 0, -1 }, 10, new int[] { p[0], p[1] - 1 });
                lookForNodeJPS(c, new int[] { -1, 0 }, 10, new int[] { p[0] - 1, p[1] });
                lookForNodeJPS(c, new int[] { 1, 1 }, 14, new int[] { p[0] + 1, p[1] + 1 });
                lookForNodeJPS(c, new int[] { 1, -1 }, 14, new int[] { p[0] + 1, p[1] - 1 });
                lookForNodeJPS(c, new int[] { -1, 1 }, 14, new int[] { p[0] - 1, p[1] + 1 });
                lookForNodeJPS(c, new int[] { -1, -1 }, 14, new int[] { p[0] - 1, p[1] - 1 });
            }
        }
        
        private bool lookForNodeJPS(Node c, int[] d, int g, int[] n)
        {
            if (!ignore(n) && !done)
            {
                if (n[0] == end[0] && n[1] == end[1])
                {
                    insert(new Node(c, n, g, h(n), d));
                    return true;
                }
                if (Math.Abs(d[0]) + Math.Abs(d[1]) == 1)
                {
                    if ((obstacles[n[0] + d[1], n[1] + d[0]] == 1) && (obstacles[n[0] + d[1] + d[0], n[1] + d[0] + d[1]] == 0))
                    {
                        insert(new Node(c, n, g, h(n), d));
                        return true;
                    }
                    if ((obstacles[n[0] - d[1], n[1] - d[0]] == 1) && (obstacles[n[0] - d[1] + d[0], n[1] - d[0] + d[1]] == 0))
                    {
                        insert(new Node(c, n, g + 10, h(n), d));
                        return true;
                    }
                    return lookForNodeJPS(c, d, g + 10, new int[] { n[0] + d[0], n[1] + d[1] });
                }
                else if (Math.Abs(d[0]) + Math.Abs(d[1]) == 2)
                {
                    if (lookForNodeJPS(new Node(c, n, g, h(n), d), new int[] { d[0], 0 }, g + 10, new int[] { n[0] + d[0], n[1] }))
                    {
                        insert(new Node(c, n, g, h(n), d));
                    }
                    if (lookForNodeJPS(new Node(c, n, g, h(n), d), new int[] { 0, d[1] }, g + 10, new int[] { n[0], n[1] + d[1] }))
                    {
                        insert(new Node(c, n, g, h(n), d));
                    }
                    if (obstacles[n[0] - d[0], n[1]] == 1 && obstacles[n[0] - d[0], n[1] + d[1]] == 0)
                    {
                        insert(new Node(c, n, g, h(n), d));
                        return true;
                    }
                    if (obstacles[n[0], n[1] - d[1]] == 1 && obstacles[n[0] + d[0], n[1] - d[1]] == 0)
                    {
                        insert(new Node(c, n, g, h(n), d));
                        return true;
                    }
                    return lookForNodeJPS(c, d, g + 14, new int[] { n[0] + d[0], n[1] + d[1] });
                }
            }
            return false;
        }

        public void lookForNeighbours(Node c)
        {
            lookForNode(new int[] { 1, 0 }, c, 10);
            lookForNode(new int[] { 0, 1 }, c, 10);
            lookForNode(new int[] { 0, -1 }, c, 10);
            lookForNode(new int[] { -1, 0 }, c, 10);
            lookForNode(new int[] { 1, 1 }, c, 14);
            lookForNode(new int[] { 1, -1 }, c, 14);
            lookForNode(new int[] { -1, 1 }, c, 14);
            lookForNode(new int[] { -1, -1 }, c, 14);
        }

        private void lookForNode(int[] d, Node c, int g)
        {
            int[] n = new int[2] {
                c.Pos[0] + d[0],
                c.Pos[1] + d[1]
            };
            
            if (ignore(n))
            {
                return;
            }
            insert(new Node(c, n, c.G + g, h(n), d));
        }
        
        private bool ignore(int[] n)
        {
            return (obstacles[n[0], n[1]] == 1) || (closed.FindIndex(se => se.Equals(n)) != -1);
        }
        
        private void insert(Node c)
        {
            int[] n = c.Pos;
            int F = c.F;
            int i = open.FindIndex(se => se.Equals(n));
            if (i != -1)
            {
                if (open[i].F > F)
                {
                    open[i].G = c.G;
                    open[i].Parent = c.Parent;
                    open = open.OrderBy(o => o.F).ToList();
                }
            }
            else
            {
                bool inserted = false;
                int max = open.Count();
                for(int j = 0; j < max; j++)
                {
                    if(F < open[j].F)
                    {
                        open.Insert(j, c);
                        inserted = true;
                        break;
                    }
                }
                if (!inserted)
                {
                    open.Add(c);
                }
                if (n[0] == end[0] && n[1] == end[1])
                {
                    pathNode = c;
                    done = true;
                }
            }
        }
        public int HManhattan(int[] n)
        {
            int dx = Math.Abs(n[0] - end[0]);
            int dy = Math.Abs(n[1] - end[1]);
            return (dx + dy) * 10;
        }
        public int HDijkstra(int[] n)
        {
            return 0;
        }
        public int HEuclid(int[] n)
        {
            int dx = Math.Abs(n[0] - end[0]);
            int dy = Math.Abs(n[1] - end[1]);
            return (int)Math.Sqrt(dx * dx + dy * dy) * 10;
        }
    }
}
