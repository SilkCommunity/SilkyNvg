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

        private readonly IInstruction[] _instructions;
        private readonly AttribState _attribs;

        internal Shape(List<IInstruction> instructions, AttribState attribs)
        {
            _instructions = instructions.ToArray();
            _attribs = attribs;
        }

        internal void Draw(Nvg nvg)
        {
            if (_attribs.HasFill)
            {

            }
            if (_attribs.HasStroke)
            {

            }

            nvg.instructionQueue.Clear();
            nvg.pathCache.Clear();

            nvg.instructionQueue.AddRange(_instructions);

            nvg.instructionQueue.FlattenPaths(nvg.stateStack.CurrentState.Transform, nvg.pixelRatio, nvg.pathCache);
            nvg.pathCache.ExpandFill(0.0f, Graphics.LineCap.Miter, 2.4f, nvg.pixelRatio);

            nvg.renderer.Fill(_attribs.FillPaint,
            new CompositeOperationState(CompositeOperation.SourceOver),
            new Scissor(new Vector2D<float>(-1.0f)),
                nvg.pixelRatio.FringeWidth, nvg.pathCache.Bounds, nvg.pathCache.Paths);
        }

    }
}
