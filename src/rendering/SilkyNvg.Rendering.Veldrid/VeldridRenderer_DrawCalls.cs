using System;
using System.Runtime.InteropServices;
using SilkyNvg.Blending;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid;

namespace SilkyNvg.Rendering.Veldrid
{
    public sealed partial class VeldridRenderer
    {
        /// <summary>
        /// Ensures the vertex batch array has capacity for n additional vertices.
        /// Uses 1.5x growth strategy (same as OpenGL backend).
        /// When the CPU array grows, the GPU buffer is resized to match.
        /// Includes adaptive downsizing: if buffer is underutilized for FLUSHES_BEFORE_DOWNSIZE_EVAL flushes,
        /// it shrinks to 1.25x the peak usage to prevent permanent bloat from rare large draws.
        /// </summary>
        private void EnsureVertexCapacity(int additionalVertexCount)
        {
            int requiredCapacity = _vertexBatchCount + additionalVertexCount;
            
            // Check for upsize (growth)
            if (requiredCapacity > _vertexBatchArray.Length) {
                int newCapacity = Math.Max(requiredCapacity, 4096) + _vertexBatchArray.Length / 2;
                Array.Resize(ref _vertexBatchArray, newCapacity);
                
                // Resize GPU buffer to match CPU array capacity (unified sizing)
                ResizeGpuVertexBuffer(newCapacity);
                
                // Reset downsizing tracking after resize
                _flushesSinceLastBufferResize = 0;
                _peakVertexCountSinceLastResize = 0;
            }
            // Check for downsize (adaptive shrinking after sustained underutilization)
            else if (_flushesSinceLastBufferResize >= FLUSHES_BEFORE_DOWNSIZE_EVAL) {
                // Reset counter regardless of whether we downsize (keeps common path fast)
                _flushesSinceLastBufferResize = 0;
                
                // Only downsize if peak usage is less than half of current capacity
                if (_peakVertexCountSinceLastResize < _vertexBatchArray.Length / 2) {
                    int downsizedCapacity = Math.Max(4096, (int)(_peakVertexCountSinceLastResize * 1.25f));
                    Array.Resize(ref _vertexBatchArray, downsizedCapacity);
                    ResizeGpuVertexBuffer(downsizedCapacity);
                }
                
                // Reset peak tracking for next evaluation period
                _peakVertexCountSinceLastResize = 0;
            }
        }

        /// <summary>
        /// Resizes the GPU vertex buffer to match the given capacity (in vertex count).
        /// Called when the CPU array grows to keep GPU and CPU buffers synchronized.
        /// </summary>
        private void ResizeGpuVertexBuffer(int vertexCapacity)
        {
            uint newBufferSizeBytes = (uint)(vertexCapacity * Marshal.SizeOf<ShaderLayouts.NvgVertex>());
            
            _vertexBuffer?.Dispose();
            _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(
                newBufferSizeBytes,
                BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        }

        /// <summary>
        /// Adds a single vertex to the batch (with automatic array growth).
        /// </summary>
        private void AddVertex(ShaderLayouts.NvgVertex vertex)
        {
            EnsureVertexCapacity(1);
            _vertexBatchArray[_vertexBatchCount++] = vertex;
        }

        private enum DrawCallType : byte
        {
            SolidFill,
            Textured,       // Font atlas text (R8_UNorm alpha, UV from vertices)
            Gradient,       // Linear/radial/box gradients (SDF in paint space)
            ImagePattern,   // Image fill (RGBA texture, UV from paintMat transform)
            NonConvexFill   // Two-pass stencil fill for self-intersecting/concave paths
        }

        // NvgVertex is now in Shaders/ShaderLayouts.cs (shared vertex format)
        // GradientUniforms is now in Shaders/GradientShader.cs
        // ImagePatternUniforms is now in Shaders/ImagePatternShader.cs

        private struct DrawCall
        {
            public int VertexOffset;
            public int VertexCount;
            public DrawCallType Type;
            public int TextureId; // Only used when Type == Textured or ImagePattern
            public ShaderLayouts.PaintUniforms PaintParams; // Used for Gradient and ImagePattern draw calls
            public bool HasScissor;
            public int ScissorX;
            public int ScissorY;
            public uint ScissorWidth;
            public uint ScissorHeight;
            public BlendStateDescription BlendState;

            // NonConvexFill sub-ranges (vertex data is: [stencil fill tris] [fringe tris] [cover quad])
            public int StencilFillVertexCount;   // Fill triangles for stencil pass
            public int CoverQuadVertexOffset;    // Bounds quad for cover pass (always 6 vertices)
            public DrawCallType NonConvexCoverPaintType; // Original paint type for cover pass (SolidFill/Gradient/ImagePattern)
        }

        /// <summary>
        /// Extracts an axis-aligned scissor rect from the NVG Scissor.
        /// Scissor.Transform positions the center, Scissor.Extent is the half-size.
        /// Returns false if scissor is disabled (extent &lt; -0.5).
        /// </summary>
        private static bool TryExtractScissorRect(Scissor scissor, out int scissorX, out int scissorY, out uint scissorWidth, out uint scissorHeight)
        {
            if (scissor.Extent.Width < -0.5f || scissor.Extent.Height < -0.5f) {
                scissorX = 0;
                scissorY = 0;
                scissorWidth = 0;
                scissorHeight = 0;
                return false;
            }

            // Scissor center is at Transform translation, extent is half-size
            float centerX = scissor.Transform.M31;
            float centerY = scissor.Transform.M32;
            float halfWidth = scissor.Extent.Width;
            float halfHeight = scissor.Extent.Height;

            scissorX = Math.Max(0, (int)(centerX - halfWidth));
            scissorY = Math.Max(0, (int)(centerY - halfHeight));
            scissorWidth = (uint)Math.Max(0, (int)(halfWidth * 2.0f));
            scissorHeight = (uint)Math.Max(0, (int)(halfHeight * 2.0f));
            return true;
        }

        private DrawCall CreateDrawCall(int vertexOffset, int vertexCount, Paint paint, Scissor scissor)
        {
            var drawCall = new DrawCall
            {
                VertexOffset = vertexOffset,
                VertexCount = vertexCount,
                BlendState = VeldridCompat.SingleAlphaBlend
            };

            // Determine draw call type based on paint properties
            if (paint.Image != 0) {
                // Image pattern fill: transform world position to UV via paintMat
                drawCall.Type = DrawCallType.ImagePattern;
                drawCall.TextureId = paint.Image;
                drawCall.PaintParams = ComputePaintUniforms(paint);
            } else if (IsGradientPaint(paint)) {
                drawCall.Type = DrawCallType.Gradient;
                drawCall.PaintParams = ComputePaintUniforms(paint);
            } else {
                drawCall.Type = DrawCallType.SolidFill;
            }

            drawCall.HasScissor = TryExtractScissorRect(scissor,
                out drawCall.ScissorX, out drawCall.ScissorY,
                out drawCall.ScissorWidth, out drawCall.ScissorHeight);

            return drawCall;
        }

        /// <summary>
        /// Detects if a paint represents a gradient (vs solid color).
        /// A gradient has different inner/outer colors, or non-zero radius/feather.
        /// </summary>
        private static bool IsGradientPaint(Paint paint)
        {
            // NVG sets radius > 0 for box gradients, feather > 1 for all gradients
            // Solid colors have feather = 0 and radius = 0
            return paint.Radius > 0 || paint.Feather > 1.0f ||
                   !ColorsEqual(paint.InnerColour, paint.OuterColour);
        }

        private static bool ColorsEqual(Colour colorA, Colour colorB)
        {
            return colorA.R == colorB.R && colorA.G == colorB.G &&
                   colorA.B == colorB.B && colorA.A == colorB.A;
        }

        /// <summary>
        /// Computes gradient uniform data from a NVG Paint, matching the OpenGL backend's FragUniforms logic.
        /// </summary>
        private static ShaderLayouts.PaintUniforms ComputePaintUniforms(Paint paint)
        {
            Matrix3x2.Invert(paint.Transform, out Matrix3x2 inversePaintTransform);

            return new ShaderLayouts.PaintUniforms
            {
                PaintMat = new Matrix4x4(inversePaintTransform),
                InnerColor = new Vector4(
                    paint.InnerColour.R, paint.InnerColour.G,
                    paint.InnerColour.B, paint.InnerColour.A),
                OuterColor = new Vector4(
                    paint.OuterColour.R, paint.OuterColour.G,
                    paint.OuterColour.B, paint.OuterColour.A),
                Extent = new Vector2(paint.Extent.Width, paint.Extent.Height),
                Radius = paint.Radius,
                Feather = MathF.Max(1.0f, paint.Feather)
            };
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, RectangleF bounds, IReadOnlyList<Path> paths)
        {
            // Use convex fast path for single convex paths (rects, circles, rounded rects)
            // Use stencil fill for multi-path or non-convex paths (pentagrams, concave shapes)
            // NOTE: Path.Convex doesn't detect self-intersection, but single-path self-intersecting
            // shapes (like pentagrams) are still classified as convex by NVG. For now, this means
            // pentagrams will still have triangle fan artifacts. A future fix could detect
            // self-intersection or always use stencil for paths with > N vertices.
            bool isConvexSinglePath = (paths.Count == 1) && paths[0].Convex;

            if (isConvexSinglePath) {
                FillConvex(paint, scissor, paths);
            } else {
                FillNonConvex(paint, scissor, bounds, paths);
            }
        }

        /// <summary>
        /// Convex fill: simple triangle fan + fringe. No stencil needed.
        /// </summary>
        private void FillConvex(Paint paint, Scissor scissor, IReadOnlyList<Path> paths)
        {
            var fillColor = paint.InnerColour;
            int vertexOffset = _vertexBatchCount;

            foreach (var path in paths) {
                // Fill vertices as triangle fan → triangle list
                if (path.Fill.Count >= 3) {
                    var firstVertex = path.Fill[0];
                    for (int i = 1; i < path.Fill.Count - 1; i++) {
                        AddVertex(new ShaderLayouts.NvgVertex(firstVertex, fillColor));
                        AddVertex(new ShaderLayouts.NvgVertex(path.Fill[i], fillColor));
                        AddVertex(new ShaderLayouts.NvgVertex(path.Fill[i + 1], fillColor));
                    }
                }

                // Fringe vertices for edge anti-aliasing
                if (path.Stroke.Count >= 3) {
                    for (int i = 0; i < path.Stroke.Count - 2; i++) {
                        if (i % 2 == 0) {
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i], fillColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 1], fillColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 2], fillColor));
                        } else {
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 1], fillColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i], fillColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 2], fillColor));
                        }
                    }
                }
            }

            int vertexCount = _vertexBatchCount - vertexOffset;
            if (vertexCount > 0) {
                _drawCalls.Add(CreateDrawCall(vertexOffset, vertexCount, paint, scissor));
            }
        }

        /// <summary>
        /// Non-convex fill: two-pass stencil-then-cover algorithm.
        /// Vertex layout: [stencil fill triangles] [fringe triangles] [cover quad (6 verts)]
        /// </summary>
        private void FillNonConvex(Paint paint, Scissor scissor, RectangleF bounds, IReadOnlyList<Path> paths)
        {
            var fillColor = paint.InnerColour;
            int vertexOffset = _vertexBatchCount;

            // Part 1: Fill triangles (for stencil pass — winding count)
            int stencilFillStartOffset = _vertexBatchCount;
            foreach (var path in paths) {
                if (path.Fill.Count >= 3) {
                    var firstVertex = path.Fill[0];
                    for (int i = 1; i < path.Fill.Count - 1; i++) {
                        AddVertex(new ShaderLayouts.NvgVertex(firstVertex, fillColor));
                        AddVertex(new ShaderLayouts.NvgVertex(path.Fill[i], fillColor));
                        AddVertex(new ShaderLayouts.NvgVertex(path.Fill[i + 1], fillColor));
                    }
                }
            }
            int stencilFillVertexCount = _vertexBatchCount - stencilFillStartOffset;

            // Part 2: Fringe triangles (for AA pass — drawn where stencil == 0)
            foreach (var path in paths) {
                if (path.Stroke.Count >= 3) {
                    for (int i = 0; i < path.Stroke.Count - 2; i++) {
                        if (i % 2 == 0) {
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i], fillColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 1], fillColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 2], fillColor));
                        } else {
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 1], fillColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i], fillColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 2], fillColor));
                        }
                    }
                }
            }

            // Part 3: Cover quad (bounds rectangle as 2 triangles, for cover pass)
            int coverQuadVertexOffset = _vertexBatchCount;
            var coverColor = fillColor; // Cover quad uses the fill color
            // TexCoord = (0.5, 1.0) for full opacity (same as fill body vertices)
            var coverVertex = new Vertex(0, 0, 0.5f, 1.0f);

            // Triangle 1: top-left, top-right, bottom-right
            coverVertex = new Vertex(bounds.Left, bounds.Top, 0.5f, 1.0f);
            AddVertex(new ShaderLayouts.NvgVertex(coverVertex, coverColor));
            coverVertex = new Vertex(bounds.Right, bounds.Top, 0.5f, 1.0f);
            AddVertex(new ShaderLayouts.NvgVertex(coverVertex, coverColor));
            coverVertex = new Vertex(bounds.Right, bounds.Bottom, 0.5f, 1.0f);
            AddVertex(new ShaderLayouts.NvgVertex(coverVertex, coverColor));

            // Triangle 2: top-left, bottom-right, bottom-left
            coverVertex = new Vertex(bounds.Left, bounds.Top, 0.5f, 1.0f);
            AddVertex(new ShaderLayouts.NvgVertex(coverVertex, coverColor));
            coverVertex = new Vertex(bounds.Right, bounds.Bottom, 0.5f, 1.0f);
            AddVertex(new ShaderLayouts.NvgVertex(coverVertex, coverColor));
            coverVertex = new Vertex(bounds.Left, bounds.Bottom, 0.5f, 1.0f);
            AddVertex(new ShaderLayouts.NvgVertex(coverVertex, coverColor));

            int totalVertexCount = _vertexBatchCount - vertexOffset;

            if (totalVertexCount > 0) {
                var drawCall = CreateDrawCall(vertexOffset, totalVertexCount, paint, scissor);
                // Preserve the original paint type (SolidFill/Gradient/ImagePattern) for the cover pass
                drawCall.NonConvexCoverPaintType = drawCall.Type;
                drawCall.Type = DrawCallType.NonConvexFill;
                drawCall.StencilFillVertexCount = stencilFillVertexCount;
                drawCall.CoverQuadVertexOffset = coverQuadVertexOffset;
                _drawCalls.Add(drawCall);
            }
        }

        public void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Path> paths)
        {
            var strokeColor = paint.InnerColour;

            int vertexOffset = _vertexBatchCount;

            foreach (var path in paths)
            {
                // Stroke vertices are already tesselated as triangle strips
                if (path.Stroke.Count >= 3) {
                    // Convert triangle strip to triangle list
                    for (int i = 0; i < path.Stroke.Count - 2; i++) {
                        if (i % 2 == 0) {
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i], strokeColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 1], strokeColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 2], strokeColor));
                        } else {
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 1], strokeColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i], strokeColor));
                            AddVertex(new ShaderLayouts.NvgVertex(path.Stroke[i + 2], strokeColor));
                        }
                    }
                }
            }

            int vertexCount = _vertexBatchCount - vertexOffset;
            if (vertexCount > 0) {
                _drawCalls.Add(CreateDrawCall(vertexOffset, vertexCount, paint, scissor));
            }
        }

        public void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringeWidth)
        {
            // Text rendering: paint.Image contains the font atlas texture ID.
            // Vertices already have proper UV coordinates from FontStash.
            // Force DrawCallType.Textured (font atlas pipeline) — NOT ImagePattern.
            var color = paint.InnerColour;
            int vertexOffset = _vertexBatchCount;

            foreach (var vertex in vertices) {
                AddVertex(new ShaderLayouts.NvgVertex(vertex, color));
            }

            int vertexCount = _vertexBatchCount - vertexOffset;
            if (vertexCount > 0) {
                var drawCall = CreateDrawCall(vertexOffset, vertexCount, paint, scissor);
                // Override: text always uses the font atlas textured pipeline
                drawCall.Type = DrawCallType.Textured;
                drawCall.TextureId = paint.Image;
                _drawCalls.Add(drawCall);
            }
        }
    }
}