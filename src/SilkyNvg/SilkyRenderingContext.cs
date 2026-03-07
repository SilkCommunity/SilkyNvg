using System;
using SilkyNvg.Rendering;
using SilkyNvg.States;
using SilkyNvg.Utils;

namespace SilkyNvg
{
    public sealed class SilkyRenderingContext : IDisposable
    {

        private readonly SilkyRenderingContextSettings _settings;
        private readonly ISilkyRenderer _renderer;

        internal RenderTolerances Tolerances { get; } = new();

        internal StateStack StateStack { get; } = new();
        
        public SilkyRenderingContext(SilkyRenderingContextSettings settings, ISilkyRenderer renderer)
        {
            _settings = settings;
            _renderer = renderer;
        }
        
        public SilkyRenderingContextSettings GetContextSettings()
            =>  _settings;
        
    #region Dimension
        
        public uint Width { get; set; } = 600;

        public uint Height { get; set; } = 400;

        public float PixelRatio
        {
            get;
            set
            {
                field = value;
                Tolerances.UpdateTols(field);
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

        public void Dispose()
        {
            
        }

    }
}