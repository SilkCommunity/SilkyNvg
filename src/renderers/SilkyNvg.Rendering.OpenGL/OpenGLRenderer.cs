using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;
using SilkyNvg.Rendering.OpenGL.Shaders;

namespace SilkyNvg.Rendering.OpenGL
{
    public sealed class OpenGLRenderer : ISilkyRenderer
    {

        private readonly Buffer<Vector2> _vertexBuffer;
        private readonly Buffer<CommandData> _commandBuffer;
        private readonly Buffer<SubpathData>  _subpathBuffer;
        private readonly Buffer<PathData> _pathBuffer;

        private readonly Buffer<float> _intersectionTimes;
        
        private readonly FragmentGenerationComputeShader _fragmentGenerationShader;
        
        private readonly GL _gl;

        private RenderTolerances _tolerances;

        private uint _commandCount = 0;
        private uint _subpathCount = 0;
        private uint _pathCount = 0;
        
        public OpenGLRenderer(GL gl)
        {
            _gl = gl;
            
            _fragmentGenerationShader = new FragmentGenerationComputeShader(gl);

            _vertexBuffer = new Buffer<Vector2>(0, 128, BufferUsageARB.DynamicDraw, _gl);
            _commandBuffer = new Buffer<CommandData>(1, 16, BufferUsageARB.DynamicDraw, _gl);
            _subpathBuffer = new Buffer<SubpathData>(2, 16, BufferUsageARB.DynamicDraw, _gl);
            _pathBuffer = new Buffer<PathData>(3, 16, BufferUsageARB.DynamicDraw, _gl);

            _intersectionTimes = new Buffer<float>(4, 128, BufferUsageARB.StreamCopy, _gl);
        }

        public void Init(RenderTolerances tolerances)
        {
            _tolerances = tolerances;
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

            _commandCount += frame.CommandCount;
            _subpathCount += frame.SubpathCount;
            _pathCount += frame.PathCount;
        }

        public void Render()
        {
            _fragmentGenerationShader.Start();
            
            _fragmentGenerationShader.LoadFpTol(_tolerances.FloatingPointTol);

            uint maxIntersections = _commandCount * (2 * 32 + 2);
            _intersectionTimes.EnsureCapacity(maxIntersections);    
            
            uint fragmentGenerationWorkgroupCount = _commandCount / 32 + 1;
            _gl.DispatchCompute(fragmentGenerationWorkgroupCount, 1, 1);
            _gl.MemoryBarrier(MemoryBarrierMask.AllBarrierBits);

            var outputData = new float[_intersectionTimes.Capacity];
            _intersectionTimes.Read(outputData);

            _fragmentGenerationShader.Stop();
        }

        public void Dispose()
        {
            _intersectionTimes.Dispose();
            
            _vertexBuffer.Dispose();
            _commandBuffer.Dispose();
            _subpathBuffer.Dispose();
            _pathBuffer.Dispose();
            
            _fragmentGenerationShader.Dispose();
        }
        
    }
}