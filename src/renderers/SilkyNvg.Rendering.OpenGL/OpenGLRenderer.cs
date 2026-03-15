using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;
using SilkyNvg.Rendering.OpenGL.Shaders;

namespace SilkyNvg.Rendering.OpenGL
{
    public sealed class OpenGLRenderer : ISilkyRenderer
    {

        private readonly Buffer<uint> _inputBuffer0;
        private readonly Buffer<uint> _inputBuffer1;
        private readonly Buffer<uint> _outputBuffer;

        private readonly Buffer<Vector2> _vertexBuffer;
        private readonly Buffer<CommandData> _commandBuffer;
        private readonly Buffer<SubpathData>  _subpathBuffer;
        private readonly Buffer<PathData> _pathBuffer;
        
        private readonly FragmentGenerationComputeShader _fragmentGenerationShader;
        
        private readonly GL _gl;
        
        public OpenGLRenderer(GL gl)
        {
            _gl = gl;
            
            _fragmentGenerationShader = new FragmentGenerationComputeShader(gl);

            _inputBuffer0 = new Buffer<uint>(1, 10, BufferUsageARB.DynamicDraw, _gl);
            _inputBuffer1 = new Buffer<uint>(2, 10, BufferUsageARB.DynamicDraw, _gl);
            _outputBuffer = new Buffer<uint>(3, 10, BufferUsageARB.StreamCopy, _gl);

            _vertexBuffer = new Buffer<Vector2>(4, 128, BufferUsageARB.DynamicDraw, _gl);
            _commandBuffer = new Buffer<CommandData>(5, 16, BufferUsageARB.DynamicDraw, _gl);
            _subpathBuffer = new Buffer<SubpathData>(6, 16, BufferUsageARB.DynamicDraw, _gl);
            _pathBuffer = new Buffer<PathData>(7, 16, BufferUsageARB.DynamicDraw, _gl);
            
            _inputBuffer0.Update(new uint[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            _inputBuffer1.Update(new uint[10] { 41, 42, 43, 44, 45, 46, 47, 48, 49, 50 });
        }

        public void PrepareFrame()
        {
            
        }

        public void AddFrame(FrameContainer frame)
        {
            _vertexBuffer.Update(frame.Points, 0, frame.PointCount);
            _commandBuffer.Update(frame.Commands, 0, frame.CommandCount);
            _subpathBuffer.Update(frame.Subpaths, 0, frame.SubpathCount);
            _pathBuffer.Update(frame.Paths, 0, frame.PathCount);
            
        }

        public void Render()
        {
            _fragmentGenerationShader.Start();
            
            _fragmentGenerationShader.LoadFactor(4);
            
            _gl.DispatchCompute(10, 1, 1);
            _gl.MemoryBarrier(MemoryBarrierMask.AllBarrierBits);

            var outputData = new uint[10];
            _outputBuffer.Read(outputData);

            _fragmentGenerationShader.Stop();
        }

        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _commandBuffer.Dispose();
            _subpathBuffer.Dispose();
            _pathBuffer.Dispose();
            
            _inputBuffer0.Dispose();
            _inputBuffer1.Dispose();
            _outputBuffer.Dispose();
            _fragmentGenerationShader.Dispose();
        }
        
    }
}