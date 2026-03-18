using System;
using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;
using SilkyNvg.Rendering.OpenGL.Fragments;
using SilkyNvg.Rendering.OpenGL.Shaders;

namespace SilkyNvg.Rendering.OpenGL
{
    public sealed class OpenGLRenderer : ISilkyRenderer
    {

        private readonly Buffer<Vector2> _vertexBuffer;
        private readonly Buffer<CommandData> _commandBuffer;
        private readonly Buffer<SubpathData>  _subpathBuffer;
        private readonly Buffer<PathData> _pathBuffer;

        // Stores: 32 bits of floating point intersection time | ID of associated path taken from CommandData struct
        private readonly Buffer<ulong> _intersectionsBuffer;
        private readonly Buffer<FragmentData> _fragmentBuffer;
        
        private readonly IntersectionFindingShader _intersectionFindingShader;
        private readonly FragmentGenerationShader _fragmentGenerationShader;
        
        private readonly GL _gl;

        private RenderTolerances _tolerances;

        private uint _commandCount = 0;
        private uint _subpathCount = 0;
        private uint _pathCount = 0;

        private uint _intersectionsCount = 0;
        
        public OpenGLRenderer(GL gl)
        {
            _gl = gl;
            
            _intersectionFindingShader = new IntersectionFindingShader(gl);
            _fragmentGenerationShader = new FragmentGenerationShader(gl);

            _vertexBuffer = new Buffer<Vector2>(0, 128, BufferUsageARB.DynamicDraw, _gl);
            _commandBuffer = new Buffer<CommandData>(1, 16, BufferUsageARB.DynamicDraw, _gl);
            _subpathBuffer = new Buffer<SubpathData>(2, 16, BufferUsageARB.DynamicDraw, _gl);
            _pathBuffer = new Buffer<PathData>(3, 16, BufferUsageARB.DynamicDraw, _gl);

            _intersectionsBuffer = new Buffer<ulong>(4, 128, BufferUsageARB.StreamCopy, _gl);
            _fragmentBuffer = new Buffer<FragmentData>(5, 128, BufferUsageARB.StreamCopy, _gl);
        }

        public void Init(RenderTolerances tolerances)
        {
            _tolerances = tolerances;
        }

        public void Resize(uint newWidth, uint newHeight)
        {
            _fragmentGenerationShader.Start();
            _fragmentGenerationShader.LoadScreenDimensions(newWidth, newHeight);
            _fragmentGenerationShader.Stop();
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

            _intersectionsCount += frame.IntersectionsCount;
        }

        public void Render()
        {
            // ---------- CALCULATE INTERSECTION TIMES ---------- //
            _intersectionFindingShader.Start();
            
            _intersectionFindingShader.LoadFpTol(_tolerances.FloatingPointTol);

            _intersectionsBuffer.EnsureCapacity(_intersectionsCount);
            _fragmentBuffer.EnsureCapacity(_intersectionsCount);
            
            uint intersectionFindingWorkgroupCount = _commandCount / 32 + 1;
            _gl.DispatchCompute(intersectionFindingWorkgroupCount, 1, 1);
            _gl.MemoryBarrier(MemoryBarrierMask.ShaderStorageBarrierBit);

            _intersectionFindingShader.Stop();

            // ---------- GENERATE BOUNDARY FRAGMENTS ---------- //
            _fragmentGenerationShader.Start();
            
            _fragmentGenerationShader.LoadIntersectionCount(_intersectionsCount);

            uint fragmentGenerationWorkgroupCount = _intersectionsCount / 32 + 1; // Actually a few less, but this way we can use ThreadID to index times buffer
            _gl.DispatchCompute(fragmentGenerationWorkgroupCount, 1, 1);
            _gl.MemoryBarrier(MemoryBarrierMask.ShaderStorageBarrierBit);
            
            var outputData = new FragmentData[_fragmentBuffer.Capacity];
            _fragmentBuffer.Read(outputData);
            
            _fragmentGenerationShader.Stop();
        }

        public void Dispose()
        {
            _intersectionsBuffer.Dispose();
            
            _vertexBuffer.Dispose();
            _commandBuffer.Dispose();
            _subpathBuffer.Dispose();
            _pathBuffer.Dispose();
            
            _intersectionFindingShader.Dispose();
            _fragmentGenerationShader.Dispose();
        }
        
    }
}