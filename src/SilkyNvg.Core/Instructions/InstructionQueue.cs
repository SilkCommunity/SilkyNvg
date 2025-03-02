using Silk.NET.Maths;
using SilkyNvg.Blending;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering;
using System.Collections.Generic;

namespace SilkyNvg.Core.Instructions
{
    internal sealed class InstructionQueue
    {

        private const uint INIT_INSTRUCTIONS_SIZE = 256;

        private readonly Queue<IInstruction> _instructions = new((int)INIT_INSTRUCTIONS_SIZE);

        public Vector2D<float> EndPosition { get; private set; }

        public uint Count => (uint)_instructions.Count;

        public InstructionQueue()
        {
            EndPosition = default;
        }

        public void AddMoveTo(Vector2D<float> pos)
        {
            EndPosition = pos;
            _instructions.Enqueue(new MoveToInstruction(pos));
        }

        public void AddLineTo(Vector2D<float> pos)
        {
            EndPosition = pos;
            _instructions.Enqueue(new LineToInstruction(pos));
        }

        public void AddBezierTo(Vector2D<float> p0, Vector2D<float> p1, Vector2D<float> p2)
        {
            EndPosition = p2;
            _instructions.Enqueue(new BezierToInstruction(p0, p1, p2));
        }

        public void AddClose()
        {
            _instructions.Enqueue(new CloseInstruction());
        }

        public void AddWinding(Winding winding)
        {
            _instructions.Enqueue(new WindingInstruction(winding));
        }

        internal void Add(IInstruction instruction)
        {
            _instructions.Enqueue(instruction);
        }

        internal void AddRange(IList<IInstruction> instructions)
        {
            for (int i = 0; i <  instructions.Count; i++)
            {
                Add(instructions[i]);
            }
        }

        internal void RenderFill(Paint fillPaint, bool shapeAntialias, float alpha, CompositeOperationState compositeOperation, Scissor scissor,
            Matrix3X2<float> transform, PixelRatio pixelRatio, PathCache pathCache, INvgRenderer renderer)
        {
            FlattenPaths(transform, pixelRatio, pathCache);

            if (renderer.EdgeAntiAlias && shapeAntialias)
            {
                pathCache.ExpandFill(pixelRatio.FringeWidth, LineCap.Miter, 2.4f, pixelRatio);
            }
            else
            {
                pathCache.ExpandFill(0.0f, LineCap.Miter, 2.4f, pixelRatio);
            }

            fillPaint.PremultiplyAlpha(alpha);

            renderer.Fill(fillPaint, compositeOperation, scissor, pixelRatio.FringeWidth, pathCache.Bounds, pathCache.Paths);
        }

        internal void RenderStroke(Paint strokePaint, bool shapeAntialias, float strokeWidth, float alpha,
            LineCap lineCap, LineCap lineJoin, float miterLimit, CompositeOperationState compositeOperation, Scissor scissor,
            Matrix3X2<float> transform, PixelRatio pixelRatio, PathCache pathCache, INvgRenderer renderer)
        {
            float scale = Maths.GetAverageScale(transform);
            float calcStrokeWidth = Maths.Clamp(strokeWidth * scale, 0.0f, 200.0f);

            if (calcStrokeWidth < pixelRatio.FringeWidth)
            {
                float smallAlpha = Maths.Clamp(calcStrokeWidth / pixelRatio.FringeWidth, 0.0f, 1.0f);
                strokePaint.PremultiplyAlpha(smallAlpha * smallAlpha);
                calcStrokeWidth = pixelRatio.FringeWidth;
            }

            strokePaint.PremultiplyAlpha(alpha);

            FlattenPaths(transform, pixelRatio, pathCache);

            if (renderer.EdgeAntiAlias && shapeAntialias)
            {
                pathCache.ExpandStroke(calcStrokeWidth * 0.5f, pixelRatio.FringeWidth, lineCap, lineJoin, miterLimit, pixelRatio);
            }
            else
            {
                pathCache.ExpandStroke(calcStrokeWidth * 0.5f, 0.0f, lineCap, lineJoin, miterLimit, pixelRatio);
            }

            renderer.Stroke(strokePaint, compositeOperation, scissor, pixelRatio.FringeWidth, calcStrokeWidth, pathCache.Paths);
        }

        public void FlattenPaths(Matrix3X2<float> transform, PixelRatio pixelRatio, PathCache pathCache)
        {
            while (_instructions.Count > 0)
            {
                _instructions.Dequeue().BuildPaths(transform, pixelRatio, pathCache);
            }
            pathCache.FlattenPaths();
        }

        public void Clear()
        {
            _instructions.Clear();
        }

    }
}
