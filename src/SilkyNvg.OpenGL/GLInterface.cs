using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Colouring;
using SilkyNvg.Common;
using SilkyNvg.OpenGL.Calls;
using SilkyNvg.OpenGL.Instructions;
using SilkyNvg.OpenGL.Shaders;
using SilkyNvg.OpenGL.Textures;
using SilkyNvg.OpenGL.VertexArray;
using System;
using System.Collections.Generic;
using Shader = SilkyNvg.OpenGL.Shaders.Shader;
using Texture = SilkyNvg.OpenGL.Textures.Texture;

namespace SilkyNvg.OpenGL
{
    internal sealed class GLInterface : IDisposable
    {

        private readonly Shader _shader;
        private readonly CallQueue _callQueue;

        private readonly List<Vertex> _vertices = new List<Vertex>();
        private readonly IDictionary<int, Texture> _textures = new Dictionary<int, Texture>();

        private readonly VAO _vao;

        private readonly GraphicsManager _graphicsManager;
        private readonly GL _gl;

        private Vector2D<float> _view;
        private RenderMeta _renderMeta;

        public LaunchParameters LaunchParameters => _graphicsManager.LaunchParameters;

        private void BindTexture(uint id)
        {
            if (_renderMeta.BondTexture != id)
            {
                _renderMeta.BondTexture = id;
                _gl.BindTexture(TextureTarget.Texture2D, id);
            }
        }

        private void DelTexture(int image)
        {
            var tex = _textures[image];
            _gl.DeleteTexture(tex.TextureId);
            _textures.Remove(image);
        }

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

        public unsafe int CreateTexture(TextureType type, int w, int h, uint imageFlags, byte[] data)
        {
            var flags = new TextureFlags(imageFlags);
            var tex = new Texture(_textures.Count + 1, _gl.GenTexture(), w, h, type, flags, _gl);
            _textures.Add(tex.Id, tex);
            BindTexture(tex.TextureId);

            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, tex.Width);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);

            if (type == TextureType.Rgba)
            {
                fixed (byte* pixels = data)
                {
                    _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, (uint)w, (uint)h, 0, GLEnum.Rgba, GLEnum.UnsignedByte, pixels);
                }
            }
            else
            {
                fixed (byte* pixels = data)
                {
                    _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Red, (uint)w, (uint)h, 0, GLEnum.Red, GLEnum.UnsignedByte, pixels);
                }
            }

            if (tex.ImageFlags.Mipmaps)
            {
                if (tex.ImageFlags.Nearest)
                {
                    _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
                }
                else
                {
                    _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                }
            }
            else
            {
                if (tex.ImageFlags.Nearest)
                {
                    _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                }
                else
                {
                    _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                }
            }

            if (tex.ImageFlags.Nearest)
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            }
            else
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            if (tex.ImageFlags.RepeatX)
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            }
            else
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            }

            if (tex.ImageFlags.RepeatY)
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
            else
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }

            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);

            if (tex.ImageFlags.Mipmaps)
            {
                _gl.GenerateMipmap(TextureTarget.Texture2D);
            }

            CheckError("create tex!");
            BindTexture(0);

            return tex.Id;
        }

        public void DeleteTexture(int image)
        {
            if (!_textures.ContainsKey(image))
                throw new InvalidOperationException("Image with handle " + image + " does not exist!");

            DelTexture(image);
        }

        public unsafe void UpdateTexture(int image, int x, int y, int w, int h, byte[] data)
        {
            var tex = _textures[image - 1];

            BindTexture(tex.TextureId);

            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, tex.Width);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, x);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, y);

            if (tex.Type == TextureType.Rgba)
            {
                fixed (byte* d = data)
                {
                    _gl.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, (uint)w, (uint)h, GLEnum.Rgba, PixelType.UnsignedByte, d);
                }
            }
            else
            {
                fixed (byte* d = data)
                {
                    _gl.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, (uint)w, (uint)h, GLEnum.Red, PixelType.UnsignedByte, d);
                }
            }

            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 4);

            _gl.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            _gl.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);

            BindTexture(0);
        }

        public Vector2D<int> GetTextureSize(int image)
        {
            var tex = _textures[image - 1];
            return new Vector2D<int>(tex.Width, tex.Height);
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

            if (paint.Image != 0)
            {
                var tex = _textures[paint.Image];

                if (tex.ImageFlags.FlipY)
                {
                    var m1 = Maths.TransformTranslate(new Matrix3X2<float>(), 0.0f, frag.Extent.Y * 0.5f);
                    m1 = Maths.TransformMultiply(m1, paint.XForm);
                    var m2 = Maths.TransformScale(new Matrix3X2<float>(), 1.0f, -1.0f);
                    m2 = Maths.TransformMultiply(m2, m1);
                    m1 = Maths.TransformTranslate(m1, 0.0f, -frag.Extent.Y * 0.5f);
                    m1 = Maths.TransformMultiply(m1, m2);
                    invxform = Maths.TransformInverse(m1);
                }
                else
                {
                    invxform = Maths.TransformInverse(paint.XForm);
                }
                frag.Type = (int)Shaders.ShaderType.Fillimg;

                if (tex.Type == TextureType.Rgba)
                {
                    frag.TextureType = tex.ImageFlags.Premult ? 0 : 1;
                }
                else
                {
                    frag.TextureType = 2;
                }
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

            if (image != 0)
            {
                var tex = _textures[image];
                BindTexture(tex.TextureId);
            }

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

                call = new FillCall(triangleOffset, triangleCount, paint.Image, blendFunc, frag, paths_);
            }
            else
            {
                var frag = ConvertPaint(new FragmentDataUniforms(), paint, scissor, fringe, fringe, -1.0f);

                call = new ConvexFillCall(triangleOffset, triangleCount, paint.Image, blendFunc, frag, paths_);
            }

            _callQueue.Add(call);
        }

        public void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, Core.Paths.Path[] paths)
        {
            Path[] paths_ = new Path[paths.Length];

            int offset = _vertices.Count;

            for (int i = 0; i < paths.Length; i++)
            {
                var copy = new Path();
                var path = paths[i];
                if (path.Stroke.Count > 0)
                {
                    copy.StrokeOffset = offset;
                    copy.StrokeCount = path.Stroke.Count;
                    _vertices.AddRange(path.Stroke);
                    offset += path.Stroke.Count;
                }
                paths_[i] = copy;
            }

            Call call;

            if (LaunchParameters.StencilStrokes)
            {
                var frag0 = ConvertPaint(new FragmentDataUniforms(), paint, scissor, strokeWidth, fringe, -1.0f);
                var frag1 = ConvertPaint(new FragmentDataUniforms(), paint, scissor, strokeWidth, fringe, 1.0f - 0.5f / 255.0f);
                call = new StrokeCall(paint.Image, new Blend(compositeOperation), frag0, paths_, frag1);
            }
            else
            {
                var frag0 = ConvertPaint(new FragmentDataUniforms(), paint, scissor, strokeWidth, fringe, -1.0f);

                call = new StrokeCall(paint.Image, new Blend(compositeOperation), frag0, paths_, new FragmentDataUniforms());
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