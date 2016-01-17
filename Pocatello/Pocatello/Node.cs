using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pocatello
{
    class Node
    {
        private int f;
        private int h;
        private int g;
        private Node parent;
        private int[] pos;
        private int[] d;
        
        public Node(int[] n, int h)
        {
            pos = n;
            g = 0;
            this.h = h;
            d = new int[] { 0, 0 };
            f = g + h;
            parent = this;
        }
        
        public Node(Node C, int[] pos, int g, int h, int[] d)
        {
            parent = C;
            this.g = g;
            this.h = h;
            this.d = d;
            f = this.g + this.h;
            this.pos = pos;
        }

        public Node(Node C)
        {
            d = C.D;
            parent = C.Parent;
            f = C.F;
            g = C.G;
            h = C.H;
            pos = C.Pos;
        }

        public Node Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }
        public int G
        {
            get
            {
                return g;
            }
            set
            {
                g = value;
                f = g + h;
            }
        }
        public int H
        {
            get
            {
                return h;
            }
            set
            {
                h = value;
                f = g + h;
            }
        }
        public int[] D
        {
            get
            {
                return d;
            }
            set
            {
                d = value;
            }
        }
        public int[] Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }
        public int F
        {
            get
            {
                return f;
            }
        }
        public override bool Equals(object obj)
        {
            int[] c = obj as int[];
            return (c[0] == pos[0]) && (c[1] == pos[1]);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "pos: " + pos[0] + ", " + pos[1] + " F:" + F + " G:" + G + " H:" + H;
        }
    }
}
