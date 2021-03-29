
namespace SilkyNvg.Core.Instructions
{
    internal enum InstructionType
    {

        BezireTo = 2, // set position
        LineTo = 1, // set position
        MoveTo = 0, // set position
        Winding = 4, // no position
        Close = 3 // no position

    }
}