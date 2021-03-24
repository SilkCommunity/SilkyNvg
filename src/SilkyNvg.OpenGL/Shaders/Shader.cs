using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SilkyNvg.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace SilkyNvg.OpenGL.Shaders
{
    internal class Shader : IDisposable
    {

        private static readonly string SHADER_HEADER = "#version 150 core" + Environment.NewLine;
        private static readonly string DEFINE_EDGE_AA = "#define EDGE_AA 1" + Environment.NewLine;

        private readonly IDictionary<UniformLocations, int> _locations;
        private readonly GL _gl;

        private uint _programmeID;
        private uint _vertexShaderID, _fragmentShaderID;

        public Shader(string name, bool aa, GL gl)
        {
            _gl = gl;
            _locations = new Dictionary<UniformLocations, int>();
            CreateShader(name, aa);
            GetUniforms();
        }

        private unsafe void CreateShader(string name, bool aa)
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

        public void Start()
        {
            _gl.UseProgram(_programmeID);
        }

        public void Stop()
        {
            _gl.UseProgram(0);
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

        private void GetUniforms()
        {
            _locations.Add(UniformLocations.Viewsize, _gl.GetUniformLocation(_programmeID, "viewSize"));
            _locations.Add(UniformLocations.Tex, _gl.GetUniformLocation(_programmeID, "tex"));

            _locations.Add(UniformLocations.ScissorMat, _gl.GetUniformLocation(_programmeID, "scissorMat"));
            _locations.Add(UniformLocations.PaintMat, _gl.GetUniformLocation(_programmeID, "paintMat"));
            _locations.Add(UniformLocations.InnerCol, _gl.GetUniformLocation(_programmeID, "innerCol"));
            _locations.Add(UniformLocations.OuterCol, _gl.GetUniformLocation(_programmeID, "outerCol"));
            _locations.Add(UniformLocations.ScissorExt, _gl.GetUniformLocation(_programmeID, "scissorExt"));
            _locations.Add(UniformLocations.ScissorScale, _gl.GetUniformLocation(_programmeID, "scissorScale"));
            _locations.Add(UniformLocations.Extent, _gl.GetUniformLocation(_programmeID, "extent"));
            _locations.Add(UniformLocations.Radius, _gl.GetUniformLocation(_programmeID, "radius"));
            _locations.Add(UniformLocations.Feather, _gl.GetUniformLocation(_programmeID, "feather"));
            _locations.Add(UniformLocations.StrokeMult, _gl.GetUniformLocation(_programmeID, "strokeMult"));
            _locations.Add(UniformLocations.StrokeThr, _gl.GetUniformLocation(_programmeID, "strokeThr"));
            _locations.Add(UniformLocations.TexType, _gl.GetUniformLocation(_programmeID, "texType"));
            _locations.Add(UniformLocations.Type, _gl.GetUniformLocation(_programmeID, "type"));
        }

        private void LoadFloat(UniformLocations loc, float val)
        {
            _gl.Uniform1(_locations[loc], val);
        }

        private void LoadInt(UniformLocations loc, int val)
        {
            _gl.Uniform1(_locations[loc], val);
        }

        private void LoadVector(UniformLocations loc, Vector2D<float> val)
        {
            _gl.Uniform2(_locations[loc], val.X, val.Y);
        }

        private void LoadVector(UniformLocations loc, Vector4D<float> val)
        {
            _gl.Uniform4(_locations[loc], val.X, val.Y, val.Z, val.W);
        }

        private unsafe void LoadMatrix(UniformLocations loc, Matrix3X4<float> val)
        {
            _gl.UniformMatrix3x4(_locations[loc], 1, false, (float*)&val);
        }

        public void LoadMeta(int texture, Vector2D<float> view)
        {
            LoadInt(UniformLocations.Tex, texture);
            LoadVector(UniformLocations.Viewsize, view);
        }

        public void LoadUniforms(FragmentDataUniforms data)
        {
            LoadMatrix(UniformLocations.ScissorMat, data.ScissorMatrix);
            LoadMatrix(UniformLocations.PaintMat, data.PaintMatrix);
            LoadVector(UniformLocations.InnerCol, data.InnerColour);
            LoadVector(UniformLocations.OuterCol, data.OuterColour);
            LoadVector(UniformLocations.ScissorExt, data.ScissorExt);
            LoadVector(UniformLocations.ScissorScale, data.ScissorScale);
            LoadVector(UniformLocations.Extent, data.Extent);
            LoadFloat(UniformLocations.Radius, data.Radius);
            LoadFloat(UniformLocations.Feather, data.Feather);
            LoadFloat(UniformLocations.StrokeMult, data.StrokeMult);
            LoadFloat(UniformLocations.StrokeThr, data.StrokeThr);
            LoadInt(UniformLocations.TexType, data.TextureType);
            LoadInt(UniformLocations.Type, data.Type);
        }

        public void Dispose()
        {
            Stop();
            _gl.DeleteShader(_vertexShaderID);
            _gl.DeleteShader(_fragmentShaderID);
            _gl.DeleteProgram(_programmeID);
        }

    }
}