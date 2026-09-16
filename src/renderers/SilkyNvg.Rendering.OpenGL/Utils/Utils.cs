using System.Runtime.CompilerServices;

namespace SilkyNvg.Rendering.OpenGL.Utils;

internal static class Utils
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint DivUp(uint a, uint b)
    {
        return (a + (b - 1)) / b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint Align(uint value, uint alignment)
    {
        return (value + alignment - 1) & ~(alignment - 1);
    }
    
}