namespace FontStash.NET
{

    public delegate bool RenderCreate(int width, int height);
    public delegate bool RenderResize(int width, int height);
    public delegate void RenderUpdate(int[] rect, byte[] data);
    public delegate void RenderDraw(float[] verts, float[] tcoords, uint[] colours, int nverts);
    public delegate void RenderDelete();

    public struct FonsParams
    {

        public int width;
        public int height;

        public byte flags;

        public RenderCreate renderCreate;
        public RenderResize renderResize;
        public RenderUpdate renderUpdate;
        public RenderDraw renderDraw;
        public RenderDelete renderDelete;

    }
}
