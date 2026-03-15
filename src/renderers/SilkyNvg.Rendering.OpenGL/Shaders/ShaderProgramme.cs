using System;
using System.IO;
using System.Reflection;
using System.Resources;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal abstract class ShaderProgramme : IDisposable
    {
        
        protected readonly uint ProgrammeID;
        protected readonly GL Gl;
        
        internal ShaderProgramme(GL gl, params (string, ShaderType)[] shaders)
        {
            Gl = gl;
            
            ProgrammeID = Gl.CreateProgram();
            
            Span<uint> shaderIds = stackalloc uint[shaders.Length];
            for (int i = 0; i < shaders.Length; i++)
            {
                string code = shaders[i].Item1;
                ShaderType shaderType = shaders[i].Item2;

                uint shaderId = Gl.CreateShader(shaderType);
                Gl.ShaderSource(shaderId, code);
                Gl.CompileShader(shaderId);
                Gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out int compileStatus);
                if (compileStatus == 0)
                {
                    Gl.GetShaderInfoLog(shaderId, out string infoLog);
                    Console.Error.WriteLine(infoLog);
                    throw new Exception("Failed to compile shader, " + shaderType);
                }

                Gl.AttachShader(ProgrammeID, shaderId);

                shaderIds[i] = shaderId;
            }
            
            Gl.LinkProgram(ProgrammeID);
            Gl.GetProgram(ProgrammeID, ProgramPropertyARB.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                Gl.GetProgramInfoLog(ProgrammeID, out string infoLog);
                Console.Error.WriteLine(infoLog);
                throw new Exception("Failed to link shader");
            }

            for (int i = 0; i < shaderIds.Length; i++)
            {
                Gl.DeleteShader(shaderIds[i]);
            }
        }

        internal void Start()
        {
            Gl.UseProgram(ProgrammeID);
        }

        internal void Stop()
        {
            Gl.UseProgram(0);
        }

        public void Dispose()
        {
            Gl.DeleteProgram(ProgrammeID);
        }

        protected static string LoadShaderCode(string shaderName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("SilkyNvg.Rendering.OpenGL.Shaders.ShaderCode." + shaderName))
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