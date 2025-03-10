# SilkyNvg
[![NuGet](https://img.shields.io/nuget/v/SilkyNvg)](https://nuget.org/packages/SilkyNvg)

NanoVectorGraphicsExceptItUsesSilkNETNowWooHoo (Thank KoziLord for the description)
(A port of [memononen/nanovg](https://github.com/memononen/nanovg/) to .NET)

SilkyNvg is a small library providing basic 2D-Drawing functionallity well suited for UI and other such applications. It provides basic shape capabillities like rectangles or circles, as well as the abillity to build custom paths which can either be filled or stroked (lines).
The colour can either be a colour (`SilkyNvg.Colour`), a paint (`SilkyNvg.Paint`) or an image (`SilkyNvg.Paint`).
On top of this a simple text-drawing-API is provided. See [FontStash.NET](https://github.com/MatijaBrown/FontStash.NET) for further details.

## Usage / Examples
> A transcription of the NanoVG example can be found in the '[samples](https://github.com/MatijaBrown/SilkyNvg/tree/main/samples)' directory.

### Initialization And Rendering
To create a new Nvg instance, an implementation of `SilkyNvg.Rendering.INvgRenderer` must be specified,
then an instance can simply be created like this: `Nvg nvg = Nvg.Create(renderer);`.

All calls to the render-API must be wrapped between `Nvg.BeginFrame(width, height, pixelRatio);`
and ``Nvg.EndFrame();``. To transform from a pixel-based coordinate system to any other, with and height can have any values independant from the actual window's size.

### Further Details
For further details, see the [NanoVG doc](https://github.com/memononen/nanovg/), as the API and implementations are intentionally kept similar.

## API Description

### Paths
*Paths* form the basis of *SilkyNvg* rendering. To begin drawing a *path*, call `NvgPaths.BeginPath`, which clears the path cache and intialises drawing a new path.
A *path* is composed of five basic instructions: *MoveTo*, *LineTo* *BezierTo* and *Close*. *MoveTo* jumps to the specified point, *Close* is special in that it
returns to the paths origin with a straight line.
It is easiest to imagine this as a pen, tracing out the desired shape on a canvas, picking up and setting down for *MoveTo* instructions.

Additionaly, many convenience methods are provided to draw more complex shapes such as *arcs*, *ellipses* or *rectangles*. These are constructed from the atomic
segments above.

The following example would create the *path* for a simple rectangle.
```csharp
nvg.BeginPath();
nvg.MoveTo(100, 100);
nvg.LineTo(150, 100);
nvg.LineTo(150, 150);
nvg.LineTo(100, 150);
nvg.Close();
```

### Fill and Stroke
There are two ways to render your *path*: **Stroke** and **Fill**.
**Fill** fills the traced out path according to the specified *winding rule*. `Winding.Cw // Clockwise` winding draws the path as a hole, `Winding.Ccw // Counter-Clockwise` fills it regularly.
**Stroke** draws along the path, tracing out then "pen's" journey about the canvas. Behaviour at sharp corners and ends can be specified using `NvgRenderStyle.LineJoin` and `NvgRenderStyle.LineCap`.

### Paints
The manner in which the *fill* and *stroke*s actually show up is handled by **paints**. These can be either specified as
1. A solid colour
2. A linear, radial or box gradient
3. An image

*SilkyNvg* uses different *paints* for **fill** and **stroke** rendering, which can be set by `Nvg.FillPaint` and `Nvg.StrokePain` respectively.
Additionaly there are convenience methods `Nvg.FillColour` and `Nvg.StrokeColour` provided so that no new paint need be explicitly created for solid colour filling.

Finally the `Paint` struct also exposes all settings in a public constructor, allowing the implementation of custom gradients using a paint transform.

### Images
As mentioned above, instead of gradients or solid colours *paints* also provide the ability to set images as background for drawing *paths*.
Images can be loaded, either from a file or memory using `NvgImages.CreateImage` / `NvgImages.CreateImageMem` / `NvgImages.CreateImageRgba`.

### Fonts
**[WORK IN PROGRESS]**

### API structure
The implementations are split up into different components, these being:
- SilkyNvg.Blending *for global blending*
- SilkyNvg.Graphics *for render styles*
- SilkyNvg.Images *for images*
- SilkyNvg.Paths *for creating and drawing paths*
- SilkyNvg.Scissoring *for custom scissors*
- SilkyNvg.Text *for text*
- SilkyNvg.Transforms *for creating and altering the transform*
- SilkyNvg *for core methods, such as BeginFrame, DebugDumpCache, Colour and Paint*
All components are linked to `SilkyNvg.Nvg` via extension methods in their respective namespaces.

## Renderers
The renderers are specified when creating the Nvg instance in `Nvg.Create`.
SilkyNvg provides a Vulkan- and OpenGL-Renderer out of the box, but custom renderers can easilly be built using the `SilkyNvg.Rendering.INvgRenderer` interface.
It is standard for renderers taking a `CreateFlags` mask in their constructor used to specifiy renderer-specific settings.

### Antialiasing
SilkyNvg automatically calculates antialiased vertices if `SilkyNvg.Rendering.INvgRenderer.EdgeAntiAlias` is set to true. This is done because some renderers might not support antialiasing. In the custom renderers, the CreateFlags specify wheather or not antialiasing should be done.

### OpenGL Renderer
The [OpenGL renderer](https://github.com/MatijaBrown/SilkyNvg/tree/main/src/rendering/SilkyNvg.Rendering.OpenGL)'s constructor takes 2 arguments; `SilkyNvg.Rendering.OpenGL.CreateFlags` and `Silk.NET.OpenGL.GL`. The second parameter is an API-Instance of OpenGL, the `CreateFlags` mask has the following options:
- `CreateFlags.Antialias` States that the renderer should draw antialiased or raw meshes.
- `CreateFlags.StencilStrokes` States that the renderer should use the stencil buffer when drawing strokes (lines).
- `CreateFlags.Debug` States that the renderer should print errors.

**Note**: *SilkyNvg* OpenGL renderer makes heavy use of the stencil buffer. Please remember to `glClear(... | ClearBufferMask.StencilBufferBit)`!

### Vulkan Renderer
The [Vulkan renderer](https://github.com/MatijaBrown/SilkyNvg/tree/main/src/rendering/SilkyNvg.Rendering.Vulkan) is currently **deprectated** and only compatible with older versions of *Silky*. A faster update is in the works.

### Custom Renderer Implementations
To create a custom render implementation, the renderer class musst implement `SilkyNvg.Rendering.INvgRenderer`.
This interface contains the following methods and properties:

#### INvgRenderer
- `bool EdgeAntiAlias` Wheather or not antialiasing was enabled when creating the renderer. Can depend on wheather antialiasing is supported and if its use was specified when instantiating the renderer.
- `bool Create()` Is called when NanoVG is initialized. Initialize the renderer here. Return false if initialization failed, otherwise return true.
- `int CreateTexture(Texture type, SizeF size, ImageFlags imageFlags, ReadOnlySpan<byte> data)` Is called to create a new image. *type* parameter specifies wheather *data* should be interpreted either as **RGBA** or **Alpha** values. Return handle to the new image.
- `bool DeleteTexture(int image)` Is called to delete an image. Return false if image was not found, otherwise return true.
- `bool UpdateTexture(int image, Rectangle bounds, ReadOnlySpan<byte> data)` Updates a specific section of the image to the new *data*. Return false if the image was not found, otherwise true.
- `bool GetTextureSize(int image, out SizeF size)` Returns the size of the specified image. Return false if the image was not found, otherwise true.
- `void Viewport(SizeF size, float devicePxRatio)` Is called once per frame during `Nvg.BeginFrame` to set the viewport size and pixel ratio.
- `void Cancel()` Stops drawing the frame. Clear all cached data.
- `void Flush()` Draws the frame. Render everything here.
- `void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, RectangleF bounds, IReadOnlyList<Path> paths)` Is called on ``Nvg.Fill()``. Signals that the data contained in *paths* should be added to the rendered when flushing.
- `Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Path> paths)` Is called on ``Nvg.Stroke()``. Signals that the data contained in *paths* should be added to the rendered when flushing.
- `Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringeWidth)` Is called when rendering text. Signals that the *vertices* should be added should be added to the rendered when flushing.
- `Dispose()` Delete and shut down the renderer.
