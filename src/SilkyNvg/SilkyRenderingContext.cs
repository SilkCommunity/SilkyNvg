using System;
using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.States;
using SilkyNvg.Utils;

namespace SilkyNvg
{
    public sealed class SilkyRenderingContext : IDisposable
    {
        
        private readonly SilkyRenderingContextSettings _settings;
        private readonly ISilkyRenderer _renderer;

        private readonly FrameContainer _frameContainer;
        
        private float _pixelRatio = 1.0f;

        internal RenderTolerances Tolerances { get; } = new RenderTolerances();

        internal StateStack StateStack { get; } = new StateStack();
        
        public SilkyRenderingContext(SilkyRenderingContextSettings settings, ISilkyRenderer renderer)
        {
            _settings = settings;
            _renderer = renderer;

            _frameContainer = new FrameContainer(Tolerances);
        }
        
        public SilkyRenderingContextSettings GetContextSettings()
            =>  _settings;
        
        #region Dimension
        
        public uint Width { get; set; } = 600;

        public uint Height { get; set; } = 400;

        public float PixelRatio
        {
            get => _pixelRatio;
            set
            {
                _pixelRatio = value;
                Tolerances.UpdateTols(_pixelRatio);
            }
        }
        
        public void SetWidth(int width) => Width = (uint)width;
        
        public void SetHeight(int height) => Height = (uint)height;

        public void Resize(int newWidth, int newHeight, float pixelRatio = 1.0f)
        {
            SetWidth(newWidth);
            SetHeight(newHeight);
            PixelRatio = pixelRatio;
        }
        
        #endregion

        #region State

        public void Save()
        {
            StateStack.PushState();
        }

        public void Restore()
        {
            StateStack.PopState();
        }

        public void Reset()
        {
            
        }

        public bool IsContextLost()
        {
            return false;
        }
        
        #endregion

        public void FillPath(Path path)
        {
            _frameContainer.AddPath(path);
        }

        public void BeginFrame()
        {
            _frameContainer.Clear();
        }

        public void EndFrame()
        {
            _renderer.Render(_frameContainer, new Vector2(Width, Height));
        }

        public void Dispose()
        {
            
        }

    }
}