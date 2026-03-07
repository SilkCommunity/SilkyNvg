using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Resources;
using Silk.NET.OpenGL;

namespace SilkyNvg.Renderers.OpenGL.Shaders
{
    internal class SilkyShader : IDisposable
    {

        private readonly uint _programmeID;

        private readonly int _viewExtentUniformLocation;
        
        private readonly GL _gl;
        
        internal SilkyShader(GL gl)
        {
            _gl = gl;

            string vertexSource = LoadShaderCode("shader.vert.glsl");
            string fragmentSource = LoadShaderCode("shader.frag.glsl");
            
            uint vertexShaderID = _gl.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vertexShaderID, vertexSource);
            _gl.CompileShader(vertexShaderID);

            _gl.GetShader(vertexShaderID, ShaderParameterName.CompileStatus, out int success);
            if (success == 0)
            {
                Console.WriteLine("VERTEX ERROR");
            }
            
            uint fragmentShaderID = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fragmentShaderID, fragmentSource);
            _gl.CompileShader(fragmentShaderID);

            _gl.GetShader(fragmentShaderID, ShaderParameterName.CompileStatus, out success);
            if (success == 0)
            {
                Console.WriteLine("FRAGMENT ERROR");
            }
            
            _programmeID = _gl.CreateProgram();
            _gl.AttachShader(_programmeID, vertexShaderID);
            _gl.AttachShader(_programmeID, fragmentShaderID);
            _gl.LinkProgram(_programmeID);

            _gl.GetProgram(_programmeID, ProgramPropertyARB.LinkStatus, out success);
            if (success == 0)
            {
                Console.WriteLine("LINK ERROR");
            }
            
            _gl.DeleteShader(vertexShaderID);
            _gl.DeleteShader(fragmentShaderID);

            _viewExtentUniformLocation = _gl.GetUniformLocation(_programmeID, "viewExtent");
        }

        internal void Start()
        {
            _gl.UseProgram(_programmeID);
        }

        internal void Stop()
        {
            _gl.UseProgram(0);
        }

        internal void LoadViewExtent(Vector2 viewExtent)
        {
            _gl.Uniform2(_viewExtentUniformLocation, viewExtent);
        }

        public void Dispose()
        {
            Stop();
            _gl.DeleteProgram(_programmeID);
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