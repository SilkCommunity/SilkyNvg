using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Colouring;
using SilkyNvg.Common;
using SilkyNvg.OpenGL.Calls;
using SilkyNvg.OpenGL.Shaders;
using System;
using System.Collections.Generic;
using Shader = SilkyNvg.OpenGL.Shaders.Shader;

namespace SilkyNvg.OpenGL
{
    internal sealed class GLInterface
    {

        private readonly Shader _shader;

        private readonly Queue<Call> _calls = new Queue<Call>();

        private readonly List<Path> _paths = new List<Path>();
        private readonly List<Vertex> _vertices = new List<Vertex>();
        private readonly List<FragmentDataUniforms> _uniforms = new List<FragmentDataUniforms>();

        private readonly uint _vao;
        private readonly uint _vertexVBO, _textureVBO;

        private readonly GraphicsManager _graphicsManager;
        private readonly GL _gl;

        private Vector2D<float> _view;
        private RenderMeta _renderMeta;

        internal void CheckError(string str)
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

            CheckError("init");
            _shader = new Shader("SilkyNvg-Shader", _graphicsManager.LaunchParameters.Antialias, _gl);
            CheckError("loaded shaders");
            _shader.GetUniforms();

            _vao = _gl.GenVertexArray();
            _vertexVBO = _gl.GenBuffer();
            _textureVBO = _gl.GenBuffer();

            CheckError("create done!");
            _gl.Finish();

            _renderMeta = new RenderMeta();
        }

        private void StencilMask(uint mask)
        {
            if (_renderMeta.StencilMask != mask)
            {
                _renderMeta.StencilMask = mask;
                _gl.StencilMask(mask);
            }
        }

        private void StencilFunc(StencilFunction func, uint mask, int stencilFuncRef)
        {
            if ((_renderMeta.StencilFunk != (GLEnum)func) || (_renderMeta.StencilFunkRef != stencilFuncRef) || (_renderMeta.StencilFunkMask != mask))
            {
                _renderMeta.StencilFunk = (GLEnum)func;
                _renderMeta.StencilFunkRef = stencilFuncRef;
                _renderMeta.StencilFunkMask = mask;
                _gl.StencilFunc(func, stencilFuncRef, mask);
            }
        }

        public void Viewport(float width, float height)
        {
            _view = new Vector2D<float>(width, height);
        }

        private unsafe void SetUniforms(int uniformOffset, int image)
        {
            _shader.LoadUniforms(_uniforms[uniformOffset]);

            // TODO: Images

            CheckError("tex paint tex");
        }

        private void BlendFuncSeperate(Blend blend)
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

        private void Fill(Call call)
        {
            int start = call.PathOffset;
            int pathCount = call.PathCount;

            _gl.Enable(EnableCap.StencilTest);
            StencilMask(0xff);
            StencilFunc(StencilFunction.Always, 0xff, 0);
            _gl.ColorMask(false, false, false, false);

            SetUniforms(call.UniformOffset, 0);
            CheckError("fill simple");

            _gl.StencilOpSeparate(StencilFaceDirection.Front, StencilOp.Keep, StencilOp.Keep, StencilOp.IncrWrap);
            _gl.StencilOpSeparate(StencilFaceDirection.Back, StencilOp.Keep, StencilOp.Keep, StencilOp.DecrWrap);
            _gl.Disable(EnableCap.CullFace);
            for (int i = 0; i < pathCount; i++)
            {
                _gl.DrawArrays(PrimitiveType.TriangleFan, _paths[start + i].FillOffset, (uint)_paths[start + i].FillCount);
            }
            _gl.Enable(EnableCap.CullFace);

            _gl.ColorMask(true, true, true, true);

            SetUniforms(call.UniformOffset, 0);
            CheckError("Fill fill");

            if (_graphicsManager.LaunchParameters.Antialias)
            {
                // TODO: Antialias
            }

            StencilFunc(StencilFunction.Notequal, 0xff, 0x0);
            _gl.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
            _gl.DrawArrays(PrimitiveType.TriangleStrip, call.TriangleOffset, (uint)call.TriangleCount);

            _gl.Disable(EnableCap.StencilTest);
        }

        private void ConvexFill(Call call)
        {
            var start = call.PathOffset;
            int pathCount = call.PathCount;

            SetUniforms(call.UniformOffset, 0);
            CheckError("convex fill");

            for (int i = 0; i < pathCount; i++)
            {
                _gl.DrawArrays(GLEnum.TriangleFan, _paths[start + i].FillOffset, (uint)_paths[start + i].FillCount);
                if (_paths[start + i].StrokeCount > 0)
                {
                    _gl.DrawArrays(GLEnum.TriangleStrip, _paths[start + i].StrokeOffset, (uint)_paths[start + i].StrokeCount);
                }
            }

        }

        public unsafe void Flush()
        {
            if (_calls.Count > 0)
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

                _gl.BindVertexArray(_vao);

                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexVBO);
                fixed (void* d = vertices)
                {
                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), d, BufferUsageARB.StreamDraw);
                }
                _gl.VertexAttribPointer(0, 2, GLEnum.Float, false, (uint)(2 * sizeof(float)), (float*)0);
                _gl.EnableVertexAttribArray(0);

                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _textureVBO);
                fixed (void* d = textureCoords)
                {
                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(textureCoords.Length * sizeof(float)), d, BufferUsageARB.StreamDraw);
                }
                _gl.VertexAttribPointer(1, 2, GLEnum.Float, false, (uint)(2 * sizeof(float)), (float*)0);
                _gl.EnableVertexAttribArray(1);

                _gl.Uniform1(_shader.Locations[UniformLocations.Tex], 0);
                _gl.Uniform2(_shader.Locations[UniformLocations.Viewsize], _view.X, _view.Y);

                while (_calls.Count > 0)
                {
                    var call = _calls.Dequeue();
                    BlendFuncSeperate(call.BlendFunc);
                    switch (call.Type)
                    {
                        case CallType.Fill:
                            Fill(call);
                            break;
                        case CallType.Convexfill:
                            ConvexFill(call);
                            break;
                        case CallType.Stroke:
                            // later
                            break;
                        case CallType.Triangles:
                            // later
                            break;
                        default:
                            throw new Exception("Invalid call type!");
                    }
                }

                _gl.DisableVertexAttribArray(0);
                _gl.DisableVertexAttribArray(1);
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
                _gl.BindVertexArray(0);
                _shader.Stop();

                _vertices.Clear();
                _paths.Clear();
                _calls.Clear();
                _uniforms.Clear();
            }
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

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Vector4D<float> bounds, SilkyNvg.Core.Paths.Path[] paths)
        {
            var call = new Call()
            {
                Type = CallType.Fill,
                TriangleCount = 4,
                PathOffset = _paths.Count,
                PathCount = paths.Length,
                // TODO: Image
                BlendFunc = new Blend(compositeOperation)
            };

            if (paths.Length == 1 && paths[0].Convex)
            {
                call.Type = CallType.Convexfill;
                call.TriangleCount = 0;
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
                _paths.Add(copy);
            }

            if (call.Type == CallType.Fill)
            {
                call.TriangleOffset = offset;
                _vertices.Add(new Vertex(bounds.Z, bounds.W, 0.5f, 1.0f));
                _vertices.Add(new Vertex(bounds.Z, bounds.Y, 0.5f, 1.0f));
                _vertices.Add(new Vertex(bounds.X, bounds.W, 0.5f, 1.0f));
                _vertices.Add(new Vertex(bounds.X, bounds.Y, 0.5f, 1.0f));

                call.UniformOffset = _uniforms.Count;
                var frag = new FragmentDataUniforms
                {
                    StrokeThr = -1.0f,
                    Type = (int)SilkyNvg.OpenGL.Shaders.ShaderType.Simple
                };

                frag = ConvertPaint(frag, paint, scissor, fringe, fringe, -1.0f);
                _uniforms.Add(frag);
            }
            else
            {
                call.UniformOffset = _uniforms.Count;
                var frag = ConvertPaint(new FragmentDataUniforms(), paint, scissor, fringe, fringe, -1.0f);
                _uniforms.Add(frag);
            }

            _calls.Enqueue(call);
        }

    }
}