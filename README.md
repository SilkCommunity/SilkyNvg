# SilkyNvg
NanoVectorGraphicsExceptItUsesSilkNETNowWooHoo (Thank KoziLord for the description)
(A port of [memononen/nanovg](https://github.com/memononen/nanovg/) to .NET5.0)

## Usage / Examples
> A transcription of the NanoVG example can be found in the '[samples](https://github.com/MatijaBrown/SilkyNvg/tree/main/samples)' directory.

### Initialization And Rendering
To create a new Nvg instance, an implementation of `SilkyNvg.Rendering.INvgRenderer` must be specified,
then an instance can simply be created like this: `Nvg nvg = Nvg.Create(renderer);`.

All calls to the render-API must be wrapped between `Nvg.BeginFrame(width, height, pixelRatio);`
and ``Nvg.EndFrame();``.

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

### Renderers
SilkyNvg provides an OpenGL and a Vulkan render implementation out of the box.
It is also possible to create a custom render implementation (PRs welcome, if you want to share).

#### OpenGL Renderer
The [OpenGL renderer](https://github.com/MatijaBrown/SilkyNvg/tree/main/src/rendering/SilkyNvg.Rendering.OpenGL) is highly straightforward to use: In the constructer the API-Object and additional flags
have to be specified, the rest is handled internally. See the [NanoVG OpenGL Renderer](https://github.com/memononen/nanovg/tree/077b65e0cf3e22ee4f588783e319b19b0a608065#opengl-state-touched-by-the-backend) for more info.

#### Vulkan Renderer
The [Vulkan renderer](https://github.com/MatijaBrown/SilkyNvg/tree/main/src/rendering/SilkyNvg.Rendering.Vulkan) requires specifying some more additional information for creation using the `SilkyNvg.Rendering.Vulkan.VulkanRendererParams` struct, such as the current device, custom allocators if
used, the command buffer to render on and the current render pass. Furthermore, the constructor takes the API object and the queue to be used.
For additional info, see this [NanoVG Vulkan Renderer](https://github.com/danilw/nanovg_vulkan)

For further details, see the [NanoVG doc](https://github.com/memononen/nanovg/), as the API and implementations are intentionally kept similar.

#### Custom Renderer Implementations
To create a custom render implementation, the renderer class musst implement `SilkyNvg.Rendering.INvgRenderer`.
This interface contains the following methods and properties:

> ##### INvgRenderer
> - `bool EdgeAntiAlias` Wheather or not antialiasing was enabled when creating the renderer. Used because some renderers might not support > antialiasing.
> - `bool Create()` Is called when NanoVG is initialized. Initialize the renderer here. Return false if initialization failed, otherwise return true.
> - `int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data)` Is called to create a new image. *type* parameter specifies wheather *data* should be interpreted either as **RGBA** or **Alpha** values. Return handle to the new image.
> - `bool DeleteTexture(int image)` Is called to delete an image. Return false if image was not found, otherwise return true.
> - `bool UpdateTexture(int image, Rectangle<uint> bounds, ReadOnlySpan<byte> data)` Updates a specific section of the image to the new *data*. Return false if the image was not found, otherwise true.
> - `bool GetTextureSize(int image, out Vector2D<uint> size)` Returns the size of the specified image. Return false if the image was not found, otherwise true.
> - `void Viewport(Vector2D<float> size, float devicePxRatio)` Is called once per frame during `Nvg.BeginFrame` to set the viewport size and pixel ratio.
> - `void Cancel()` Stops drawing the frame. Clear all cached data.
> - `void Flush()` Draws the frame. Render everything here.
> - `void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Box2D<float> bounds, IReadOnlyList<Path> paths)` Is called on ``Nvg.Fill()``. Signals that the data contained in *paths* should be added to the rendered when flushing.
> - `Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Path> paths)` Is called on ``Nvg.Stroke()``. Signals that the data contained in *paths* should be added to the rendered when flushing.
> - `Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringeWidth)` Is called when rendering text. Signals that the *vertices* should be added should be added to the rendered when flushing.
> - `Dispose()` Delete and shut down the renderer.