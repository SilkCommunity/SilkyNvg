using SilkyNvg.Common;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.Rendering;
using System;
using System.Drawing;
using System.Numerics;

namespace SilkyNvg
{
    public sealed class Nvg : IDisposable
    {

        #region Create / Destroy
        public static Nvg Create(Rendering.INvgRenderer renderer)
        {
            return new Nvg(renderer);
        }

        internal readonly Rendering.INvgRenderer renderer;
        internal readonly InstructionQueue instructionQueue;
        internal readonly PathCache pathCache;
        internal readonly StateStack stateStack;
        internal readonly PixelRatio pixelRatio;

        public FrameMeta FrameMeta { get; internal set; }

        private Nvg(Rendering.INvgRenderer renderer)
        {
            this.renderer = renderer;

            instructionQueue = new InstructionQueue(this);
            pathCache = new PathCache(this);
            stateStack = new StateStack();
            pixelRatio = new PixelRatio();

            if (!this.renderer.Create())
            {
                Dispose();
                throw new InvalidOperationException("Failed to initialize the renderer!");
            }
        }

        public void Dispose()
        {
            instructionQueue.Clear();
            pathCache.Clear();

            renderer.Dispose();
        }
        #endregion

        #region Frames
        /// <summary>
        /// Begin drawing a new frame.<br/>
        /// Calls to NanoVG drawing API should be wrapped in <see cref="BeginFrame(SizeF, float)"/> and <see cref="EndFrame()"/>.
        /// <para><see cref="BeginFrame(SizeF, float)"/> defines the size of the window to render to in relation currently
        /// set viewport (i.e. glViewport on GL backends). Device pixel ratio allows to
        /// control the rendering on Hi-DPI devices.</para>
        /// <para>For example, GLFW returns two dimension for an opened window: window size and
        /// frame buffer size. In that case you would set windowWidth / Height to the window size,
        /// devicePixelRatio to: <c>frameBufferWidth / windowWidth</c>.</para>
        /// </summary>
        public void BeginFrame(SizeF windowSize, float devicePixelRatio)
        {
            stateStack.Clear();
            Save();
            Reset();

            pixelRatio.SetDevicePixelRatio(devicePixelRatio);

            renderer.Viewport(windowSize, devicePixelRatio);

            FrameMeta = default;
        }

        /// <inheritdoc cref="BeginFrame(SizeF, float)"/>
        public void BeginFrame(Vector2 windowSize, float devicePixelRatio) => BeginFrame((SizeF)windowSize, devicePixelRatio);

        /// <inheritdoc cref="BeginFrame(SizeF, float)"/>
        public void BeginFrame(float windowWidth, float windowHeight, float devicePixelRatio) => BeginFrame(new SizeF(windowWidth, windowHeight), devicePixelRatio);

        /// <summary>
        /// Cancels drawing the current frame.
        /// </summary>
        public void CancelFrame()
        {
            renderer.Cancel();
        }

        /// <summary>
        /// End drawing flushing remaining render state.
        /// </summary>
        public void EndFrame()
        {
            renderer.Flush();
        }
        #endregion

        #region Colour Utils
        /// <inheritdoc cref="Colour(byte, byte, byte)"/>
        public Colour Rgb(byte r, byte g, byte b) => Rgba(r, g, b, 255);

        /// <inheritdoc cref="Colour(float, float, float)"/>
        public Colour RgbF(float r, float g, float b) => RgbaF(r, g, b, 1.0f);

        /// <inheritdoc cref="Colour(byte, byte, byte, byte)"/>
        public Colour Rgba(byte r, byte g, byte b, byte a)
        {
            return new(r, g, b, a);
        }

        /// <inheritdoc cref="Colour(float, float, float, float)"/>
        public Colour RgbaF(float r, float g, float b, float a)
        {
            return new(r, g, b, a);
        }

        /// <inheritdoc cref="Colour(Colour, byte)"/>
        public Colour TransRgba(Colour c, byte a)
        {
            return new(c, a);
        }

        /// <inheritdoc cref="Colour(Colour, float)"/>
        public Colour TransRgbaF(Colour c, float a)
        {
            return new(c, a);
        }

        /// <inheritdoc cref="Colour(Colour, Colour, float)"/>
        public Colour LerpRgba(Colour c0, Colour c1, float u)
        {
            return new(c0, c1, u);
        }

        /// <summary>
        /// HSL values are all in range [0..1], alpha will be set to 255.
        /// </summary>
        /// <returns>Color value specified by hue, saturation and lightness.</returns>
        public Colour Hsl(float h, float s, float l) => Hsla(h, s, l, 255);

        /// <inheritdoc cref="Colour(float, float, float, byte)"/>
        public Colour Hsla (float h, float s, float l, byte a)
        {
            return new(h, s, l, a);
        }
        #endregion

        #region State Handling
        /// <summary>
        /// Pushes and saves the current render state into a state stack.
        /// A matchine <see cref="Restore()"/> must be used to restore the state.
        /// </summary>
        public void Save()
        {
            stateStack.Save();
        }

        /// <summary>
        /// Pops and restores current render state.
        /// </summary>
        public void Restore()
        {
            stateStack.Restore();
        }

        /// <summary>
        /// Resets current render state to default values. Does not affect the render state stack.
        /// </summary>
        public void Reset()
        {
            stateStack.Reset();
        }
        #endregion

        #region Paints
        /// <inheritdoc cref="Paint.LinearGradient(Vector2, Vector2, Colour, Colour)"/>
        public Paint LinearGradient(Vector2 s, Vector2 e, Colour icol, Colour ocol)
            => Paint.LinearGradient(s, e, icol, ocol);

        /// <inheritdoc cref="Paint.LinearGradient(PointF, PointF, Colour, Colour)"/>
        public Paint LinearGradient(PointF s, PointF e, Colour icol, Colour ocol)
            => Paint.LinearGradient(s, e, icol, ocol);

        /// <inheritdoc cref="Paint.LinearGradient(float, float, float, float, Colour, Colour)"/>
        public Paint LinearGradient(float sx, float sy, float ex, float ey, Colour icol, Colour ocol)
            => Paint.LinearGradient(sx, sy, ex, ey, icol, ocol);

        /// <inheritdoc cref="Paint.BoxGradient(RectangleF, float, float, Colour, Colour)"/>
        public Paint BoxGradient(RectangleF box, float r, float f, Colour icol, Colour ocol)
            => Paint.BoxGradient(box, r, f, icol, ocol);

        /// <inheritdoc cref="Paint.BoxGradient(Vector4, float, float, Colour, Colour)"/>
        public Paint BoxGradient(Vector4 box, float r, float f, Colour icol, Colour ocol)
            => Paint.BoxGradient(box, r, f, icol, ocol);

        /// <inheritdoc cref="Paint.BoxGradient(PointF, SizeF, float, float, Colour, Colour)"/>
        public Paint BoxGradient(PointF pos, SizeF size, float r, float f, Colour icol, Colour ocol)
            => BoxGradient(pos, size, r, f, icol, ocol);

        /// <inheritdoc cref="Paint.BoxGradient(Vector2, Vector2, float, float, Colour, Colour)"/>
        public Paint BoxGradient(Vector2 pos, Vector2 size, float r, float f, Colour icol, Colour ocol)
            => BoxGradient(pos, size, r, f, icol, ocol);

        /// <inheritdoc cref="Paint.BoxGradient(float, float, float, float, float, float, Colour, Colour)"/>
        public Paint BoxGradient(float x, float y, float w, float h, float r, float f, Colour icol, Colour ocol)
            => Paint.BoxGradient(x, y, w, h, r, f, icol, ocol);

        /// <inheritdoc cref="Paint.RadialGradient(Vector2, float, float, Colour, Colour)"/>
        public Paint RadialGradient(Vector2 c, float inr, float outr, Colour icol, Colour ocol)
            => Paint.RadialGradient(c, inr, outr, icol, ocol);

        /// <inheritdoc cref="Paint.RadialGradient(PointF, float, float, Colour, Colour)"/>
        public Paint RadialGradient(PointF c, float inr, float outr, Colour icol, Colour ocol)
            => Paint.RadialGradient(c, inr, outr, icol, ocol);

        /// <inheritdoc cref="Paint.RadialGradient(float, float, float, float, Colour, Colour)"/>
        public Paint RadialGradient(float cx, float cy, float inr, float outr, Colour icol, Colour ocol)
            => Paint.RadialGradient(cx, cy, inr, outr, icol, ocol);
        #endregion

        #region Debug
        /// <summary>
        /// Debug function to dump cached path data.
        /// </summary>
        public void DebugDumpPathCache()
        {
            pathCache.Dump();
        }
        #endregion

    }
}
