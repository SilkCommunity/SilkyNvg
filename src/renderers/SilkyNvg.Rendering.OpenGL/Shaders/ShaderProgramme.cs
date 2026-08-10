using System;
using System.IO;
using System.Reflection;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal abstract class ShaderProgramme : IDisposable
    {

        protected readonly uint ProgrammeId;
        protected readonly GL Gl;
        
        protected ShaderProgramme(GL gl, params (string, ShaderType)[] shaders)
        {
            Gl = gl;

            ProgrammeId = Gl.CreateProgram();
            
            Span<uint> shaderIds = stackalloc uint[shaders.Length];
            for (int i = 0; i < shaders.Length; i++)
            {
                string code = LoadShaderCode(shaders[i].Item1);
                ShaderType type = shaders[i].Item2;
                
                uint shaderId = Gl.CreateShader(type);
                Gl.ShaderSource(shaderId, code);
                Gl.CompileShader(shaderId);
                Gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out int compileStatus);
                if (compileStatus == 0)
                {
                    Gl.GetShaderInfoLog(shaderId, out string infoLog);
                    Console.Error.WriteLine(infoLog);
                    throw new Exception("Failed to compile shader, " + type);
                }
                
                Gl.AttachShader(ProgrammeId, shaderId);
                shaderIds[i] = shaderId;
            }
            
            Gl.LinkProgram(ProgrammeId);
            Gl.GetProgram(ProgrammeId, ProgramPropertyARB.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                Gl.GetProgramInfoLog(ProgrammeId, out string infoLog);
                Console.Error.WriteLine(infoLog);
                throw new Exception("Failed to link shader programme!");
            }

            foreach (var shaderId in shaderIds)
            {
                Gl.DetachShader(ProgrammeId, shaderId);
                Gl.DeleteShader(shaderId);
            }
        }

        internal void Start()
        {
            Gl.UseProgram(ProgrammeId);
        }

        internal void Stop()
        {
            Gl.UseProgram(0);
        }

        public void Dispose()
        {
            Stop();
            Gl.DeleteProgram(ProgrammeId);
        }

        internal static string LoadShaderCode(string shaderName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream =
                   assembly.GetManifestResourceStream("SilkyNvg.Rendering.OpenGL.Shaders.ShaderCode." + shaderName))
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