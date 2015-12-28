using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pocatello
{
    class PathFinder
    {
        private List<Node> open = new List<Node>();
        private List<Node> closed = new List<Node>();
        private byte[,] obstacles;
        private int[] end;
        private Node path = null;
        private bool done = false;
        
        public void run(byte[,] obstacles, int[] start, int[] end, ref List<Node> nodes, ref Stack<int[]> path)
        {
            this.end = end;
            open.Add(new Node(start, HManhattan(start)));
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
            if (obstacles[open[0].getPos()[0], open[0].getPos()[1]] == 1 || obstacles[end[0], end[1]] == 1)
            {
                open.RemoveAt(0);
            }
            
            while (open.Count() != 0 && !done)
            {

                closed.Add(open[0]);
                open.RemoveAt(0);
                lookForNeighboursJPS(closed[closed.Count() - 1]);
            }
            open.AddRange(closed);
            nodes = open;
            
            if (!done)
            {
                path.Push(new int[] { -1, -1 });
            }
            else
            {
                while (!this.path.getParent().Equals(this.path.getPos()))
                {
                    path.Push(this.path.getPos());
                    this.path = this.path.getParent();
                }
                path.Push(this.path.getPos());
            }

        }
        
        private void lookForNeighboursJPS(Node c)
        {
            int[] n = c.getPos();
            int[] d = c.getD();
            int g = c.getG();
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
                int[] p = c.getPos();
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
        
        private int lookForNodeJPS(Node c, int[] d, int g, int[] n)
        {
            if (!ignore(n) && !done)
            {
                if (n[0] == end[0] && n[1] == end[1])
                {
                    insert(new Node(c, n, g, HManhattan(n), d));
                    return 1;
                }
                if (Math.Abs(d[0]) + Math.Abs(d[1]) == 1)
                {
                    if ((obstacles[n[0] + d[1], n[1] + d[0]] == 1) && (obstacles[n[0] + d[1] + d[0], n[1] + d[0] + d[1]] == 0))
                    {
                        insert(new Node(c, n, g, HManhattan(n), d));
                        return 1;
                    }
                    if ((obstacles[n[0] - d[1], n[1] - d[0]] == 1) && (obstacles[n[0] - d[1] + d[0], n[1] - d[0] + d[1]] == 0))
                    {
                        insert(new Node(c, n, g + 10, HManhattan(n), d));
                        return 1;
                    }
                    return lookForNodeJPS(c, d, g + 10, new int[] { n[0] + d[0], n[1] + d[1] });
                }
                else if (Math.Abs(d[0]) + Math.Abs(d[1]) == 2)
                {
                    if (lookForNodeJPS(new Node(c, n, g, HManhattan(n), d), new int[] { d[0], 0 }, g + 10, new int[] { n[0] + d[0], n[1] }) == 1)
                    {
                        insert(new Node(c, n, g, HManhattan(n), d));
                    }
                    if (lookForNodeJPS(new Node(c, n, g, HManhattan(n), d), new int[] { 0, d[1] }, g + 10, new int[] { n[0], n[1] + d[1] }) == 1)
                    {
                        insert(new Node(c, n, g, HManhattan(n), d));
                    }
                    if (obstacles[n[0] - d[0], n[1]] == 1 && obstacles[n[0] - d[0], n[1] + d[1]] == 0)
                    {
                        insert(new Node(c, n, g, HManhattan(n), d));
                        return 1;
                    }
                    if (obstacles[n[0], n[1] - d[1]] == 1 && obstacles[n[0] + d[0], n[1] - d[1]] == 0)
                    {
                        insert(new Node(c, n, g, HManhattan(n), d));
                        return 1;
                    }
                    return lookForNodeJPS(c, d, g + 14, new int[] { n[0] + d[0], n[1] + d[1] });
                }
            }
            return -1;
        }
        
        private void lookForNeighbours(Node c)
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
        
        private int lookForNode(int[] d, Node c, int g)
        {
            int[] j = c.getPos();
            int[] n = new int[2];
            n[0] = j[0] + d[0];
            n[1] = j[1] + d[1];
            
            if (ignore(n))
            {
                return -1;
            }
            insert(new Node(c, n, c.getG() + g, HManhattan(n), d));
            return -1;
        }
        
        private bool ignore(int[] n)
        {
            if (obstacles[n[0], n[1]] == 1)
            {
                return true;
            }
            if (closed.FindIndex(se => se.Equals(n)) != -1)
            {
                return true;
            }
            return false;
        }
        
        private void insert(Node c)
        {
            int[] n = c.getPos();
            int F = c.getF();
            int i = open.FindIndex(se => se.Equals(n));
            if (i != -1)
            {
                if (open[i].getF() > F)
                {
                    open[i].setG(c.getG());
                    open[i].setParent(c.getParent());
                    open = open.OrderBy(o => o.getF()).ToList();
                }
            }
            else
            {
                bool continu = true;
                int count = 0;
                while (continu && count < open.Count())
                {
                    if (F < open[count].getF())
                    {
                        open.Insert(count, c);
                        continu = false;
                    }
                    count++;
                }
                if (continu)
                {
                    open.Add(c);
                }
                if (n[0] == end[0] && n[1] == end[1])
                {
                    path = c;
                    done = true;
                }
            }
        }

        private int HManhattan(int[] n)
        {
            int dx = Math.Abs(n[0] - end[0]);
            int dy = Math.Abs(n[1] - end[1]);
            return (dx + dy) * 10;
        }
    }
}
