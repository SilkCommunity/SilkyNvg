using Silk.NET.OpenGL.Legacy;
using SilkyNvg.Common;
using System;
using System.Numerics;
using Shader = SilkyNvg.Rendering.OpenGL.Legacy.Shaders.Shader;

namespace SilkyNvg.Rendering.OpenGL.Legacy
{
    public class LegacyOpenGLRenderer : INvgRenderer
    {

        private readonly GL _gl;

        private LaunchParameters _launchParameters;
        private Shader _shader;

        private Vector2 _viewSize;

        public LegacyOpenGLRenderer(GL gl)
        {
            _gl = gl;
        }

        private void CheckError(string str)
        {
            if (!_launchParameters.Debug)
            {
                return;
            }
            ErrorCode error = (ErrorCode)_gl.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine("Error " + error + " after " + str);
                return;
            }
        }

        public bool Create(LaunchParameters launchParameters)
        {
            _launchParameters = launchParameters;

            CheckError("init");

            if (_launchParameters.Antialias)
            {
                _shader = new Shader("shader", "#define EDGE_AA 1", _gl);
            }
            else
            {
                _shader = new Shader("shader", null, _gl);
            }

            if (_shader.Error)
            {
                return false;
            }

            CheckError("uniform locations");
            _shader.GetUniforms();

            // TODO: Dummy texture

            CheckError("create done!");
            _gl.Finish();

            return true;
        }

        public int CreateTexture()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateTexture()
        {
            throw new System.NotImplementedException();
        }

        public void DeleteTexture()
        {
            throw new System.NotImplementedException();
        }

        public Vector2 GetTextureSize()
        {
            throw new System.NotImplementedException();
        }

        public void Viewport(Vector2 viewSize, float _)
        {
            _viewSize = viewSize;
        }

        public void Cancel()
        {
            throw new System.NotImplementedException();
        }

        public void Flush()
        {
            throw new System.NotImplementedException();
        }

        public void Fill()
        {
            throw new System.NotImplementedException();
        }

        public void Stroke()
        {
            throw new System.NotImplementedException();
        }

        public void Triangles()
        {
            throw new System.NotImplementedException();
        }

        public void Delete()
        {
            throw new System.NotImplementedException();
        }

    }
}
