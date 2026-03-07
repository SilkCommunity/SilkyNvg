using System;
using System.IO;
using System.Reflection;
using System.Resources;
using Silk.NET.OpenGL;

namespace SilkyNvg.Renderers.OpenGL.Shaders
{
    internal class SilkyShader : IDisposable
    {

        private readonly uint _programmeID;
        
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
        }

        internal void Start()
        {
            _gl.UseProgram(_programmeID);
        }

        internal void Stop()
        {
            _gl.UseProgram(0);
        }

        public void Dispose()
        {
            
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