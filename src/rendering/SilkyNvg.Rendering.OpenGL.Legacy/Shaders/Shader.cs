using Silk.NET.Maths;
using Silk.NET.OpenGL.Legacy;
using System;
using System.Collections.Generic;
using System.IO;

namespace SilkyNvg.Rendering.OpenGL.Legacy.Shaders
{
    internal class Shader : IDisposable
    {

        private readonly IDictionary<UniformLoc, int> _uniformLocations = new Dictionary<UniformLoc, int>();

        private readonly uint _programmeID;
        private readonly uint _vertexShaderID, _fragmentShaderID;

        private readonly string _name;

        private readonly GL _gl;

        public bool ErrorStatus { get; private set; }

        private void DumpShaderError(uint shader, Silk.NET.OpenGL.Legacy.ShaderType type)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            Console.Error.WriteLine(
                "Shader: " + _name + Environment.NewLine
                + "Type:" + type.ToString() + Environment.NewLine
                + "Error: " + infoLog);
        }

        private void DumpProgrammeError()
        {
            string infoLog = _gl.GetProgramInfoLog(_programmeID);
            Console.Error.WriteLine(
                "Programme: " + _name + Environment.NewLine
                + "Error: " + infoLog);
        }

        public Shader(string name, bool antialias, GL gl)
        {
            _name = name;
            _gl = gl;

            string header;

            if (antialias)
            {
                header = "#version 120" + Environment.NewLine + "#define EDGE_AA 1" + Environment.NewLine;
            }
            else
            {
                header = "#version 120" + Environment.NewLine;
            }

            _programmeID = _gl.CreateProgram();
            _vertexShaderID = _gl.CreateShader(Silk.NET.OpenGL.Legacy.ShaderType.VertexShader);
            _fragmentShaderID = _gl.CreateShader(Silk.NET.OpenGL.Legacy.ShaderType.FragmentShader);

            string vertexSource = header + File.ReadAllText("./Shaders/fillVertexShader.glsl");
            _gl.ShaderSource(_vertexShaderID, vertexSource);
            string fragmentSource = header + File.ReadAllText("./Shaders/fillFragmentShader.glsl");
            _gl.ShaderSource(_fragmentShaderID, fragmentSource);

            _gl.CompileShader(_vertexShaderID);
            string infoLog = _gl.GetShaderInfoLog(_vertexShaderID);
            if (!string.IsNullOrEmpty(infoLog))
            {
                DumpShaderError(_vertexShaderID, Silk.NET.OpenGL.Legacy.ShaderType.VertexShader);
                ErrorStatus = false;
                return;
            }

            _gl.CompileShader(_fragmentShaderID);
            infoLog = _gl.GetShaderInfoLog(_fragmentShaderID);
            if (!string.IsNullOrEmpty(infoLog))
            {
                DumpShaderError(_fragmentShaderID, Silk.NET.OpenGL.Legacy.ShaderType.FragmentShader);
                ErrorStatus = false;
                return;
            }

            _gl.AttachShader(_programmeID, _vertexShaderID);
            _gl.AttachShader(_programmeID, _fragmentShaderID);

            _gl.BindAttribLocation(_programmeID, 0, "vertex");
            _gl.BindAttribLocation(_programmeID, 1, "tcoord");

            _gl.LinkProgram(_programmeID);
            infoLog = _gl.GetProgramInfoLog(_programmeID);
            if (!string.IsNullOrEmpty(infoLog))
            {
                DumpProgrammeError();
                ErrorStatus = false;
                return;
            }

            ErrorStatus = true;
        }

        public void Dispose()
        {
            _gl.DeleteProgram(_programmeID);
            _gl.DeleteShader(_vertexShaderID);
            _gl.DeleteShader(_fragmentShaderID);
        }

        public void Start()
        {
            _gl.UseProgram(_programmeID);
        }

        public void Stop()
        {
            _gl.UseProgram(0);
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

        public void LoadInt(UniformLoc loc, int value)
        {
            _gl.Uniform1(_uniformLocations[loc], value);
        }

        public void LoadFloat(UniformLoc loc, float value)
        {
            _gl.Uniform1(_uniformLocations[loc], value);
        }

        public unsafe void LoadVector(UniformLoc loc, Vector2D<float> value)
        {
            _gl.Uniform2(_uniformLocations[loc], 2, (float*)&value);
        }

        public unsafe void LoadColour(UniformLoc loc, Colour colour)
        {
            _gl.Uniform4(_uniformLocations[loc], 4, (float*)&colour);
        }

        public unsafe void LoadMatrix(UniformLoc loc, Matrix3X4<float> value)
        {
            _gl.UniformMatrix3x4(_uniformLocations[loc], 12, false, (float*)&value);
        }

        public void LoadMeta(Vector2D<float> viewSize, int tex)
        {
            LoadVector(UniformLoc.ViewSize, viewSize);
            LoadInt(UniformLoc.Tex, tex);
        }

    }
}
