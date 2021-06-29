using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.Rendering;
using System;

namespace SilkyNvg
{
    public sealed class Nvg : IDisposable
    {

        #region Create / Destroy
        public static Nvg Create(INvgRenderer renderer)
        {
            return new Nvg(new GraphicsManager(renderer));
        }

        internal readonly GraphicsManager graphicsManager;
        internal readonly InstructionQueue instructionQueue;
        internal readonly PathCache pathCache;
        internal readonly StateStack stateStack;
        internal readonly PixelRatio pixelRatio;

        internal Action endFrameText;

        public FrameMeta FrameMeta { get; internal set; }

        private Nvg(GraphicsManager graphicsManager)
        {
            this.graphicsManager = graphicsManager;

            instructionQueue = new InstructionQueue(this);
            pathCache = new PathCache(this);
            stateStack = new StateStack();
            pixelRatio = new PixelRatio();

            if (!graphicsManager.Create())
            {
                throw new InvalidOperationException("Failed to initialize the renderer!");
            }
        }

        public void Dispose()
        {
            instructionQueue.Clear();
            pathCache.Clear();

            graphicsManager.Delete();
        }
        #endregion

        #region Frames
        public void BeginFrame(Vector2D<float> windowSize, float devicePixelRatio)
        {
            stateStack.Clear();
            Save();
            Reset();

            pixelRatio.SetDevicePixelRatio(devicePixelRatio);

            graphicsManager.Viewport(windowSize, devicePixelRatio);

            FrameMeta = default;
        }

        public void BeginFrame(float windowWidth, float windowHeight, float devicePixelRatio) => BeginFrame(new Vector2D<float>(windowWidth, windowHeight), devicePixelRatio);

        public void CancelFrame()
        {
            graphicsManager.Cancel();
        }

        public void EndFrame()
        {
            graphicsManager.Flush();
            endFrameText?.Invoke();
        }

        #endregion

        #region Colour Utils
        public Colour Rgb(byte r, byte g, byte b) => Rgba(r, g, b, 255);

        public Colour RgbF(float r, float g, float b) => RgbaF(r, g, b, 1.0f);

        public Colour Rgba(byte r, byte g, byte b, byte a)
        {
            return new(r, g, b, a);
        }

        public Colour RgbaF(float r, float g, float b, float a)
        {
            return new(r, g, b, a);
        }

        public Colour TransRgba(Colour c, byte a)
        {
            return new(c, a);
        }

        public Colour TransRgbaF(Colour c, float a)
        {
            return new(c, a);
        }

        public Colour LerpRgba(Colour c0, Colour c1, float u)
        {
            return new(c0, c1, u);
        }

        public Colour Hsl(float h, float s, float l) => Hsla(h, s, l, 255);

        public Colour Hsla (float h, float s, float l, byte a)
        {
            return new(h, s, l, a);
        }
        #endregion

        #region State Handling
        public void Save()
        {
            stateStack.Save();
        }

        public void Restore()
        {
            stateStack.Restore();
        }

        public void Reset()
        {
            stateStack.Reset();
        }
        #endregion

        #region Paints
        public Paint LinearGradient(float sx, float sy, float ex, float ey, Colour icol, Colour ocol)
        {
            return Paint.LinearGradient(sx, sy, ex, ey, icol, ocol);
        }

        public Paint LinearGradient(Vector2D<float> s, Vector2D<float> e, Colour icol, Colour ocol) => LinearGradient(s.X, s.Y, e.X, e.Y, icol, ocol);

        public Paint BoxGradient(float x, float y, float w, float h, float r, float f, Colour icol, Colour ocol)
        {
            return Paint.BoxGradient(x, y, w, h, r, f, icol, ocol);
        }

        public Paint BoxGradient(Vector2D<float> pos, Vector2D<float> size, float r, float f, Colour icol, Colour ocol) => BoxGradient(pos.X, pos.Y, size.X, size.Y, r, f, icol, ocol);

        public Paint RadialGradient(float cx, float cy, float inr, float outr, Colour icol, Colour ocol)
        {
            return Paint.RadialGradient(cx, cy, inr, outr, icol, ocol);
        }

        public Paint RadialGradient(Vector2D<float> c, Vector2D<float> radii, Colour icol, Colour ocol) => RadialGradient(c.X, c.Y, radii.X, radii.Y, icol, ocol);
        #endregion

        #region Debug
        public void DebugDumpPathCache()
        {
            pathCache.Dump();
        }
        #endregion

    }
}
