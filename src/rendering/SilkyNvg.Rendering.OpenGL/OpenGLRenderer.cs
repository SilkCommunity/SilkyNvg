using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Renderer;
using SilkyNvg.Rendering.OpenGL.Shaders;
using SilkyNvg.Rendering.OpenGL.Utils;
using System;
using Shader = SilkyNvg.Rendering.OpenGL.Shaders.Shader;

namespace SilkyNvg.Rendering.OpenGL
{
    public sealed class OpenGLRenderer : INvgRenderer
    {

        private readonly CreateFlags _flags;

        private Shader _shader;

        private Vector2D<float> _size;

        private uint _vertexArray;
        private uint _vertexBuffer;

        internal GL Gl { get; }

        internal bool StencilStrokes => _flags.HasFlag(CreateFlags.StencilStrokes);

        internal bool Debug => _flags.HasFlag(CreateFlags.Debug);

        public bool EdgeAntiAlias => _flags.HasFlag(CreateFlags.Antialias);

        public OpenGLRenderer(CreateFlags flags, GL gl)
        {
            _flags = flags;
            Gl = gl;
        }

        internal bool CheckError(string str)
        {
            if (!Debug)
            {
                return false;
            }

            GLEnum err = Gl.GetError();
            if (err != GLEnum.NoError)
            {
                Console.Error.WriteLine("Error " + err + " after" + Environment.NewLine + str);
                return true;
            }
            return false;
        }

        public bool Create()
        {
            _ = CheckError("init");

            if (EdgeAntiAlias)
            {
                _shader = new Shader("SilkyNvg-Shader", "vertexShader", "fragmentShaderEdgeAA", Gl);
                if (!_shader.Status)
                {
                    return false;
                }
            }
            else
            {
                _shader = new Shader("SilkyNvg-Shader", "vertexShader", "fragmentShader", Gl);
                if (!_shader.Status)
                {
                    return false;
                }
            }

            _ = CheckError("uniform locations");
            _shader.GetUniforms();

            _vertexArray = Gl.GenVertexArray();
            _vertexBuffer = Gl.GenBuffer();

            _ = CheckError("done");

            return true;
        }

        private bool SetupPaint(Paint paint, Scissor scissor, float width, float fringe)
        {
            Colour innerCol = paint.InnerColour;
            Colour outerCol = paint.OuterColour;

            Matrix3X2<float> invtransform = Maths.Inverse(paint.Transform);
            Matrix3X3<float> paintMat = Maths.TransformToMat3x3(invtransform);

            Matrix3X3<float> scissorMat;
            Vector2D<float> scissorPos, scissorScale;
            if (scissor.Extent.X < 0.5f || scissor.Extent.Y < 0.5f)
            {
                scissorMat = default;
                scissorPos = new(1.0f);
                scissorScale = new(1.0f);
            }
            else
            {
                invtransform = Maths.Inverse(scissor.Transform);
                scissorMat = Maths.TransformToMat3x3(invtransform);
                scissorPos = scissor.Extent;
                scissorScale = new(
                    MathF.Sqrt(scissor.Transform.M11 * scissor.Transform.M11 + scissor.Transform.M21 * scissor.Transform.M21) / fringe,
                    MathF.Sqrt(scissor.Transform.M12 * scissor.Transform.M12 + scissor.Transform.M22 * scissor.Transform.M22) / fringe
                );
            }

            if (paint.Image != 0)
            {
                // TODO: Implement this!
                throw new Exception();
            }
            else
            {
                _shader.Start();
                _shader.LoadInt(UniformLoc.Type, (int)Shaders.ShaderType.Fillgrad);
                _shader.LoadVector(UniformLoc.ViewSize, _size);
                _shader.LoadMatrix(UniformLoc.ScissorMat, scissorMat);
                _shader.LoadVector(UniformLoc.ScissorExt, scissorPos);
                _shader.LoadVector(UniformLoc.ScissorScale, scissorScale);
                _shader.LoadMatrix(UniformLoc.PaintMat, paintMat);
                _shader.LoadVector(UniformLoc.Extent, paint.Extent);
                _shader.LoadFloat(UniformLoc.Radius, paint.Radius);
                _shader.LoadFloat(UniformLoc.Feather, paint.Feather);
                _shader.LoadColour(UniformLoc.InnerCol, innerCol);
                _shader.LoadColour(UniformLoc.OuterCol, outerCol);
                _shader.LoadFloat(UniformLoc.StrokeMult, (width * 0.5f + fringe * 0.5f) / fringe);
                CheckError("grad paint loc");
            }

            return true;
        }

        public void Viewport(Vector2D<float> size, float devicePixelRatio)
        {
            _size = size;
        }

        public void Flush() { }

        private static uint VertexCount(Path[] paths)
        {
            uint count = 0;
            foreach (Path path in paths)
            {
                count += (uint)path.Fill.Length;
                count += (uint)path.Stroke.Length;
            }
            return count;
        }

        private unsafe void UploadPaths(Path[] paths)
        {
            uint n = 0;
            foreach (Path path in paths)
            {
                if (path.Fill.Length > 0)
                {
                    fixed (Vertex* vertex = &path.Fill[0])
                    {
                        Gl.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)(n * sizeof(Vertex)), (uint)(path.Fill.Length * sizeof(Vertex)), vertex);
                    }
                    n += (uint)path.Fill.Length;
                }
                if (path.Stroke.Length > 0)
                {
                    fixed (Vertex* vertex = &path.Stroke[0])
                    {
                        Gl.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)(n * sizeof(Vertex)), (uint)(path.Stroke.Length * sizeof(Vertex)), vertex);
                    }
                    n += (uint)path.Stroke.Length;
                }
            }
        }

        private unsafe void Fill_ConvexRender(Path[] paths)
        {
            uint n = 0;
            foreach (Path path in paths)
            {
                uint offset = (uint)(n * sizeof(Vertex));
                Gl.VertexAttribPointer(0, 2, GLEnum.Float, false, (uint)(sizeof(Vertex)), (void*)offset);
                Gl.VertexAttribPointer(1, 2, GLEnum.Float, false, (uint)(sizeof(Vertex)), (void*)(offset + 2 * sizeof(float)));
                Gl.DrawArrays(PrimitiveType.TriangleFan, 0, (uint)path.Fill.Length);
                n += (uint)(path.Fill.Length * path.Stroke.Length);
            }
        }

        private unsafe void Fill_Antialias(Path[] paths)
        {
            Gl.StencilFunc(StencilFunction.Lequal, 0x00, 0xff);
            Gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

            uint n = 0;
            foreach (Path path in paths)
            {
                uint offset = (uint)((n + path.Fill.Length) * sizeof(Vertex));
                Gl.VertexAttribPointer(0, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)offset);
                Gl.VertexAttribPointer(1, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(offset + 2 * sizeof(float)));
                Gl.DrawArrays(PrimitiveType.TriangleStrip, 0, (uint)path.Stroke.Length);
                n += (uint)(path.Fill.Length + path.Stroke.Length);
            }
        }

        private unsafe void Fill_Stencil(Path[] paths)
        {
            uint n = 0;
            foreach (Path path in paths)
            {
                uint offset = (uint)(n * sizeof(Vertex));
                Gl.VertexAttribPointer(0, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)offset);
                Gl.DrawArrays(PrimitiveType.TriangleFan, 0, (uint)path.Fill.Length);
                n += (uint)(path.Fill.Length + path.Stroke.Length);
            }
        }

        private unsafe void Fill_RenderQuad(Vector4D<float> bounds)
        {
            float[] quadVerts =
            {
                    bounds.X, bounds.W,
                    bounds.Z, bounds.W,
                    bounds.Z, bounds.Y,
                    bounds.X, bounds.W,
                    bounds.Z, bounds.Y,
                    bounds.X, bounds.Y,
                };
            fixed (float* qv = quadVerts)
            {
                Gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (uint)(6 * 2 * sizeof(float)), qv);
            }
            Gl.VertexAttribPointer(0, 2, GLEnum.Float, false, (uint)(2 * sizeof(float)), (void*)0);
            Gl.VertexAttrib2(1, 0.5f, 0.5f);
            Gl.DrawArrays(PrimitiveType.TriangleFan, 0, 6);
        }

        public unsafe void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Vector4D<float> bounds, Path[] paths)
        {
            Blending.Blending.Blend(compositeOperation, Gl);

            Gl.BindVertexArray(_vertexArray);
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(VertexCount(paths) * sizeof(Vertex)), null, BufferUsageARB.StreamDraw);
            UploadPaths(paths);

            if (paths.Length == 1 && paths[0].Convex)
            {
                Gl.Enable(EnableCap.CullFace);

                Gl.EnableVertexAttribArray(0);
                Gl.EnableVertexAttribArray(1);
                SetupPaint(paint, scissor, fringe, fringe);

                Gl.Disable(EnableCap.CullFace);
                Fill_ConvexRender(paths);

                Gl.Enable(EnableCap.CullFace);

                if (EdgeAntiAlias)
                {
                    Fill_Antialias(paths);
                }

                _shader.Stop();
                Gl.DisableVertexAttribArray(0);
                Gl.DisableVertexAttribArray(1);
                Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
                Gl.BindVertexArray(0);
            }
            else
            {
                Gl.Enable(EnableCap.CullFace);

                Gl.BindVertexArray(_vertexArray);
                Gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);

                Gl.Disable(EnableCap.Blend);
                Gl.Enable(EnableCap.StencilTest);
                Gl.StencilMask(0xff);
                int op = ~0;
                Gl.StencilFunc(StencilFunction.Always, 0, (uint)op);
                Gl.ColorMask(false, false, false, false);

                _shader.Start();
                _shader.LoadInt(UniformLoc.Type, (int)Shaders.ShaderType.Simple);
                _shader.LoadVector(UniformLoc.ViewSize, _size);
                CheckError("fill solid loc");

                Gl.EnableVertexAttribArray(0);

                Gl.StencilOpSeparate(StencilFaceDirection.Front, StencilOp.Keep, StencilOp.Keep, StencilOp.IncrWrap);
                Gl.StencilOpSeparate(StencilFaceDirection.Back, StencilOp.Keep, StencilOp.Keep, StencilOp.DecrWrap);
                Gl.Disable(EnableCap.CullFace);

                Fill_Stencil(paths);

                Gl.Enable(EnableCap.CullFace);

                Gl.ColorMask(true, true, true, true);
                Gl.Enable(EnableCap.Blend);

                Gl.EnableVertexAttribArray(1);
                SetupPaint(paint, scissor, fringe, fringe);

                if (EdgeAntiAlias)
                {
                    Fill_Antialias(paths);
                }

                Gl.StencilFunc(StencilFunction.Notequal, 0x0, 0xff);
                Gl.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);

                Gl.DisableVertexAttribArray(1);

                Fill_RenderQuad(bounds);

                _shader.Stop();

                Gl.DisableVertexAttribArray(0);

                Gl.Disable(EnableCap.StencilTest);
            }
        }

        private unsafe void Stroke_Render(Path[] paths)
        {
            uint n = 0;
            foreach (Path path in paths)
            {
                uint offset = (uint)((n * path.Fill.Length) * sizeof(Vertex));
                Gl.VertexAttribPointer(0, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)offset);
                Gl.VertexAttribPointer(1, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(offset + 2 * sizeof(float)));
                Gl.DrawArrays(PrimitiveType.TriangleStrip, 0, (uint)path.Stroke.Length);
                n += (uint)(path.Fill.Length + path.Stroke.Length);
            }
        }

        private unsafe void Stroke_Stencil(Path[] paths)
        {
            uint n = 0;
            foreach (Path path in paths)
            {
                uint offset = (uint)(n * sizeof(Vertex));
                Gl.VertexAttribPointer(0, 1, GLEnum.Float, false, 0, (void*)offset);
                Gl.DrawArrays(PrimitiveType.TriangleStrip, 0, (uint)path.Stroke.Length);
                n += (uint)(path.Fill.Length + path.Stroke.Length);
            }
        }

        public unsafe void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float width, Path[] paths)
        {
            Blending.Blending.Blend(compositeOperation, Gl);

            if (StencilStrokes)
            {
                Gl.Enable(EnableCap.StencilTest);
                Gl.StencilMask(0xff);

                Gl.StencilFunc(StencilFunction.Equal, 0x0, 0xff);
                _shader.Start();
                _shader.LoadInt(UniformLoc.Type, (int)Shaders.ShaderType.Simple);
                _shader.LoadVector(UniformLoc.ViewSize, _size);
                CheckError("fill solid loc");

                Gl.EnableVertexAttribArray(0);
                Stroke_Stencil(paths);

                _shader.LoadInt(UniformLoc.Type, (int)Shaders.ShaderType.Simple);
                _shader.LoadVector(UniformLoc.ViewSize, _size);
                Gl.StencilFunc(StencilFunction.Equal, 0x00, 0xff);
                Gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                Stroke_Stencil(paths);

                Gl.ColorMask(false, false, false, false);
                Gl.StencilFunc(StencilFunction.Always, 0x0, 0xff);
                Gl.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
                CheckError("stroke fill 1");

            }
            else
            {
                SetupPaint(paint, scissor, width, fringe);

                Gl.Enable(EnableCap.CullFace);

                uint maxCount = VertexCount(paths);
                Gl.BindVertexArray(_vertexArray);
                Gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(maxCount * sizeof(Vertex)), null, BufferUsageARB.StreamDraw);
                UploadPaths(paths);

                Gl.EnableVertexAttribArray(0);
                Gl.EnableVertexAttribArray(1);

                Stroke_Render(paths);

                Gl.DisableVertexAttribArray(0);
                Gl.DisableVertexAttribArray(1);
            }

            _shader.Stop();
        }

    }
}
