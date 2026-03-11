// See https://aka.ms/new-console-template for more information

using Silk.NET.Maths;
using Silk.NET.Windowing;

var options = WindowOptions.Default;
options.Title = "OpenGL Example";
options.Size = new Vector2D<int>(1280, 720);

var window = Window.Create(options);
window.Update += Update;
window.Render += Render;
window.Run();

return;

static void Update(double delta)
{
    
}

static void Render(double _)
{
    
}