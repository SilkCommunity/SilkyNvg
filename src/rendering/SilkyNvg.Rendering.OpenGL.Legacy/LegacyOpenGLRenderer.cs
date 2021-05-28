using Silk.NET.Maths;
using Silk.NET.OpenGL.Legacy;
using SilkyNvg.Blending;
using SilkyNvg.Renderer;
using SilkyNvg.Rendering.OpenGL.Legacy.Blending;
using SilkyNvg.Rendering.OpenGL.Legacy.Calls;
using SilkyNvg.Rendering.OpenGL.Legacy.Shaders;
using System;
using System.Collections.Generic;
using Path = SilkyNvg.Rendering.OpenGL.Legacy.Paths.Path;

namespace SilkyNvg.Rendering.OpenGL.Legacy
{
    public class LegacyOpenGLRenderer : INvgRenderer
    {

        private readonly CallQueue _callQueue;
        private readonly List<Vertex> _vertices = new();

        private uint _vertexBuffer;

        private StateFilter _stateFilter;

        private Vector2D<float> _viewSize;

        public bool EdgeAntiAlias { get; }

        internal bool StencilStrokes { get; }

        internal bool Debug { get; }

        internal GL Gl { get; }

        internal Shaders.Shader Shader { get; private set; }

        internal StateFilter Filter => _stateFilter;

        public LegacyOpenGLRenderer(CreateFlags flags, GL gl)
        {
            Gl = gl;

            EdgeAntiAlias = (flags & CreateFlags.Antialias) != 0;
            StencilStrokes = (flags & CreateFlags.StencilStrokes) != 0;
            Debug = (flags & CreateFlags.Debug) != 0;

            _viewSize = default;
            _stateFilter = default;
            _callQueue = new CallQueue();
        }

        private void CheckError(string str)
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

            Shader = new Shaders.Shader("shader", EdgeAntiAlias, Gl);
            if (!Shader.ErrorStatus)
            {
                return false;
            }

            CheckError("uniform locations");
            Shader.GetUniforms();

            _vertexBuffer = Gl.GenBuffer();

            // TODO: Texture

            CheckError("create done!");

            Gl.Finish();

            return true;
        }

        public void Viewport(Vector2D<float> size, float _)
        {
            _viewSize = size;
        }

        public unsafe void Flush()
        {
            GLEnum err = Gl.GetError();
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
                Gl.StencilFunc(GLEnum.Always, 0, 0xffffffff);
                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.BindTexture(TextureTarget.Texture2D, 0);

                _stateFilter.BoundsTexture = 0;
                _stateFilter.StencilMask = 0xffffffff;
                _stateFilter.StencilFunk = StencilFunction.Always;
                _stateFilter.StencilFuncRef = 0;
                _stateFilter.StencilFuncMask = 0xffffffff;
                _stateFilter.BlendFunc = new Blend(GLEnum.InvalidEnum, Gl);


                Gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
                fixed (void* verts = _vertices.ToArray())
                {
                    Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Count * sizeof(Vertex)), verts, BufferUsageARB.StreamDraw);
                }
                Gl.EnableVertexAttribArray(0);
                Gl.EnableVertexAttribArray(1);
                Gl.VertexAttribPointer(0, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)0);
                Gl.VertexAttribPointer(1, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(0 + (2 * sizeof(float))));
                CheckError("Walrus!");

                Shader.LoadMeta(_viewSize, 0);

                Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

                _callQueue.CallAll(this);

                Gl.DisableVertexAttribArray(0);
                Gl.DisableVertexAttribArray(1);

                Gl.Disable(EnableCap.CullFace);
                Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
                Shader.Stop();

                _vertices.Clear();
                _callQueue.Clear();
            }
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Vector4D<float> bounds, Renderer.Path[] paths)
        {
            bool convexFill = false;
            int image = paint.Image;
            Blend blendFunc = new(compositeOperation, Gl);

            if (paths.Length == 1 && paths[0].Convex)
            {
                convexFill = true;
            }

            Path[] glPaths = new Path[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                uint offset = (uint)_vertices.Count;
                glPaths[i] = new Path(paths[i], offset);
                _vertices.AddRange(paths[i].Fill);
                _vertices.AddRange(paths[i].Stroke);
            }

            if (!convexFill)
            {
                // TODO!
            }
            else
            {
                Uniforms uniforms = new(paint, scissor, fringe, fringe, -1.0f);
                _callQueue.AddCall(new ConvexFillCall(uniforms, blendFunc, image, glPaths));
            }

        }

    }
}
