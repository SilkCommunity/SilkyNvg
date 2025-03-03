using System;

namespace FontStash.NET
{
    internal class FonsAtlas
    {

        public int width, height;
        public FonsAtlasNode[] nodes;
        public int nnodes;
        public int cnodes;

        public FonsAtlas(int w, int h, int nnodes)
        {
            width = w;
            height = h;

            nodes = new FonsAtlasNode[nnodes];
            this.nnodes = 0;
            cnodes = nnodes;

            nodes[0].x = 0;
            nodes[0].y = 0;
            nodes[0].width = (short)w;
            this.nnodes++;
        }

        public bool InsertNode(int idx, int x, int y, int w)
        {
            if (nnodes + 1 > cnodes)
            {
                cnodes = cnodes == 0 ? 8 : cnodes * 2;
                try { Array.Resize(ref nodes, cnodes); } catch  { return false; }
            }
            for (int i = nnodes; i > idx; i--)
                nodes[i] = nodes[i - 1];
            nodes[idx].x = (short)x;
            nodes[idx].y = (short)y;
            nodes[idx].width = (short)w;
            nnodes++;
            return true;
        }

        public void RemoveNode(int idx)
        {
            if (nnodes == 0)
                return;
            for (int i = idx; i < nnodes - 1; i++)
            {
                nodes[i] = nodes[i + 1];
            }
            nnodes--;
        }

        public void Expand(int w, int h)
        {
            if (w > width)
                InsertNode(nnodes, width, 0, w - width);
            width = w;
            height = h;
        }

        public void Reset(int w, int h)
        {
            width = w;
            height = h;
            nnodes = 0;

            nodes[0].x = 0;
            nodes[0].y = 0;
            nodes[0].width = (short)w;
            nnodes++;
        }

        public bool AddSkylineLevel(int idx, int x, int y, int w, int h)
        {
            if (!InsertNode(idx, x, y + h, w))
                return false;

            for (int i = idx + 1; i < nnodes; i++)
            {
                if (nodes[i].x < nodes[i - 1].x + nodes[i - 1].width)
                {
                    int shrink = nodes[i - 1].x + nodes[i - 1].width - nodes[i].x;
                    nodes[i].x += (short)shrink;
                    nodes[i].width -= (short)shrink;
                    if (nodes[i].width <= 0)
                    {
                        RemoveNode(i);
                        i--;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            for (int i = 0; i < nnodes - 1; i++)
            {
                if (nodes[i].y == nodes[i + 1].y)
                {
                    nodes[i].width += nodes[i + 1].width;
                    RemoveNode(i + 1);
                    i--;
                }
            }

            return true;
        }

        public int RectFits(int i, int w, int h)
        {
            int x = nodes[i].x;
            int y = nodes[i].y;
            if (x + w > width)
                return -1;
            int spaceLeft = w;
            while (spaceLeft > 0)
            {
                if (i == nnodes)
                    return Fontstash.INVALID;
                y = Math.Max(y, nodes[i].y);
                if (y + h > height)
                    return Fontstash.INVALID;
                spaceLeft -= nodes[i].width;
                i++;
            }
            return y;
        }

        public bool AddRect(int rw, int rh, ref int rx, ref int ry)
        {
            int besth = height, bestw = width, besti = Fontstash.INVALID;
            int bestx = -1, besty = -1;

            for (int i = 0; i < nnodes; i++)
            {
                int y = RectFits(i, rw, rh);
                if (y != -1)
                {
                    if (y + rh < besth || (y + rh == besth && nodes[i].width < bestw))
                    {
                        besti = i;
                        bestw = nodes[i].width;
                        besth = y + rh;
                        bestx = nodes[i].x;
                        besty = y;
                    }
                }
            }

            if (besti == Fontstash.INVALID)
                return false;

            if (!AddSkylineLevel(besti, bestx, besty, rw, rh))
                return false;

            rx = bestx;
            ry = besty;

            return true;
        }

    }
}
