using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.States;
using System.Numerics;

namespace SilkyNvg
{
    public static class Paths
    {

        public static void BeginPath(this Nvg nvg)
        {
            nvg.pathCache.Clear();
        }

        public static void Rect(this Nvg nvg, Vector2 pos, Vector2 size)
        {
            nvg.instructionQueue.AddInstructions(
                nvg.stateStack.CurrentState.transform,
                nvg.pathCache,
                new MoveToInstruction(pos),
                new LineToInstruction(new(pos.X, pos.Y + size.Y)),
                new LineToInstruction(pos + size),
                new LineToInstruction(new(pos.X + size.X, pos.Y)),
                new CloseInstruction()
            );
        }

        public static void Rect(this Nvg nvg, float x, float y, float w, float h) => Rect(nvg, new(x, y), new(w, h));

        public static void Fill(this Nvg nvg)
        {
            State state = nvg.stateStack.CurrentState;
            Paint fillPaint = state.fill;

            nvg.instructionQueue.BuildPaths();
        }

    }
}
