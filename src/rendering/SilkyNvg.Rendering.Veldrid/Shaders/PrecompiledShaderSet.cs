using System;
using Veldrid;

namespace SilkyNvg.Rendering.Veldrid
{
    /// <summary>
    /// Static helper for creating Veldrid Shader objects from precompiled shader bytecode.
    /// Each generated shader class (e.g. SolidFill.generated.cs) calls CreateShaderPair()
    /// after selecting the correct bytes for the current graphics backend.
    /// </summary>
    internal static class PrecompiledShaderSet
    {
        /// <summary>
        /// Creates a vertex + fragment shader pair from precompiled bytes.
        /// The caller is responsible for selecting the correct bytes and entry points
        /// for the current GraphicsBackend.
        /// </summary>
        internal static Shader[] CreateShaderPair(
            ResourceFactory factory,
            byte[] vertexShaderBytes, string vertexEntryPoint,
            byte[] fragmentShaderBytes, string fragmentEntryPoint,
            string shaderSetName = "Unknown")
        {
            if (vertexShaderBytes == null) {
                throw new ArgumentNullException(nameof(vertexShaderBytes),
                    $"[{shaderSetName}] Vertex shader bytes are null for backend {factory.BackendType}");
            }
            if (fragmentShaderBytes == null) {
                throw new ArgumentNullException(nameof(fragmentShaderBytes),
                    $"[{shaderSetName}] Fragment shader bytes are null for backend {factory.BackendType}");
            }
            
            Console.WriteLine($"[SHADER:{shaderSetName}] Creating vertex shader: {vertexShaderBytes.Length} bytes, entry={vertexEntryPoint}");
            var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, vertexEntryPoint);
            Console.WriteLine($"[SHADER:{shaderSetName}] ShaderDescription created, bytes field is {(vertexShaderDesc.ShaderBytes == null ? "NULL" : $"{vertexShaderDesc.ShaderBytes.Length} bytes")}");
            var vertexShader = factory.CreateShader(ref vertexShaderDesc);
                
            Console.WriteLine($"[SHADER:{shaderSetName}] Creating fragment shader: {fragmentShaderBytes.Length} bytes, entry={fragmentEntryPoint}");
            var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, fragmentEntryPoint);
            Console.WriteLine($"[SHADER:{shaderSetName}] ShaderDescription created, bytes field is {(fragmentShaderDesc.ShaderBytes == null ? "NULL" : $"{fragmentShaderDesc.ShaderBytes.Length} bytes")}");
            var fragmentShader = factory.CreateShader(ref fragmentShaderDesc);
            return new[] { vertexShader, fragmentShader };
        }
    }
}