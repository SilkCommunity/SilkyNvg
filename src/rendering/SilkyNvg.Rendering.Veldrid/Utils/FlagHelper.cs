using System.Runtime.CompilerServices;

namespace SilkyNvg.Rendering.Vulkan.Utils
{
    public static class FlagHelper
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasFlag( int var, int flag )
        {
            return (var & flag) != 0;
        }
    }    
}
