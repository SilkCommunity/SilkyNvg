using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SilkyNvg.Core;
using SilkyNvg.Core.Composite;
using SilkyNvg.Core.Geometry;
using SilkyNvg.Core.Paths;
using SilkyNvg.OpenGL.Calls;
using SilkyNvg.OpenGL.Shaders;
using SilkyNvg.OpenGL.VertexArray;
using SilkyNvg.OpenGL.VertexBuffer;
using System;
using System.Collections.Generic;

namespace SilkyNvg.OpenGL
{
    public sealed class GLInterface
    {

        private readonly List<GLPath> _paths = new List<GLPath>();
        private readonly List<Vertex> _vertices = new List<Vertex>();

        private readonly List<FragmentData> _uniforms = new List<FragmentData>();
        private readonly List<Call> _calls = new List<Call>();

        private readonly GraphicsManager _graphicsManager;
        private readonly GL _gl;

        private Vector2D<float> _viewport = new Vector2D<float>(0.0F);

        private VAO _vao;
        private NvgShader _shader;

        private void CheckError(string str)
        {
            if (!_graphicsManager.LaunchParameters.Debug)
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
            Init();
        }

        private void Init()
        {
            CheckError("init");
            _shader = new NvgShader("NvgShader", _graphicsManager.LaunchParameters.EdgeAntialias, _gl);
            CheckError("Uniform locations");
            _shader.GetUniforms();
            _vao = new VAO(_gl);
            CheckError("Post initialization");
            _gl.Finish();
        }

        public void SetupViewSize(float width, float height)
        {
            _viewport.X = width;
            _viewport.Y = height;
        }

        private FragmentData ConvertPaint(FragmentData frag, Paint paint, Scissor scissor, float width, float fringe, float strokeThr)
        {

            frag.Premult(paint.InnerColour, paint.OuterColour);

            if (scissor.Extent.X < -0.5f || scissor.Extent.Y < -0.5f)
            {
                frag.ScissorMatrix = new Matrix3X4<float>();
                frag.ScissorExtent = new Vector2D<float>(1.0f, 1.0f);
                frag.ScissorScale = new Vector2D<float>(1.0f, 1.0f);
            }
            else
            {
                // TODO
            }

            frag.Extent = new Vector2D<float>(paint.Extent.X, paint.Extent.Y);
            frag.StrokeMultiplier = (width * 0.5f + fringe * 0.5f) / fringe;
            frag.StrokeThreshold = strokeThr;

            // TODO: Image

            frag.Type = Shaders.ShaderType.Fillgrad;
            frag.Radius = paint.Radius;
            frag.Feather = paint.Feather;
            var invxform = Maths.TransformInverse(paint.Transformation);

            frag.PaintMatrix = Maths.XFormToMat3x4(invxform);

            return frag;
        }

        private int MaxVertCount(Path[] paths, int pathCount)
        {
            int count = 0;
            for (int i = 0; i < pathCount; i++)
            {
                count += paths[i].NFill;
                count += paths[i].NStroke;
            }
            return count;
        }

        private void StencilMask(uint mask)
        {
            _gl.StencilMask(mask);
        }

        private void StencilFunk(StencilFunction func, int reference, uint mask)
        {
            _gl.StencilFunc(func, reference, mask);
        }

        private void Fill(Call call)
        {
            int offset = call.PathOffset;
            int npaths = call.PathCount;

            _gl.Enable(EnableCap.StencilTest);
            StencilMask(0xff);
            StencilFunk(StencilFunction.Always, 0, 0xff);
            _gl.ColorMask(false, false, false, false);

            CheckError("fill simple");

            _uniforms[offset].LoadToShader(_shader);
            CheckError("Uniform variables");

            _gl.StencilOpSeparate(GLEnum.Front, GLEnum.Keep, GLEnum.Keep, GLEnum.IncrWrap);
            _gl.StencilOpSeparate(GLEnum.Back, GLEnum.Keep, GLEnum.Keep, GLEnum.DecrWrap);
            _gl.Disable(EnableCap.CullFace);
            foreach (GLPath path in _paths)
            {
                _gl.DrawArrays(GLEnum.TriangleFan, path.FillOffset, (uint)path.FillCount);
            }
            _gl.Enable(GLEnum.CullFace);

            _gl.ColorMask(true, true, true, true);

            StencilFunk(StencilFunction.Notequal, 0x0, 0xff);
            _gl.StencilOp(StencilOp.Zero, StencilOp.Keep, StencilOp.Keep);
            _gl.DrawArrays(GLEnum.TriangleStrip, call.TriangleOffset, (uint)call.TriangleCount);

            _gl.Disable(GLEnum.StencilTest);
        }

        public void RenderFlush()
        {
            if (_calls.Count > 0)
            {
                _shader.Start();

                _gl.Enable(EnableCap.CullFace);
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
            }

            var vertexVBO = new VBO(BufferTargetARB.ArrayBuffer, _gl);
            vertexVBO.Store(_vertices.ToArray());
            _vao.StoreVBO(0, 2, VertexAttribPointerType.Float, vertexVBO);
            _vao.Bind();

            _shader.LoadStdUniforms(_viewport);

            foreach (Call call in _calls)
            {
                if (call.Type == CallType.Fill)
                    Fill(call);
            }

            _vao.Unbind();
            _shader.Stop();
            _gl.BindTexture(GLEnum.Texture2D, 0);

            _vertices.Clear();
            _paths.Clear();
            _calls.Clear();
            _uniforms.Clear();
        }

        public void RenderFill(Paint paint, ICompositeOperation compositeOperation, Scissor scissor, float fringe, Rectangle<float> bounds, Path[] paths, int pathCount)
        {
            Vertex[] quad = new Vertex[4];

            var call = new Call
            {
                Type = CallType.Fill,
                TriangleCount = 4,
                PathOffset = _paths.Count,
                PathCount = pathCount,
                // TODO: Images
                Blend = new Blend(compositeOperation)
            };
            _calls.Add(call);

            if (call.PathCount == -1)
                throw new Exception("!");

            if (pathCount == 1 && paths[0].Convex)
            {
                call.Type = CallType.ConvexFill;
                call.TriangleCount = 0;
            }

            int maxverts = MaxVertCount(paths, pathCount);
            int offset = _vertices.Count;

            for (int i = 0; i < pathCount; i++)
            {
                var copy = new GLPath();
                var path = paths[i];
                if (path.NFill > 0)
                {
                    copy.FillOffset = offset;
                    copy.FillCount = path.NFill;
                    _vertices.AddRange(path.Fill);
                    offset += path.NFill;
                }
                if (path.NStroke > 0)
                {
                    copy.StrokeOffset = offset;
                    copy.StrokeCount = path.NStroke;
                    _vertices.AddRange(path.Stroke);
                    offset += path.NStroke;
                }
                _paths.Add(copy);
            }

            if (call.Type == CallType.Fill)
            {
                call.TriangleOffset = offset;
                _vertices.Add(new Vertex(bounds.Origin.X + bounds.Size.X, bounds.Size.Y + bounds.Origin.Y, 0.5f, 1.0f));
                _vertices.Add(new Vertex(bounds.Origin.X + bounds.Size.X, bounds.Size.Y, 0.5f, 1.0f));
                _vertices.Add(new Vertex(bounds.Origin.X, bounds.Size.Y + bounds.Origin.Y, 0.5f, 1.0f));
                _vertices.Add(new Vertex(bounds.Origin.X, bounds.Size.Y, 0.5f, 1.0f));
                quad[0] = _vertices[offset];
                quad[1] = _vertices[offset + 1];
                quad[2] = _vertices[offset + 2];
                quad[3] = _vertices[offset + 3];

                call.UniformOffset = _uniforms.Count;
                _uniforms.Add(new FragmentData());
                var frag = _uniforms[call.UniformOffset];
                frag.StrokeThreshold = -1.0f;
                frag.Type = Shaders.ShaderType.Simple;

                _uniforms[call.UniformOffset] = ConvertPaint(frag, paint, scissor, fringe, fringe, -1.0f);
            }
            else
            {
                call.UniformOffset = _uniforms.Count;

                _uniforms[call.UniformOffset] = ConvertPaint(_uniforms[call.UniformOffset], paint, scissor, fringe, fringe, -1.0f);
            }

        }

    }
}
