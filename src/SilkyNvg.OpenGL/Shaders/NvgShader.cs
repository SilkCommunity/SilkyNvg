using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.IO;

namespace SilkyNvg.OpenGL.Shaders
{
    internal class NvgShader
    {

        private readonly uint _programmeID;
        private readonly uint _vertexShaderID, _fragmentShaderID;
        private readonly GL _gl;
        private readonly IDictionary<string, int> _uniformVariables = new Dictionary<string, int>();

        public NvgShader(string name, bool edgeAA, GL gl)
        {
            _gl = gl;

            _programmeID = _gl.CreateProgram();
            _vertexShaderID = _gl.CreateShader(ShaderType.VertexShader);
            _fragmentShaderID = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(_vertexShaderID, GetShaderSource("Vertex"));
            _gl.ShaderSource(_fragmentShaderID, GetShaderSource("Fragment"));

            _gl.CompileShader(_vertexShaderID);
            _gl.GetShader(_vertexShaderID, ShaderParameterName.CompileStatus, out int status);
            if (status != (int)GLEnum.True)
            {
                DumpShaderError(_vertexShaderID, "VertexShader", _gl);
                throw new System.Exception("Failed to load vertex shader!");
            }

            _gl.CompileShader(_fragmentShaderID);
            _gl.GetShader(_vertexShaderID, ShaderParameterName.CompileStatus, out status);
            if (status != (int)GLEnum.True)
            {
                DumpShaderError(_fragmentShaderID, "FragmentShader", _gl);
                throw new System.Exception("Failed to load fragment shader!");
            }

            _gl.AttachShader(_programmeID, _vertexShaderID);
            _gl.AttachShader(_programmeID, _fragmentShaderID);

            _gl.BindAttribLocation(_programmeID, 0, "vertex");
            _gl.BindAttribLocation(_programmeID, 1, "textureCoord");

            _gl.LinkProgram(_programmeID);
            _gl.GetProgram(_programmeID, GLEnum.LinkStatus, out status);
            if (status != (int)GLEnum.True)
            {
                DumpProgrammeError(_programmeID, name, _gl);
                throw new System.Exception("Failed to link programme!");
            }

            Start();
            _uniformVariables.Add("useEdgeAA", _gl.GetUniformLocation(_programmeID, "useEdgeAA"));
            _gl.Uniform1(_uniformVariables["useEdgeAA"], edgeAA ? 1 : 0);
            Stop();

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
            _uniformVariables.Add("viewSize", _gl.GetUniformLocation(_programmeID, "viewSize"));
            _uniformVariables.Add("textureSampler", _gl.GetUniformLocation(_programmeID, "textureSampler"));
            _uniformVariables.Add("scissorMatrix", _gl.GetUniformLocation(_programmeID, "scissorMatrix"));
            _uniformVariables.Add("paintMatrix", _gl.GetUniformLocation(_programmeID, "paintMatrix"));
            _uniformVariables.Add("innerColour", _gl.GetUniformLocation(_programmeID, "innerColour"));
            _uniformVariables.Add("outerColour", _gl.GetUniformLocation(_programmeID, "outerColour"));
            _uniformVariables.Add("scissorExtent", _gl.GetUniformLocation(_programmeID, "scissorExtent"));
            _uniformVariables.Add("scissorScale", _gl.GetUniformLocation(_programmeID, "scissorScale"));
            _uniformVariables.Add("extent", _gl.GetUniformLocation(_programmeID, "extent"));
            _uniformVariables.Add("radius", _gl.GetUniformLocation(_programmeID, "radius"));
            _uniformVariables.Add("feather", _gl.GetUniformLocation(_programmeID, "feather"));
            _uniformVariables.Add("strokeMultiplier", _gl.GetUniformLocation(_programmeID, "strokeMultiplier"));
            _uniformVariables.Add("strokeThreshold", _gl.GetUniformLocation(_programmeID, "strokeThreshold"));
            _uniformVariables.Add("texType", _gl.GetUniformLocation(_programmeID, "texType"));
            _uniformVariables.Add("type", _gl.GetUniformLocation(_programmeID, "type"));
        }

        private static string GetShaderSource(string file)
        {
            return File.ReadAllText("Shaders/nvg" + file + "Shader.glsl");
        }

        private static void DumpShaderError(uint shader, string name, GL gl)
        {
            string infoLog = gl.GetShaderInfoLog(shader);
            System.Console.Error.WriteLine("Shader: " + name + " Message: " + infoLog);
        }

        private static void DumpProgrammeError(uint programme, string name, GL gl)
        {
            string infoLog = gl.GetProgramInfoLog(programme);
            System.Console.Error.WriteLine("Programme: " + name + " Message: "  + infoLog);
        }

    }
}
