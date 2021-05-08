using Silk.NET.OpenGL.Legacy;
using System;
using System.Collections.Generic;
using System.IO;

namespace SilkyNvg.Rendering.OpenGL.Legacy.Shaders
{
    internal class Shader
    {

        private const string VERSION = "#version 120";

        private readonly uint _programmeID;
        private readonly uint _vertexID, _fragmentID;
        private readonly GL _gl;

        private readonly bool _error;
        public bool Error => _error;

        private readonly IDictionary<UniformLoc, int> _uniformLocations = new Dictionary<UniformLoc, int>();

        public Shader(string name, string opts, GL gl)
        {
            _gl = gl;

            string str = VERSION + Environment.NewLine + opts + Environment.NewLine;

            _programmeID = _gl.CreateProgram();
            _vertexID = _gl.CreateShader(ShaderType.VertexShader);
            _fragmentID = _gl.CreateShader(ShaderType.FragmentShader);

            string vshader = str + File.ReadAllText("./Shaders/nvgVertexShader.glsl");
            _gl.ShaderSource(_vertexID, vshader);
            string fshader = str + File.ReadAllText("./Shaders/nvgFragmentShader.glsl");
            _gl.ShaderSource(_fragmentID, fshader);

            _gl.CompileShader(_vertexID);
            _gl.GetShader(_vertexID, ShaderParameterName.CompileStatus, out int status);
            if (status != (int)GLEnum.True)
            {
                DumpShaderError(_vertexID, name, ShaderType.VertexShader);
                _error = true;
            }

            _gl.CompileShader(_fragmentID);
            _gl.GetShader(_fragmentID, ShaderParameterName.CompileStatus, out status);
            if (status != (int)GLEnum.True)
            {
                DumpShaderError(_fragmentID, name, ShaderType.FragmentShader);
                _error = true;
            }

            _gl.AttachShader(_programmeID, _vertexID);
            _gl.AttachShader(_programmeID, _fragmentID);

            _gl.BindAttribLocation(_programmeID, 0, "vertex");
            _gl.BindAttribLocation(_programmeID, 1, "tcoord");

            _gl.LinkProgram(_programmeID);
            _gl.GetProgram(_programmeID, ProgramPropertyARB.LinkStatus, out status);
            if (status != (int)GLEnum.True)
            {
                DumpProgrammeError(name);
                _error = true;
            }
        }

        private void DumpProgrammeError(string name)
        {
            string infoLog = _gl.GetProgramInfoLog(_programmeID);
            Console.Error.WriteLine("Shader: " + name + Environment.NewLine + "Error:" + Environment.NewLine + infoLog);
        }

        private void DumpShaderError(uint shader, string name, ShaderType type)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            Console.Error.WriteLine("Shader: " + name + " " + type + Environment.NewLine + "Error:" + Environment.NewLine + infoLog);
        }

        public void GetUniforms()
        {
            _uniformLocations.Add(UniformLoc.ViewSize, _gl.GetUniformLocation(_programmeID, "viewSize"));
            _uniformLocations.Add(UniformLoc.Tex, _gl.GetUniformLocation(_programmeID, "tex"));

            _uniformLocations.Add(UniformLoc.ScissorMat, _gl.GetUniformLocation(_programmeID, "scissorMat"));
            _uniformLocations.Add(UniformLoc.PaintMat, _gl.GetUniformLocation(_programmeID, "paintMat"));
            _uniformLocations.Add(UniformLoc.InnerCol, _gl.GetUniformLocation(_programmeID, "innerCol"));
            _uniformLocations.Add(UniformLoc.OuterCol, _gl.GetUniformLocation(_programmeID, "outerCol"));
            _uniformLocations.Add(UniformLoc.ScissorExt, _gl.GetUniformLocation(_programmeID, "scissorExt"));
            _uniformLocations.Add(UniformLoc.ScissorScale, _gl.GetUniformLocation(_programmeID, "scissorScale"));
            _uniformLocations.Add(UniformLoc.Extent, _gl.GetUniformLocation(_programmeID, "extent"));
            _uniformLocations.Add(UniformLoc.Radius, _gl.GetUniformLocation(_programmeID, "radius"));
            _uniformLocations.Add(UniformLoc.Feather, _gl.GetUniformLocation(_programmeID, "feather"));
            _uniformLocations.Add(UniformLoc.StrokeMult, _gl.GetUniformLocation(_programmeID, "strokeMult"));
            _uniformLocations.Add(UniformLoc.StrokeThr, _gl.GetUniformLocation(_programmeID, "strokeThr"));
            _uniformLocations.Add(UniformLoc.TexType, _gl.GetUniformLocation(_programmeID, "texType"));
            _uniformLocations.Add(UniformLoc.Type, _gl.GetUniformLocation(_programmeID, "type"));
        }

    }
}
