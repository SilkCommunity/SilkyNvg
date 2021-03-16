﻿using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Colouring;
using SilkyNvg.Common;
using SilkyNvg.OpenGL.Calls;
using SilkyNvg.OpenGL.Instructions;
using SilkyNvg.OpenGL.Shaders;
using SilkyNvg.OpenGL.VertexArray;
using System;
using System.Collections.Generic;
using Shader = SilkyNvg.OpenGL.Shaders.Shader;

namespace SilkyNvg.OpenGL
{
    internal sealed class GLInterface : IDisposable
    {

        private readonly Shader _shader;
        private readonly CallQueue _callQueue;

        private readonly List<Vertex> _vertices = new List<Vertex>();

        private readonly VAO _vao;

        private readonly GraphicsManager _graphicsManager;
        private readonly GL _gl;

        private Vector2D<float> _view;
        private RenderMeta _renderMeta;

        public LaunchParameters LaunchParameters => _graphicsManager.LaunchParameters;

        public void StencilMask(uint mask)
        {
            if (_renderMeta.StencilMask != mask)
            {
                _renderMeta.StencilMask = mask;
                _gl.StencilMask(mask);
            }
        }

        public void StencilFunc(StencilFunction func, uint mask, int stencilFuncRef)
        {
            if ((_renderMeta.StencilFunk != (GLEnum)func) || (_renderMeta.StencilFunkRef != stencilFuncRef) || (_renderMeta.StencilFunkMask != mask))
            {
                _renderMeta.StencilFunk = (GLEnum)func;
                _renderMeta.StencilFunkRef = stencilFuncRef;
                _renderMeta.StencilFunkMask = mask;
                _gl.StencilFunc(func, stencilFuncRef, mask);
            }
        }

        public void BlendFuncSeperate(Blend blend)
        {
            if ((_renderMeta.SrcRgb != blend.SrcRgb) ||
                (_renderMeta.DstRgb != blend.DstRgb) ||
                (_renderMeta.SrcAlpha != blend.SrcAlpha) ||
                (_renderMeta.DstAlpha != blend.DstAlpha))
            {
                _renderMeta.SrcRgb = blend.SrcRgb;
                _renderMeta.DstRgb = blend.DstRgb;
                _renderMeta.SrcAlpha = blend.SrcAlpha;
                _renderMeta.DstAlpha = blend.DstAlpha;
            }
            _gl.BlendFuncSeparate(blend.SrcRgb, blend.DstRgb, blend.SrcAlpha, blend.DstAlpha);
        }

        public void CheckError(string str)
        {
            if (_graphicsManager.LaunchParameters.Debug)
                return;
            GLEnum error = _gl.GetError();
            if (error != GLEnum.NoError)
            {
                Console.Error.WriteLine("Error " + error + " after " + str);
            }
        }

        public GLInterface(GraphicsManager graphicsManager)
        {
            _graphicsManager = graphicsManager;
            _gl = _graphicsManager.GL;

            _callQueue = new CallQueue();

            CheckError("init");
            _shader = new Shader("SilkyNvg-Shader", _graphicsManager.LaunchParameters.Antialias, _gl);
            CheckError("loaded shaders");

            _vao = new VAO(_gl);

            CheckError("create done!");
            _gl.Finish();

            _renderMeta = new RenderMeta();
        }

        private FragmentDataUniforms ConvertPaint(FragmentDataUniforms frag, Paint paint, Scissor scissor, float width, float fringe, float strokeThr)
        {
            frag.InnerColour = Colour.Premult(paint.InnerColour).Rgba;
            frag.OuterColour = Colour.Premult(paint.OuterColour).Rgba;

            if (scissor.Extent.X < -0.5f || scissor.Extent.Y < -0.5f)
            {
                frag.ScissorMatrix = new Matrix3X4<float>();
                frag.ScissorExt = new Vector2D<float>(1.0f);
                frag.ScissorScale = new Vector2D<float>(1.0f);
            }
            else
            {
                var inv = Maths.TransformInverse(scissor.XForm);
                frag.ScissorMatrix = Maths.XFormToMat3X4(inv);
                frag.ScissorExt = scissor.Extent;
                frag.ScissorScale = new Vector2D<float>
                (
                    MathF.Sqrt(scissor.XForm.M11 * scissor.XForm.M11 + scissor.XForm.M21 * scissor.XForm.M21) / fringe,
                    MathF.Sqrt(scissor.XForm.M12 * scissor.XForm.M12 + scissor.XForm.M22 * scissor.XForm.M22) / fringe
                );
            }

            frag.Extent = paint.Extent;
            frag.StrokeMult = (width * 0.5f + fringe * 0.5f) / fringe;
            frag.StrokeThr = strokeThr;

            Matrix3X2<float> invxform;

            if (false) // Does it have images?
            {
                // TODO: Image stuff goes here!
            }
            else
            {
                frag.Type = (int)Shaders.ShaderType.Fillgrad;
                frag.Radius = paint.Radius;
                frag.Feather = paint.Feather;
                invxform = Maths.TransformInverse(paint.XForm);
            }

            frag.PaintMatrix = Maths.XFormToMat3X4(invxform);

            return frag;
        }


        public unsafe void SetUniforms(FragmentDataUniforms uniforms, int image)
        {
            _shader.LoadUniforms(uniforms);

            // TODO: Images

            CheckError("tex paint tex");
        }

        public void Viewport(float width, float height)
        {
            _view = new Vector2D<float>(width, height);
        }

        public unsafe void Flush()
        {
            if (_callQueue.QueueLength > 0)
            {
                int idx = 0;
                float[] vertices = new float[_vertices.Count * 2];
                float[] textureCoords = new float[_vertices.Count * 2];
                foreach (Vertex vertex in _vertices)
                {
                    vertices[idx] = vertex.X;
                    textureCoords[idx++] = vertex.U;
                    vertices[idx] = vertex.Y;
                    textureCoords[idx++] = vertex.V;
                }

                _shader.Start();
                _gl.Enable(EnableCap.CullFace);
                _gl.CullFace(CullFaceMode.Back);
                _gl.FrontFace(FrontFaceDirection.Ccw);
                _gl.Enable(EnableCap.Blend);
                _gl.Disable(EnableCap.DepthTest);
                _gl.Disable(EnableCap.ScissorTest);
                _gl.ColorMask(true, true, true, true);
                _gl.StencilMask(0xffffffff);
                _gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                _gl.StencilFunc(GLEnum.Always, 0, 0xffffffff);
                _gl.ActiveTexture(TextureUnit.Texture0);
                _gl.BindTexture(TextureTarget.Texture2D, 0);
                _renderMeta.BondTexture = 0;
                _renderMeta.StencilMask = 0xffffffff;
                _renderMeta.StencilFunk = GLEnum.Always;
                _renderMeta.StencilFunkRef = 0;
                _renderMeta.StencilFunkMask = 0xffffffff;
                _renderMeta.SrcRgb = GLEnum.InvalidEnum;
                _renderMeta.SrcAlpha = GLEnum.InvalidEnum;
                _renderMeta.DstRgb = GLEnum.InvalidEnum;
                _renderMeta.DstAlpha = GLEnum.InvalidEnum;

                _vao.Bind();

                _vao.Vertices.BufferData(vertices);
                _vao.VertexAttribPointer(0, 2);

                _vao.TextureCoords.BufferData(textureCoords);
                _vao.VertexAttribPointer(1, 2);

                _shader.LoadMeta(0, _view);

                _callQueue.RunCalls(this, _gl);

                _vao.Unbind();
                _shader.Stop();

                _vertices.Clear();
            }
        }

        private void Vertex(float x, float y, float u, float v)
        {
            _vertices.Add(new Vertex(x, y, u, v));
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Vector4D<float> bounds, Core.Paths.Path[] paths)
        {
            Call call;

            CallType type = CallType.Fill;
            int triangleCount = 4;
            int triangleOffset = 0;
            Path[] paths_ = new Path[paths.Length];
            Blend blendFunc = new Blend(compositeOperation);

            if (paths.Length == 1 && paths[0].Convex)
            {
                type = CallType.Convexfill;
                triangleCount = 0;
            }

            int offset = _vertices.Count;

            for (int i = 0; i < paths.Length; i++)
            {
                var copy = new Path();
                var path = paths[i];
                if (path.Fill.Count > 0)
                {
                    copy.FillOffset = offset;
                    copy.FillCount = path.Fill.Count;
                    _vertices.AddRange(path.Fill);
                    offset += path.Fill.Count;
                }
                if (path.Stroke.Count > 0)
                {
                    copy.StrokeOffset = offset;
                    copy.StrokeCount = path.Stroke.Count;
                    _vertices.AddRange(path.Stroke);
                    offset += path.Stroke.Count;
                }
                paths_[i] = copy;
            }

            if (type == CallType.Fill)
            {
                triangleOffset = offset;
                Vertex(bounds.Z, bounds.W, 0.5f, 1.0f);
                Vertex(bounds.Z, bounds.Y, 0.5f, 1.0f);
                Vertex(bounds.X, bounds.W, 0.5f, 1.0f);
                Vertex(bounds.X, bounds.Y, 0.5f, 1.0f);

                var frag = new FragmentDataUniforms
                {
                    StrokeThr = -1.0f,
                    Type = (int)Shaders.ShaderType.Simple
                };

                frag = ConvertPaint(frag, paint, scissor, fringe, fringe, -1.0f);

                call = new FillCall(triangleOffset, triangleCount, blendFunc, frag, paths_);
            }
            else
            {
                var frag = ConvertPaint(new FragmentDataUniforms(), paint, scissor, fringe, fringe, -1.0f);

                call = new ConvexFillCall(triangleOffset, triangleCount, blendFunc, frag, paths_);
            }

            _callQueue.Add(call);
        }

        public void Dispose()
        {
            _shader.Dispose();
            _vao.Dispose();

            // TODO: Textures

            _vertices.Clear();
            _callQueue.Clear();
        }

    }
}