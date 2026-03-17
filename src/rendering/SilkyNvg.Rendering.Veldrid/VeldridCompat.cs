using Veldrid;

namespace SilkyNvg.Rendering.Veldrid
{
    /// <summary>
    /// Compatibility shim for API differences between upstream Veldrid 4.9.0 and ppy.Veldrid fork.
    /// All breaking renames are contained here — the rest of the codebase uses these constants.
    /// Controlled by USE_LEGACY_VELDRID define (set via UseLegacyVeldrid in Directory.Build.props).
    ///
    /// ppy.Veldrid (used by osu!) is the most actively maintained fork of Veldrid.
    /// Upstream Veldrid by mellinoe has not been updated since March 2024 and is effectively abandoned.
    /// ppy.Veldrid adds Android/iOS support and continues to receive fixes and improvements.
    ///
    /// Legacy Veldrid 4.9.0 → ppy.Veldrid rename mapping:
    ///   PixelFormat.R8_UNorm                          → PixelFormat.R8UNorm
    ///   PixelFormat.R8_G8_B8_A8_UNorm                 → PixelFormat.R8G8B8A8UNorm
    ///   PixelFormat.D24_UNorm_S8_UInt                 → PixelFormat.D24UNormS8UInt
    ///   BlendStateDescription.SingleAlphaBlend         → BlendStateDescription.SINGLE_ALPHA_BLEND
    ///   SamplerFilter.MinLinear_MagLinear_MipLinear    → SamplerFilter.MinLinearMagLinearMipLinear
    /// </summary>
    public static class VeldridCompat
    {
#if USE_LEGACY_VELDRID
        // Legacy Veldrid 4.9.0 (abandoned March 2024, desktop-only)
        public const string PackageName = "veldrid (legacy)";
        public static readonly PixelFormat PixelFormatR8UNorm = PixelFormat.R8_UNorm;
        public static readonly PixelFormat PixelFormatR8G8B8A8UNorm = PixelFormat.R8_G8_B8_A8_UNorm;
        public static readonly PixelFormat DepthStencilD24S8 = PixelFormat.D24_UNorm_S8_UInt;
        public static readonly BlendStateDescription SingleAlphaBlend = BlendStateDescription.SingleAlphaBlend;
        public const SamplerFilter LinearFilter = SamplerFilter.MinLinear_MagLinear_MipLinear;
#else
        // ppy.Veldrid fork (actively maintained, used by osu!, Android/iOS support)
        public const string PackageName = "ppy.Veldrid";
        public static readonly PixelFormat PixelFormatR8UNorm = PixelFormat.R8UNorm;
        public static readonly PixelFormat PixelFormatR8G8B8A8UNorm = PixelFormat.R8G8B8A8UNorm;
        public static readonly PixelFormat DepthStencilD24S8 = PixelFormat.D24UNormS8UInt;
        public static readonly BlendStateDescription SingleAlphaBlend = BlendStateDescription.SINGLE_ALPHA_BLEND;
        public const SamplerFilter LinearFilter = SamplerFilter.MinLinearMagLinearMipLinear;
#endif
    }
}