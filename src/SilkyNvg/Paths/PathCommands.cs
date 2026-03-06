namespace SilkyNvg.Paths;

internal enum PathCommands : byte
{
    
    MoveTo = 0,
    LineTo = 1,
    QuadraticBezierTo = 2,
    CubicBezierTo = 3,
    Close
    
}