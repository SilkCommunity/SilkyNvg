using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class Shader : IDisposable
    {

        private readonly uint _programmeID;
        private readonly uint _vertexShaderID, _fragmentShaderID;

        private readonly string _name;
        private readonly GL _gl;

        private readonly IDictionary<UniformLoc, int> _loc = new Dictionary<UniformLoc, int>();

        public bool Status { get; }

        private void DumpShaderError(uint shader, Silk.NET.OpenGL.ShaderType type)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            Console.Error.WriteLine("Shader: " + _name + " / " + type + Environment.NewLine + "Error: " + infoLog);
        }

        private void DumpProgrammeError()
        {
            string infoLog = _gl.GetProgramInfoLog(_programmeID);
            Console.Error.WriteLine("Programme: " + _name + Environment.NewLine + "Error: " + infoLog);
        }

        public Shader(string name, string vertexShader, string fragmentShader, GL gl)
        {
            _gl = gl;

            _programmeID = _gl.CreateProgram();
            _vertexShaderID = _gl.CreateShader(Silk.NET.OpenGL.ShaderType.VertexShader);
            _fragmentShaderID = _gl.CreateShader(Silk.NET.OpenGL.ShaderType.FragmentShader);
            _gl.ShaderSource(_vertexShaderID, File.ReadAllText("./Shaders/nanovg_" + vertexShader + ".glsl"));
            _gl.ShaderSource(_fragmentShaderID, File.ReadAllText("./Shaders/nanovg_" + fragmentShader + ".glsl"));

            _gl.CompileShader(_vertexShaderID);
            _gl.GetShader(_vertexShaderID, ShaderParameterName.CompileStatus, out int status);
            if (status != (int)GLEnum.True)
            {
                DumpShaderError(_vertexShaderID, Silk.NET.OpenGL.ShaderType.VertexShader);
                Status = false;
                return;
            }

            _gl.CompileShader(_fragmentShaderID);
            _gl.GetShader(_fragmentShaderID, ShaderParameterName.CompileStatus, out status);
            if (status != (int)GLEnum.True)
            {
                DumpShaderError(_fragmentShaderID, Silk.NET.OpenGL.ShaderType.FragmentShader);
                Status = false;
                return;
            }

            _gl.AttachShader(_programmeID, _vertexShaderID);
            _gl.AttachShader(_programmeID, _fragmentShaderID);

            _gl.BindAttribLocation(_programmeID, 0, "vertex");
            _gl.BindAttribLocation(_programmeID, 1, "tcoord");
            _gl.BindAttribLocation(_programmeID, 2, "colour");

            _gl.LinkProgram(_programmeID);
            _gl.GetProgram(_programmeID, ProgramPropertyARB.LinkStatus, out status);
            if (status != (int)GLEnum.True)
            {
                DumpProgrammeError();
                Status = false;
                return;
            }

            Status = true;
        }

        public void Dispose()
        {
            Stop();

            if (_programmeID != 0)
                _gl.DeleteProgram(_programmeID);
            if (_vertexShaderID != 0)
                _gl.DeleteProgram(_vertexShaderID);
            if (_fragmentShaderID != 0)
                _gl.DeleteProgram(_fragmentShaderID);
        }

        public void GetUniforms()
        {
            _loc.Add(UniformLoc.ViewSize, _gl.GetUniformLocation(_programmeID, "viewSize"));
            _loc.Add(UniformLoc.ScissorMat, _gl.GetUniformLocation(_programmeID, "scissorMat"));
            _loc.Add(UniformLoc.ScissorExt, _gl.GetUniformLocation(_programmeID, "scissorExt"));
            _loc.Add(UniformLoc.ScissorScale, _gl.GetUniformLocation(_programmeID, "scissorScale"));
            _loc.Add(UniformLoc.PaintMat, _gl.GetUniformLocation(_programmeID, "paintMat"));
            _loc.Add(UniformLoc.Extent, _gl.GetUniformLocation(_programmeID, "extent"));
            _loc.Add(UniformLoc.Radius, _gl.GetUniformLocation(_programmeID, "radius"));
            _loc.Add(UniformLoc.Feather, _gl.GetUniformLocation(_programmeID, "feather"));
            _loc.Add(UniformLoc.InnerCol, _gl.GetUniformLocation(_programmeID, "innerCol"));
            _loc.Add(UniformLoc.OuterCol, _gl.GetUniformLocation(_programmeID, "outerCol"));
            _loc.Add(UniformLoc.StrokeMult, _gl.GetUniformLocation(_programmeID, "strokeMult"));
            _loc.Add(UniformLoc.StrokeThr, _gl.GetUniformLocation(_programmeID, "strokeThr"));
            _loc.Add(UniformLoc.Tex, _gl.GetUniformLocation(_programmeID, "tex"));
            _loc.Add(UniformLoc.TexType, _gl.GetUniformLocation(_programmeID, "texType"));
            _loc.Add(UniformLoc.Type, _gl.GetUniformLocation(_programmeID, "type"));
        }

        public void Start()
        {
            _gl.UseProgram(_programmeID);
        }

        public void Stop()
        {
            _gl.UseProgram(0);
        }

        public void LoadFloat(UniformLoc loc, float value)
        {
            _gl.Uniform1(_loc[loc], value);
        }

        public void LoadInt(UniformLoc loc, int value)
        {
            _gl.Uniform1(_loc[loc], value);
        }

        public void LoadVector(UniformLoc loc, Vector2D<float> value)
        {
            _gl.Uniform2(_loc[loc], value.X, value.Y);
        }

        public unsafe void LoadColour(UniformLoc loc, Colour colour)
        {
            _gl.Uniform4(_loc[loc], 1, (float*)&colour);
        }

        public unsafe void LoadMatrix(UniformLoc loc, Matrix3X4<float> value)
        {
            _gl.UniformMatrix3x4(_loc[loc], 1, false, (float*)&value);
        }

    }
}
