using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace SilkyNvg.Rendering.Veldrid
{
    /// <summary>
    /// Shared data layouts used across all NVG shaders.
    /// Defines the C# structs that must match GLSL uniform/vertex layouts exactly.
    /// </summary>
    internal static class ShaderLayouts
    {
        // ================================================================
        // VERTEX FORMAT — shared by ALL shaders
        //
        // GLSL vertex inputs (same for all pipelines):
        //   layout(location = 0) in vec2 Position;   //  8 bytes
        //   layout(location = 1) in vec2 TexCoord;    //  8 bytes
        //   layout(location = 2) in vec4 Color;       // 16 bytes
        //                                             // Total: 32 bytes per vertex
        //
        // Must use Vector2 and RgbaFloat to match Veldrid's expected layout.
        // ================================================================
        [StructLayout(LayoutKind.Sequential)]
        internal struct NvgVertex
        {
            public Vector2 Position;    //  8 bytes — world-space position
            public Vector2 TexCoord;    //  8 bytes — UV for text, AA coverage for fills/strokes
            public RgbaFloat Color;     // 16 bytes — per-vertex color (premultiplied by NVG)
            // Total: 32 bytes

            public NvgVertex(Vertex nvgVertex, Colour nvgColor)
            {
                Position = new Vector2(nvgVertex.X, nvgVertex.Y);
                TexCoord = new Vector2(nvgVertex.U, nvgVertex.V);
                Color = new RgbaFloat(nvgColor.R, nvgColor.G, nvgColor.B, nvgColor.A);
            }
        }

        /// <summary>
        /// Creates the VertexLayoutDescription shared by all NVG pipelines.
        /// Matches the NvgVertex struct layout above.
        /// </summary>
        internal static VertexLayoutDescription CreateVertexLayout()
        {
            return new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2, 8),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4, 16));
        }

        // ================================================================
        // VIEW SIZE UNIFORM — shared by ALL shaders at set=0, binding=0
        //
        // GLSL:
        //   layout(set = 0, binding = 0) uniform ViewSize {
        //       vec2 viewSize;    //  8 bytes
        //   };
        //
        // Uploaded as Vector4 for 16-byte alignment (GPU uniform buffers
        // require 16-byte alignment; vec2 alone would be 8 bytes).
        // The .zw components are unused padding.
        // ================================================================
        // No struct needed — uploaded as Vector4 directly:
        //   new Vector4(viewportWidth, viewportHeight, 0, 0)

        // ================================================================
        // PAINT UNIFORMS — shared by GradientShader and ImagePatternShader
        //
        // Both shaders use the same uniform block layout because they both
        // need the NVG paint transform (paintMat), colors, and extent.
        // They share a single GPU buffer (_paintUniformBuffer) that is
        // updated per draw call.
        //
        // GLSL layout (std140) — used in both GradientParams and ImagePatternParams:
        //   mat4 paintMat;      // 64 bytes (inverse paint transform)
        //   vec4 innerCol;      // 16 bytes (gradient start color / image tint)
        //   vec4 outerCol;      // 16 bytes (gradient end color / unused for images)
        //   vec2 extent;        //  8 bytes (gradient half-size / image size for UV)
        //   float radius;       //  4 bytes (box gradient corner radius / unused for images)
        //   float feather;      //  4 bytes (gradient softness / unused for images)
        //                       // Total: 112 bytes
        // ================================================================
        [StructLayout(LayoutKind.Sequential)]
        internal struct PaintUniforms
        {
            public Matrix4x4 PaintMat;     // 64 bytes — inverse paint transform
            public Vector4 InnerColor;     // 16 bytes — gradient start / image tint
            public Vector4 OuterColor;     // 16 bytes — gradient end / unused for images
            public Vector2 Extent;         //  8 bytes — gradient half-size / image UV size
            public float Radius;           //  4 bytes — box gradient corner radius
            public float Feather;          //  4 bytes — gradient softness
            // Total: 112 bytes
        }
    }
}