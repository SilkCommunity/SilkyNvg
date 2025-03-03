using Silk.NET.Maths;
using Silk.NET.SDL;
using SilkyNvg.Blending;
using SilkyNvg.Common;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Graphics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Extensions.Svg
{
    internal class Shape
    {

        private readonly IInstruction[] _instructions;
        private readonly AttribState _attribs;

        internal Shape(List<IInstruction> instructions, AttribState attribs)
        {
            _instructions = instructions.ToArray();
            _attribs = attribs;
        }

        internal void Draw(Nvg nvg)
        {
            nvg.instructionQueue.Clear();
            nvg.pathCache.Clear();

            nvg.instructionQueue.AddRange(_instructions);

            State state = nvg.stateStack.CurrentState;
            PixelRatio pixelRatio = nvg.pixelRatio;
            PathCache pathCache = nvg.pathCache;

            Matrix3X2<float> transform = Maths.Multiply(_attribs.Transform, state.Transform); // Matrix3X3.Multiply(new Matrix3X3<float>(_attribs.Transform), state.Transform);
            float opacity = state.Alpha * _attribs.Opacity;

            if (_attribs.HasFill)
            {
                nvg.instructionQueue.FlattenPaths(transform, pixelRatio, pathCache);

                if (nvg.renderer.EdgeAntiAlias && state.ShapeAntiAlias)
                {
                    pathCache.ExpandFill(pixelRatio.FringeWidth, LineCap.Miter, 2.4f, pixelRatio);
                }
                else
                {
                    pathCache.ExpandFill(0.0f, LineCap.Miter, 2.4f, pixelRatio);
                }

                SilkyNvg.Paint fillPaint = _attribs.FillPaint.GetPaint(pathCache.Bounds, _attribs.Viewport);
                fillPaint.PremultiplyAlpha(opacity);
                fillPaint.MultiplyTransform(transform);

                nvg.renderer.Fill(fillPaint, state.CompositeOperation, state.Scissor, pixelRatio.FringeWidth, pathCache.Bounds, pathCache.Paths);
            }
            if (_attribs.HasStroke)
            {
                float scale = Maths.GetAverageScale(transform);
                float calcStrokeWidth = Maths.Clamp(_attribs.StrokeWidth * scale, 0.0f, 200.0f);

                nvg.instructionQueue.FlattenPaths(transform, pixelRatio, pathCache);

                SilkyNvg.Paint strokePaint = _attribs.StrokePaint.GetPaint(pathCache.Bounds, _attribs.Viewport);

                if (calcStrokeWidth < pixelRatio.FringeWidth)
                {
                    float smallAlpha = Maths.Clamp(calcStrokeWidth / pixelRatio.FringeWidth, 0.0f, 1.0f);
                    strokePaint.PremultiplyAlpha(smallAlpha * smallAlpha);
                    calcStrokeWidth = pixelRatio.FringeWidth;
                }

                strokePaint.PremultiplyAlpha(opacity);
                strokePaint.MultiplyTransform(transform);

                if (nvg.renderer.EdgeAntiAlias && state.ShapeAntiAlias)
                {
                    pathCache.ExpandStroke(calcStrokeWidth * 0.5f, pixelRatio.FringeWidth, _attribs.StrokeLineCap, _attribs.StrokeLineJoin, _attribs.MiterLimit, pixelRatio);
                }
                else
                {
                    pathCache.ExpandStroke(calcStrokeWidth * 0.5f, 0.0f, _attribs.StrokeLineCap, _attribs.StrokeLineJoin, _attribs.MiterLimit, pixelRatio);
                }

                nvg.renderer.Stroke(strokePaint, state.CompositeOperation, state.Scissor, pixelRatio.FringeWidth, calcStrokeWidth, pathCache.Paths);
            }
        }

    }
}
