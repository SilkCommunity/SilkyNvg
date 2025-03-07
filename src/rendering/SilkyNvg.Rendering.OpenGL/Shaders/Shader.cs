﻿using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class Shader : IDisposable
    {

        private readonly uint _programmeID;
        private readonly uint _vertexShaderID, _fragmentShaderID;

        private readonly string _name;
        private readonly OpenGLRenderer _renderer;
        private readonly GL _gl;

        private readonly IDictionary<UniformLoc, int> _loc = new Dictionary<UniformLoc, int>();

        private uint _fragBuffer;
        private int _align;

        public UniformManager UniformManager { get; private set; }

        public int FragSize { get; private set; }

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

        public Shader(string name, bool edgeAA, OpenGLRenderer renderer)
        {
            _renderer = renderer;
            _gl = _renderer.Gl;

            _programmeID = _gl.CreateProgram();
            _vertexShaderID = _gl.CreateShader(Silk.NET.OpenGL.ShaderType.VertexShader);
            _fragmentShaderID = _gl.CreateShader(Silk.NET.OpenGL.ShaderType.FragmentShader);
            _gl.ShaderSource(_vertexShaderID, ShaderCode.VERTEX_SHADER_CODE);
            _gl.ShaderSource(_fragmentShaderID, edgeAA ? ShaderCode.FRAGMENT_SHADER_EDGE_AA_CODE : ShaderCode.FRAGMENT_SHADER_CODE);

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
            _loc.Add(UniformLoc.Tex, _gl.GetUniformLocation(_programmeID, "tex"));
            _loc.Add(UniformLoc.Frag, (int)_gl.GetUniformBlockIndex(_programmeID, "frag"));
        }

        public void BindUniformBlock()
        {
            _gl.UniformBlockBinding(_programmeID, (uint)_loc[UniformLoc.Frag], (uint)UniformBindings.FragBinding);
            _fragBuffer = _gl.GenBuffer();
            _gl.GetInteger(GetPName.UniformBufferOffsetAlignment, out _align);
            FragSize = Marshal.SizeOf(typeof(FragUniforms)) + _align - (Marshal.SizeOf(typeof(FragUniforms)) % _align);
            UniformManager = new UniformManager(FragSize);
        }

        public void BindUniformBuffer()
        {
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, _fragBuffer);
        }

        public void UploadUniformData()
        {
            BindUniformBuffer();
            _gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)(UniformManager.CurrentsOffset * FragSize), UniformManager.Uniforms, BufferUsageARB.StreamDraw);
        }

        public void SetUniforms(int uniformOffset, int image)
        {
            _gl.BindBufferRange(BufferTargetARB.UniformBuffer, (uint)UniformBindings.FragBinding, _fragBuffer, uniformOffset, (nuint)Marshal.SizeOf(typeof(FragUniforms)));

            int id = _renderer.DummyTex;
            if (image != 0)
            {
                id = image;
            }
            ref var tex = ref _renderer.TextureManager.FindTexture(id);
            if (tex.Id != 0)
            {
                tex.Bind();
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

        public void LoadFloat(UniformLoc loc, float value)
        {
            _gl.Uniform1(_loc[loc], value);
        }

        public void LoadInt(UniformLoc loc, int value)
        {
            _gl.Uniform1(_loc[loc], value);
        }

        public void LoadVector(UniformLoc loc, Vector2 value)
        {
            _gl.Uniform2(_loc[loc], value.X, value.Y);
        }

        public unsafe void LoadColour(UniformLoc loc, Colour colour)
        {
            _gl.Uniform4(_loc[loc], 1, (float*)&colour);
        }

        public unsafe void LoadMatrix(UniformLoc loc, Matrix4x4 value)
        {
            _gl.UniformMatrix3x4(_loc[loc], 1, false, (float*)&value);
        }

    }
}