// See https://aka.ms/new-console-template for more information

using Silk.NET.Maths;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Rendering;

var options = WindowOptions.Default;
options.Title = "OpenGL Example";
options.Size = new Vector2D<int>(1280, 720);

var window = Window.Create(options);
window.Load += Load;
window.Update += Update;
window.Render += Render;
window.Closing += Closing;
window.Run();

ISilkyRenderer renderer;
SilkyRenderingContext context;

return;

void Load()
{
    renderer = null;

    var settings = new SilkyRenderingContextSettings()
    {
        Alpha = true,
        Desynchronised = false
    };
    
    context = new SilkyRenderingContext(settings, renderer);
}

void Update(double delta)
{
    
}

void Render(double _)
{
    
}

void Closing()
{
    
}