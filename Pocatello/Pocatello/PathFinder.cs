using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pocatello
{
    class PathFinder
    {
        private List<Node> open = new List<Node>();
        private List<Node> closed = new List<Node>();
        private byte[,] obstacles;
        private int[] end;
        private Node pathNode = null;
        private bool done = false;
        private Canvas canvas;

        public Stack<int[]> run(byte[,] obstacles, int[] start, int[] end, System.Windows.Controls.Canvas canvas)
        {
            this.canvas = canvas;
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

            open.Add(new Node(start, HManhattan(start)));
            if (obstacles[open[0].Pos[0], open[0].Pos[1]] == 1 || obstacles[end[0], end[1]] == 1)
            {
                open.RemoveAt(0);
            }
            
            while (open.Count() != 0 && !done)
            {

                closed.Add(open[0]);
                open.RemoveAt(0);
                lookForNeighboursJPS(closed[closed.Count() - 1]);
            }
            //open.AddRange(closed);
            //nodes = open;

            var pathStack = new Stack<int[]>();
            if (!done)
            {
                pathStack.Push(new int[] { -1, -1 });
            }
            else
            {
                while (!pathNode.Parent.Equals(pathNode.Pos))
                {
                    pathStack.Push(pathNode.Pos);
                    pathNode = pathNode.Parent;
                }
                pathStack.Push(pathNode.Pos);
            }
            return pathStack;

        }
        
        private void lookForNeighboursJPS(Node c)
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
            int[] j = c.Pos;
            int[] n = new int[2];
            n[0] = j[0] + d[0];
            n[1] = j[1] + d[1];
            
            if (ignore(n))
            {
                return -1;
            }
            insert(new Node(c, n, c.G + g, HManhattan(n), d));
            return -1;
        }
        
        private bool ignore(int[] n)
        {
            if ((obstacles[n[0], n[1]] == 1) || (closed.FindIndex(se => se.Equals(n)) != -1))
            {
                return true;
            }
            return false;
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
                    var rekt = new System.Windows.Shapes.Rectangle();

                    rekt.Width = 5;
                    rekt.Height = 5;
                    rekt.Stroke = new SolidColorBrush(Colors.BlueViolet);
                    rekt.Fill = new SolidColorBrush(Colors.BlueViolet);
                    Canvas.SetLeft(rekt, c.Pos[0]);
                    Canvas.SetTop(rekt, c.Pos[1]);

                    canvas.Children.Add(rekt);
                }
                if (n[0] == end[0] && n[1] == end[1])
                {
                    pathNode = c;
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
