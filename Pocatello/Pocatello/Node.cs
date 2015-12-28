using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pocatello
{
    class Node
    {
        private int F;
        private int H;
        private int G;
        private Node parent;
        private int[] pos;
        private int[] d;
        
        public Node(int[] n, int h)
        {
            pos = n;
            G = 0;
            H = h;
            d = new int[] { 0, 0 };
            F = G + H;
            parent = this;
        }
        
        public Node(Node C, int[] pos, int g, int h, int[] d)
        {
            parent = C;
            this.G = g;
            this.H = h;
            this.d = d;
            F = this.G + this.H;
            this.pos = pos;
        }
        
        public Node(Node C)
        {
            this.d = C.getD();
            this.parent = C.getParent();
            this.F = C.getF();
            this.G = C.getG();
            this.H = C.getH();
            this.pos = C.getPos();
        }
        
        public void setParent(Node c)
        {
            parent = c;
        }
        public Node getParent()
        {
            return parent;
        }
        public void setG(int g)
        {
            G = g;
            F = G + H;
        }
        public int[] getD()
        {
            return d;
        }
        public int[] getPos()
        {
            return pos;
        }
        public int getG()
        {
            return G;
        }
        public int getF()
        {
            return F;
        }
        public int getH()
        {
            return H;
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
