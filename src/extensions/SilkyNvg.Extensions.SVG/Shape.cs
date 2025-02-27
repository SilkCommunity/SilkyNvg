using Silk.NET.Maths;
using SilkyNvg.Blending;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Rendering;

namespace SilkyNvg.Extensions.Svg
{
    internal class Shape
    {

        private readonly List<IInstruction> _instructions;
        private readonly AttribState _attribs;

        internal Shape(List<IInstruction> instructions, AttribState attribs)
        {
            _instructions = instructions;
            _attribs = attribs;
        }

        internal void Draw(Nvg nvg)
        {
            nvg.instructionQueue.Clear();
            nvg.pathCache.Clear();

            nvg.instructionQueue.AddRange(_instructions);

            nvg.instructionQueue.FlattenPaths(nvg.stateStack.CurrentState.Transform, nvg.pixelRatio, nvg.pathCache);
            nvg.pathCache.ExpandStroke(_attribs.StrokeWidth * 0.5f, 0.0f, _attribs.StrokeLineCap, _attribs.StrokeLineJoin, _attribs.MiterLimit, nvg.pixelRatio);

            nvg.renderer.Stroke(_attribs.StrokePaint,
            new CompositeOperationState(CompositeOperation.SourceOver),
            new Scissor(new Vector2D<float>(-1.0f)),
                1.0f, _attribs.StrokeWidth, nvg.pathCache.Paths);
        }

    }
}
