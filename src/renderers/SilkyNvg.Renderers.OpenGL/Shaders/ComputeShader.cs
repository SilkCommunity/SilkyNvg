using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Resources;
using Silk.NET.OpenGL;

namespace SilkyNvg.Renderers.OpenGL.Shaders
{
    internal class ComputeShader : IDisposable
    {

        private readonly uint _programmeID, _renderProgrammeID;
        private readonly uint _textureID;

        private readonly int _timeUniformLocation;
        
        private readonly GL _gl;
        
        internal uint TextureID => _textureID;
        
        internal unsafe ComputeShader(GL gl)
        {
            _gl = gl;

            string shaderSource = LoadShaderCode("shader.comp.glsl");
            
            uint computeShaderID = _gl.CreateShader(ShaderType.ComputeShader);
            _gl.ShaderSource(computeShaderID, shaderSource);
            _gl.CompileShader(computeShaderID);

            _gl.GetShader(computeShaderID, ShaderParameterName.CompileStatus, out int success);
            if (success == 0)
            {
                Console.WriteLine("COMPUTE ERROR");
            }
            
            _programmeID = _gl.CreateProgram();
            _gl.AttachShader(_programmeID, computeShaderID);
            _gl.LinkProgram(_programmeID);

            _gl.GetProgram(_programmeID, ProgramPropertyARB.LinkStatus, out success);
            if (success == 0)
            {
                Console.WriteLine("COMPUTE COMPUTE LINK ERROR");
            }
            
            _gl.DeleteShader(computeShaderID);
            
            string vertexShaderSource = LoadShaderCode("debug.vert.glsl");
            string fragmentShaderSource = LoadShaderCode("debug.frag.glsl");
            
            uint vertexShaderID = _gl.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vertexShaderID, vertexShaderSource);
            _gl.CompileShader(vertexShaderID);

            _gl.GetShader(vertexShaderID, ShaderParameterName.CompileStatus, out success);
            if (success == 0)
            {
                Console.WriteLine("COMPUTE VERTEX ERROR");
                _gl.GetShaderInfoLog(vertexShaderID, out string infoLog);
                Console.WriteLine(infoLog);
            }
            
            uint fragmentShaderID = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fragmentShaderID, fragmentShaderSource);
            _gl.CompileShader(fragmentShaderID);

            _gl.GetShader(fragmentShaderID, ShaderParameterName.CompileStatus, out success);
            if (success == 0)
            {
                Console.WriteLine("COMPUTE FRAGMENT ERROR");
            }
            
            _renderProgrammeID = _gl.CreateProgram();
            _gl.AttachShader(_renderProgrammeID, vertexShaderID);
            _gl.AttachShader(_renderProgrammeID, fragmentShaderID);
            _gl.LinkProgram(_renderProgrammeID);

            _gl.GetProgram(_renderProgrammeID, ProgramPropertyARB.LinkStatus, out success);
            if (success == 0)
            {
                Console.WriteLine("COMPUTE RENDER LINK ERROR");
            }
            
            _gl.DeleteShader(vertexShaderID);
            _gl.DeleteShader(fragmentShaderID);
            
            _textureID = _gl.GenTexture();
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _textureID);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba32f, (uint)512, (uint)512, 0, PixelFormat.Rgba,
                PixelType.Float, null);
            _gl.BindImageTexture(0, _textureID, 0, false, 0, BufferAccessARB.ReadWrite,
                InternalFormat.Rgba32f);

            _timeUniformLocation = _gl.GetUniformLocation(_programmeID, "t");
        }

        internal void StartCompute()
        {
            _gl.UseProgram(_programmeID);
        }

        internal void StopCompute()
        {
            _gl.UseProgram(0);
        }

        internal void LoadTime(float t)
        {
            _gl.Uniform1(_timeUniformLocation, t);
        }

        internal void StartRender()
        {
            _gl.UseProgram(_renderProgrammeID);
        }

        internal void StopRender()
        {
            _gl.UseProgram(0);
        }

        public void Dispose()
        {
            StopCompute();
            _gl.DeleteProgram(_programmeID);
            StopRender();
            _gl.DeleteProgram(_renderProgrammeID);
        }

        private static string LoadShaderCode(string shaderName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("SilkyNvg.Renderers.OpenGL.Shaders." + shaderName))
            {
                if (stream == null)
                {
                    throw new FileLoadException("Failed to load shader: " + shaderName);
                }

                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
        
    }
}