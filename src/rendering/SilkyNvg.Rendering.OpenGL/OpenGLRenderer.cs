using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using SilkyNvg.Rendering.OpenGL.Blending;
using SilkyNvg.Rendering.OpenGL.Calls;
using SilkyNvg.Rendering.OpenGL.Shaders;
using SilkyNvg.Rendering.OpenGL.Utils;
using System;
using System.Collections.Generic;
using Shader = SilkyNvg.Rendering.OpenGL.Shaders.Shader;

namespace SilkyNvg.Rendering.OpenGL
{
    public sealed class OpenGLRenderer : INvgRenderer
    {

        private readonly CreateFlags _flags;
        private readonly VertexCollection _vertexCollection;
        private readonly CallQueue _callQueue;

        private VAO _vao;

        private Vector2D<float> _size;

        internal GL Gl { get; }

        internal bool StencilStrokes => _flags.HasFlag(CreateFlags.StencilStrokes);

        internal bool Debug => _flags.HasFlag(CreateFlags.Debug);

        internal StateFilter Filter { get; private set; }

        internal Shader Shader { get; private set; }

        public bool EdgeAntiAlias => _flags.HasFlag(CreateFlags.Antialias);

        public OpenGLRenderer(CreateFlags flags, GL gl)
        {
            _flags = flags;
            Gl = gl;

            _vertexCollection = new VertexCollection();
            _callQueue = new CallQueue();
        }

        internal void StencilMask(uint mask)
        {
            if (Filter.StencilMask != mask)
            {
                Filter.StencilMask = mask;
                Gl.StencilMask(mask);
            }
        }

        internal void StencilFunc(StencilFunction func, int @ref, uint mask)
        {
            if (Filter.StencilFunc != func ||
                Filter.StencilFuncRef != @ref ||
                Filter.StencilFuncMask != mask)
            {
                Filter.StencilFunc = func;
                Filter.StencilFuncRef = @ref;
                Filter.StencilFuncMask = mask;
                Gl.StencilFunc(func, @ref, mask);
            }
        }

        internal void CheckError(string str)
        {
            if (!Debug)
            {
                return;
            }

            GLEnum err = Gl.GetError();
            if (err != GLEnum.NoError)
            {
                Console.Error.WriteLine("Error " + err + " after" + Environment.NewLine + str);
                return;
            }
        }

        public bool Create()
        {
            CheckError("init");

            if (EdgeAntiAlias)
            {
                Shader = new Shader("SilkyNvg-Shader", "vertexShader", "fragmentShaderEdgeAA", Gl);
                if (!Shader.Status)
                {
                    return false;
                }
            }
            else
            {
                Shader = new Shader("SilkyNvg-Shader", "vertexShader", "fragmentShader", Gl);
                if (!Shader.Status)
                {
                    return false;
                }
            }

            CheckError("uniform locations");
            Shader.GetUniforms();

            _vao = new(Gl);
            _vao.Vbo = new(Gl);

            Filter = new StateFilter();

            Shader.BindUniformBlock();

            // Dummy tex will always be at index 0.
            _ = CreateTexture(Texture.Alpha, new Vector2D<uint>(1, 1), 0, null);

            CheckError("create done!");

            Gl.Finish();

            return true;
        }

        public int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
        {
            Textures.Texture texture = new(size, imageFlags, type, data, this);
            CheckError("creating texture.");

            return texture.Id;
        }

        public bool DeleteTexture(int image)
        {
            Textures.Texture tex = Textures.Texture.FindTexture(image);
            if (tex == null)
            {
                return false;
            }
            tex.Dispose();
            return true;
        }

        public bool UpdateTexture(int image, Rectangle<uint> bounds, ReadOnlySpan<byte> data)
        {
            Textures.Texture tex = Textures.Texture.FindTexture(image);

            if (tex == null)
            {
                return false;
            }
            tex.Update(bounds, data);
            CheckError("updating texture.");
            return true;
        }

        public bool GetTextureSize(int image, out Vector2D<uint> size)
        {
            Textures.Texture tex = Textures.Texture.FindTexture(image);
            if (tex == null)
            {
                size = default;
                return false;
            }
            size = tex.Size;
            return false;
        }

        public void Viewport(Vector2D<float> size, float devicePixelRatio)
        {
            _size = size;
        }

        public void Cancel()
        {
            _vertexCollection.Clear();
            _callQueue.Clear();
        }

        public void Flush()
        {
            if (_callQueue.HasCalls)
            {
                Shader.Start();

                Gl.Enable(EnableCap.CullFace);
                Gl.CullFace(CullFaceMode.Back);
                Gl.FrontFace(FrontFaceDirection.Ccw);
                Gl.Enable(EnableCap.Blend);
                Gl.Disable(EnableCap.DepthTest);
                Gl.Disable(EnableCap.ScissorTest);
                Gl.ColorMask(true, true, true, true);
                Gl.StencilMask(0xffffffff);
                Gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                Gl.StencilFunc(StencilFunction.Always, 0, 0xffffffff);
                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.BindTexture(TextureTarget.Texture2D, 0);
                Filter.BoundTexture = 0;
                Filter.StencilMask = 0xffffffff;
                Filter.StencilFunc = StencilFunction.Always;
                Filter.StencilFuncRef = 0;
                Filter.StencilFuncMask = 0xffffffff;
                Filter.BlendFunc = new Blend(GLEnum.InvalidEnum, GLEnum.InvalidEnum, GLEnum.InvalidEnum, GLEnum.InvalidEnum);

                Shader.UploadUniformData();

                _vao.Bind();
                _vao.Vbo.Update(_vertexCollection.Vertices);

                Shader.LoadInt(UniformLoc.Tex, 0);
                Shader.LoadVector(UniformLoc.ViewSize, _size);

                Shader.BindUniformBuffer();
                _callQueue.Run();

                Gl.DisableVertexAttribArray(0);
                Gl.DisableVertexAttribArray(1);

                _vao.Unbind();

                Gl.Disable(EnableCap.CullFace);
                Shader.Stop();

                if (Filter.BoundTexture != 0)
                {
                    Filter.BoundTexture = 0;
                    Gl.BindTexture(TextureTarget.Texture2D, 0);
                }
            }

            _vertexCollection.Clear();
            _callQueue.Clear();
            Shader.UniformManager.Clear();
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Box2D<float> bounds, IReadOnlyList<Rendering.Path> paths)
        {
            int offset = _vertexCollection.CurrentsOffset;
            Path[] renderPaths = new Path[paths.Count];
            for (int i = 0; i < paths.Count; i++)
            {
                Rendering.Path path = paths[i];
                renderPaths[i] = new Path(
                    _vertexCollection.CurrentsOffset, path.Fill.Count,
                    _vertexCollection.CurrentsOffset + path.Fill.Count, path.Stroke.Count
                );
                _vertexCollection.AddVertices(path.Fill);
                _vertexCollection.AddVertices(path.Stroke);
                offset += path.Fill.Count;
                offset += path.Stroke.Count;
            }

            FragUniforms uniforms = new(paint, scissor, fringe, fringe, -1.0f);
            Call call;
            if ((paths.Count == 1) && paths[0].Convex) // Convex
            {
                int uniformOffset = Shader.UniformManager.AddUniform(uniforms);
                call = new ConvexFillCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
            }
            else
            {
                _vertexCollection.AddVertex(new Vertex(bounds.Max, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Max.X, bounds.Min.Y, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Min.X, bounds.Max.Y, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Min, 0.5f, 1.0f));

                FragUniforms stencilUniforms = new(-1.0f, Shaders.ShaderType.Simple);
                int uniformOffset = Shader.UniformManager.AddUniform(stencilUniforms);
                _ = Shader.UniformManager.AddUniform(uniforms);

                call = new FillCall(paint.Image, renderPaths, offset, uniformOffset, compositeOperation, this);
            }

            _callQueue.Add(call);
        }

        public void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Rendering.Path> paths)
        {
            int offset = _vertexCollection.CurrentsOffset;
            Path[] renderPaths = new Path[paths.Count];
            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i].Stroke.Count > 0)
                {
                    renderPaths[i] = new Path(0, 0, offset, paths[i].Stroke.Count);
                }
                else
                {
                    renderPaths[i] = default;
                }
                _vertexCollection.AddVertices(paths[i].Stroke);
                offset += paths[i].Stroke.Count;
            }

            FragUniforms uniforms = new(paint, scissor, strokeWidth, fringe, -1.0f);
            Call call;
            if (StencilStrokes)
            {
                FragUniforms stencilUniforms = new(paint, scissor, strokeWidth, fringe, 1.0f - 0.5f / 255.0f);
                int uniformOffset = Shader.UniformManager.AddUniform(uniforms);
                _ = Shader.UniformManager.AddUniform(stencilUniforms);

                call = new StencilStrokeCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
            }
            else
            {
                int uniformOffset = Shader.UniformManager.AddUniform(uniforms);
                call = new StrokeCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
            }
            _callQueue.Add(call);
        }

        public void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringe)
        {
            int offset = _vertexCollection.CurrentsOffset;
            _vertexCollection.AddVertices(vertices);

            FragUniforms uniforms = new(paint, scissor, fringe);
            int uniformOffset = Shader.UniformManager.AddUniform(uniforms);
            Call call = new TrianglesCall(paint.Image, new Blend(compositeOperation, this), offset, (uint)vertices.Count, uniformOffset, this);
            _callQueue.Add(call);
        }

        public void Dispose()
        {
            Shader.Dispose();

            _vao.Dispose();

            Textures.Texture.DeleteAll();

            _callQueue.Clear();
            _vertexCollection.Clear();
        }

    }
}