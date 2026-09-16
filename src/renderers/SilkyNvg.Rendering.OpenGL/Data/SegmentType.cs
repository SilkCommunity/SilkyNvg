namespace SilkyNvg.Rendering.OpenGL.Data;

/*
 * NOTE: This is another optimization to reduce nesting in the shader:
 *  The least significant three bits associated to each SegmentType are equal
 *  to the number of control points taken.
 *  Rational is also a cubic Bezier, however we add another flag in the fourth bit to mark it different from Quadratic.
 */
internal enum SegmentType : byte
{
    
    Linear = 0b0010,
    Quadratic = 0b0011,
    Cubic = 0b0100,
    Rational = 0b1011
    
}