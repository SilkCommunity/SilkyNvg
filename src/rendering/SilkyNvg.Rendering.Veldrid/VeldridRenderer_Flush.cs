using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace SilkyNvg.Rendering.Veldrid
{
    public sealed partial class VeldridRenderer
    {
        public void Flush()
        {
            if (_activeRenderCommandList == null)
            {
                throw new InvalidOperationException(
                    "VeldridRenderer.Flush() called without an active CommandList!\n" +
                    "You must call SetActiveCommandList(commandList) before BeginFrame/EndFrame.\n" +
                    "This is required to properly order SilkyNVG rendering with your rendering pipeline.");
            }

            if (!_isInitialized)
            {
                Cancel();
                return;
            }

            if (_vertexBatchCount == 0)
            {
                Cancel();
                return;
            }

            var commandList = _activeRenderCommandList;

            // Update view size uniform (use Vector4 for proper 16-byte alignment)
            // viewSize.z = Y direction multiplier: +1.0 for OpenGL/D3D11 (Y-up), -1.0 for Vulkan (Y-down)
            float clipSpaceYMultiplier = _graphicsDevice.IsClipSpaceYInverted ? -1.0f : 1.0f;
            var viewSizeData = new Vector4(_viewportSize.Width, _viewportSize.Height, clipSpaceYMultiplier, 0);
            _graphicsDevice.UpdateBuffer(_viewSizeUniformBuffer, 0, viewSizeData);

            // Upload vertices (zero-alloc: pass span slice directly, no ToArray() allocation)
            // GPU buffer is already sized correctly by EnsureVertexCapacity() during batching
            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertexBatchArray.AsSpan(0, _vertexBatchCount));

            // Set shared vertex buffer (same layout for both pipelines)
            commandList.SetVertexBuffer(0, _vertexBuffer);

            // Set viewport explicitly — OpenGL's default viewport may be stale after Veldrid's context takeover
            uint fullFramebufferWidth = _graphicsDevice.SwapchainFramebuffer.Width;
            uint fullFramebufferHeight = _graphicsDevice.SwapchainFramebuffer.Height;
            commandList.SetViewport(0, new Viewport(0, 0, fullFramebufferWidth, fullFramebufferHeight, 0, 1));

            // Default scissor to full framebuffer (scissor test is always enabled in pipelines)
            // IMPORTANT: Use framebuffer dimensions, not NVG viewport — scissor rect is in pixel coordinates
            commandList.SetScissorRect(0, 0, 0, fullFramebufferWidth, fullFramebufferHeight);

            // Scale factor from NVG coordinates to framebuffer pixels
            float nvgToFramebufferScaleX = _viewportSize.Width > 0 ? fullFramebufferWidth / _viewportSize.Width : 1.0f;
            float nvgToFramebufferScaleY = _viewportSize.Height > 0 ? fullFramebufferHeight / _viewportSize.Height : 1.0f;

            // Execute draw calls, switching pipeline and scissor per call
            DrawCallType lastPipelineType = (DrawCallType)255; // Force first switch
            int lastBoundTextureId = -1;
            bool lastScissorWasFullViewport = true;
            foreach (var drawCall in _drawCalls)
            {
                // Switch pipeline based on draw call type
                switch (drawCall.Type) {
                    case DrawCallType.SolidFill:
                        if (lastPipelineType != DrawCallType.SolidFill) {
                            commandList.SetPipeline(_solidFillPipeline);
                            commandList.SetGraphicsResourceSet(0, _viewSizeOnlyResourceSet);
                            lastPipelineType = DrawCallType.SolidFill;
                            lastBoundTextureId = -1;
                        }
                        break;

                    case DrawCallType.Textured:
                        if (lastPipelineType != DrawCallType.Textured || drawCall.TextureId != lastBoundTextureId) {
                            var texturedResourceSet = _textureRegistry.GetOrCreateTexturedResourceSet(drawCall.TextureId);
                            if (lastPipelineType != DrawCallType.Textured) {
                                commandList.SetPipeline(_texturedPipeline);
                                lastPipelineType = DrawCallType.Textured;
                            }
                            commandList.SetGraphicsResourceSet(0, texturedResourceSet);
                            lastBoundTextureId = drawCall.TextureId;
                        }
                        break;

                    case DrawCallType.Gradient:
                        // Update paint uniform buffer via command list for proper per-draw-call sequencing
                        commandList.UpdateBuffer(_paintUniformBuffer, 0, drawCall.PaintParams);
                        if (lastPipelineType != DrawCallType.Gradient) {
                            commandList.SetPipeline(_gradientPipeline);
                            lastPipelineType = DrawCallType.Gradient;
                        }
                        commandList.SetGraphicsResourceSet(0, _gradientResourceSet);
                        lastBoundTextureId = -1;
                        break;

                    case DrawCallType.ImagePattern:
                        // Update paint uniform buffer via command list for proper per-draw-call sequencing
                        commandList.UpdateBuffer(_paintUniformBuffer, 0, drawCall.PaintParams);
                        if (lastPipelineType != DrawCallType.ImagePattern) {
                            commandList.SetPipeline(_imagePatternPipeline);
                            lastPipelineType = DrawCallType.ImagePattern;
                        }
                        // Get or create cached resource set for this texture
                        if (!_imagePatternResourceSetCache.TryGetValue(drawCall.TextureId, out var imagePatternResourceSet)) {
                            var patternTextureView = _textureRegistry.GetTextureView(drawCall.TextureId);
                            imagePatternResourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                                _imagePatternResourceLayout,
                                _viewSizeUniformBuffer,
                                _paintUniformBuffer,
                                patternTextureView,
                                _imagePatternSampler));
                            _imagePatternResourceSetCache[drawCall.TextureId] = imagePatternResourceSet;
                        }
                        commandList.SetGraphicsResourceSet(0, imagePatternResourceSet);
                        lastBoundTextureId = drawCall.TextureId;
                        break;
                    case DrawCallType.NonConvexFill:
                        // Handled below with custom multi-pass draw
                        break;
                }

                // Apply scissor rect BEFORE drawing (scale from NVG coordinates to framebuffer pixels)
                if (drawCall.HasScissor) {
                    uint scaledScissorX = (uint)Math.Max(0, (int)(drawCall.ScissorX * nvgToFramebufferScaleX));
                    uint scaledScissorY = (uint)Math.Max(0, (int)(drawCall.ScissorY * nvgToFramebufferScaleY));
                    uint scaledScissorWidth = (uint)(drawCall.ScissorWidth * nvgToFramebufferScaleX);
                    uint scaledScissorHeight = (uint)(drawCall.ScissorHeight * nvgToFramebufferScaleY);
                    commandList.SetScissorRect(0,
                        scaledScissorX, scaledScissorY,
                        scaledScissorWidth, scaledScissorHeight);
                    lastScissorWasFullViewport = false;
                } else if (!lastScissorWasFullViewport) {
                    commandList.SetScissorRect(0, 0, 0, fullFramebufferWidth, fullFramebufferHeight);
                    lastScissorWasFullViewport = true;
                }

                // Execute draw (NonConvexFill has its own multi-pass draw sequence)
                if (drawCall.Type == DrawCallType.NonConvexFill) {
                    // === TWO-PASS STENCIL-THEN-COVER for non-convex paths ===
                    // Stencil is already cleared to 0 by ClearDepthStencil in GameLoop.

                    // Pass 1: Write winding count to stencil buffer (no color output)
                    // Front faces increment, back faces decrement (non-zero winding rule)
                    commandList.SetPipeline(_stencilFillPipeline);
                    commandList.SetGraphicsResourceSet(0, _viewSizeOnlyResourceSet);
                    commandList.Draw(
                        (uint)drawCall.StencilFillVertexCount, 1,
                        (uint)drawCall.VertexOffset, 0);

                    // Pass 2: Draw cover quad where stencil != 0, zeroing stencil as we go.
                    // Use the correct cover pipeline based on the original paint type.
                    switch (drawCall.NonConvexCoverPaintType) {
                        case DrawCallType.Gradient:
                            commandList.UpdateBuffer(_paintUniformBuffer, 0, drawCall.PaintParams);
                            commandList.SetPipeline(_stencilCoverGradientPipeline);
                            commandList.SetGraphicsResourceSet(0, _gradientResourceSet);
                            break;
                        case DrawCallType.ImagePattern:
                            commandList.UpdateBuffer(_paintUniformBuffer, 0, drawCall.PaintParams);
                            commandList.SetPipeline(_stencilCoverImagePatternPipeline);
                            if (!_imagePatternResourceSetCache.TryGetValue(drawCall.TextureId, out var coverImagePatternResourceSet)) {
                                var coverPatternTextureView = _textureRegistry.GetTextureView(drawCall.TextureId);
                                coverImagePatternResourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                                    _imagePatternResourceLayout,
                                    _viewSizeUniformBuffer,
                                    _paintUniformBuffer,
                                    coverPatternTextureView,
                                    _imagePatternSampler));
                                _imagePatternResourceSetCache[drawCall.TextureId] = coverImagePatternResourceSet;
                            }
                            commandList.SetGraphicsResourceSet(0, coverImagePatternResourceSet);
                            break;
                        default: // SolidFill
                            commandList.SetPipeline(_stencilCoverSolidPipeline);
                            commandList.SetGraphicsResourceSet(0, _viewSizeOnlyResourceSet);
                            break;
                    }
                    commandList.Draw(6, 1, (uint)drawCall.CoverQuadVertexOffset, 0);

                    // Reset pipeline tracking (stencil pipelines are transient)
                    lastPipelineType = (DrawCallType)255;
                    lastBoundTextureId = -1;
                } else {
                    commandList.Draw((uint)drawCall.VertexCount, 1, (uint)drawCall.VertexOffset, 0);
                }
            }

            Cancel();
        }

    }
}