using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Utils;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal abstract class ShaderProgramme : IDisposable
    {

        private readonly Dictionary<string, int> _uniformLocations = new();
        
        protected readonly uint ProgrammeId;
        protected readonly GL Gl;
        
        protected ShaderProgramme(GL gl, params (string?, string, ShaderType)[] shaders)
        {
            Gl = gl;

            ProgrammeId = Gl.CreateProgram();
            
            Span<uint> shaderIds = stackalloc uint[shaders.Length];
            for (int i = 0; i < shaders.Length; i++)
            {
                string header = shaders[i].Item1 != null ? LoadShaderCode(shaders[i].Item1!) : "";
                string code = header + Environment.NewLine + LoadShaderCode(shaders[i].Item2);
                ShaderType type = shaders[i].Item3;
                
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

        private int GetUniformLocation(string uniformName)
        {
            if (!_uniformLocations.TryGetValue(uniformName, out var location))
            {
                location = Gl.GetUniformLocation(ProgrammeId, uniformName);
                Errors.CheckGLError($"Get UniformLocation: \"{uniformName}\"", Gl);
                _uniformLocations.Add(uniformName, location);
            }

            return location;
        }

        internal void LoadInt(int value, string uniformName)
        {
            Gl.Uniform1(GetUniformLocation(uniformName), value);
            Errors.CheckGLError($"load int ({uniformName})", Gl);
        }

        internal void LoadUInt(uint value, string uniformName)
        {
            Gl.Uniform1(GetUniformLocation(uniformName), value);
            Errors.CheckGLError($"load uint ({uniformName})", Gl);
        }

        internal void LoadFloat(float value, string uniformName)
        {
            Gl.Uniform1(GetUniformLocation(uniformName), value);
            Errors.CheckGLError($"load float ({uniformName})", Gl);
        }

        internal void LoadVector2(float x, float y, string uniformName)
        {
            Gl.Uniform2(GetUniformLocation(uniformName), x, y);
            Errors.CheckGLError($"load vec2 ({uniformName})", Gl);
        }
        
        internal void LoadVector2(Vector2 v, string uniformName)
            => LoadVector2(v.X, v.Y, uniformName);

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