using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;

namespace SilkyNvg.OpenGL.Shaders
{
    internal class Shader
    {

        private static readonly string SHADER_HEADER = "#version 150 core" + Environment.NewLine;
        private static readonly string DEFINE_EDGE_AA = "#define EDGE_AA 1" + Environment.NewLine;

        private readonly IDictionary<UniformLocations, int> _locations;
        private readonly GL _gl;

        private uint _programmeID;
        private uint _vertexShaderID, _fragmentShaderID;
        private int _fragmentDataSize;

        public int UniformSize
        {
            get => _fragmentDataSize;
            set => _fragmentDataSize = value;
        }

        public Shader(string name, bool aa, GL gl)
        {
            _gl = gl;
            _locations = new Dictionary<UniformLocations, int>();
            CreateShader(name, aa);
        }

        private void CreateShader(string name, bool aa)
        {
            string vshader = File.ReadAllText("Shaders/vertexShader.glsl");
            vshader = vshader.Insert(0, SHADER_HEADER);
            string fshader = File.ReadAllText("Shaders/fragmentShader.glsl");
            if (aa)
                fshader = fshader.Insert(0, DEFINE_EDGE_AA);
            fshader = fshader.Insert(0, SHADER_HEADER);

            _programmeID = _gl.CreateProgram();
            _vertexShaderID = _gl.CreateShader(Silk.NET.OpenGL.ShaderType.VertexShader);
            _fragmentShaderID = _gl.CreateShader(Silk.NET.OpenGL.ShaderType.FragmentShader);
            _gl.ShaderSource(_vertexShaderID, vshader);
            _gl.ShaderSource(_fragmentShaderID, fshader);

            _gl.CompileShader(_vertexShaderID);
            _gl.GetShader(_vertexShaderID, ShaderParameterName.CompileStatus, out int stat);
            if (stat != (int)GLEnum.True)
            {
                DumpShaderError(_vertexShaderID, name, Silk.NET.OpenGL.ShaderType.VertexShader);
                Environment.Exit(-1);
            }

            _gl.CompileShader(_fragmentShaderID);
            _gl.GetShader(_fragmentShaderID, ShaderParameterName.CompileStatus, out stat);
            if (stat != (int)GLEnum.True)
            {
                DumpShaderError(_fragmentShaderID, name, Silk.NET.OpenGL.ShaderType.FragmentShader);
                Environment.Exit(-1);
            }

            _gl.AttachShader(_programmeID, _vertexShaderID);
            _gl.AttachShader(_programmeID, _fragmentShaderID);

            _gl.BindAttribLocation(_programmeID, 0, "vertexPosition");
            _gl.BindAttribLocation(_programmeID, 1, "textureCoord");

            _gl.LinkProgram(_programmeID);
            _gl.GetProgram(_programmeID, GLEnum.CompileStatus, out stat);
            if (stat != (int)GLEnum.True)
            {
                DumpProgrammeError(name);
                Environment.Exit(-1);
            }

        }

        private void DumpShaderError(uint shader, string name, Silk.NET.OpenGL.ShaderType type)
        {
            string info = _gl.GetShaderInfoLog(shader);
            Console.Error.WriteLine("Shader " + name + " - " + type + " Error: " + info);
        }

        private void DumpProgrammeError(string name)
        {
            string info = _gl.GetProgramInfoLog(_programmeID);
            Console.Error.WriteLine("Programme " + name + " Error: " + info);
        }

        public void GetUniforms()
        {
            _locations.Add(UniformLocations.Viewsize, _gl.GetUniformLocation(_programmeID, "viewSize"));
            _locations.Add(UniformLocations.Tex, _gl.GetUniformLocation(_programmeID, "texture"));

            _locations.Add(UniformLocations.FragmentData, _gl.GetUniformLocation(_programmeID, "fragmentData"));
        }

        public void BindBlock()
        {
            _gl.UniformBlockBinding(_programmeID, (uint)_locations[UniformLocations.FragmentData], (uint)UniformBindings.FragmentDataBinding);
        }

    }
}
