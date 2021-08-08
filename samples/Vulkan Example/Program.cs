using NvgExample;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering.Vulkan;
using System;
using System.Diagnostics;

namespace Vulkan_Example
{
    unsafe class Program
    {

        static void Errorcb(Silk.NET.GLFW.ErrorCode error, string desc)
        {
            Console.Error.WriteLine("GLFW error: " + error + Environment.NewLine + desc);
        }

        static Glfw glfw;
        static Vk vk;
        static Nvg nvg;

        static bool blowup = false;
        static bool screenshot = false;
        static bool premult = false;
        static bool resizeEvent = false;

        static void Key(WindowHandle* window, Keys key, int _, InputAction action, KeyModifiers __)
        {
            if (key == Keys.Escape && action == InputAction.Press)
                glfw.SetWindowShouldClose(window, true);
            if (key == Keys.Space && action == InputAction.Press)
                blowup = !blowup;
            if (key == Keys.S && action == InputAction.Press)
                screenshot = true;
            if (key == Keys.P && action == InputAction.Press)
                premult = !premult;
        }

        static void PrepareFrame(Device device, CommandBuffer cmdBuffer, ref FrameBuffers fb)
        {
            Result res = VkUtil.KhrSwapchain.AcquireNextImage(device, fb.SwapChain, uint.MaxValue, fb.PresentCompleteSemaphore, default, ref fb.CurrentBuffer);
            if (res == Result.ErrorOutOfDateKhr)
            {
                resizeEvent = true;
                return;
            }
            Debug.Assert(res == Result.Success);

            CommandBufferBeginInfo cmdBufInfo = new(StructureType.CommandBufferBeginInfo);
            res = vk.BeginCommandBuffer(cmdBuffer, cmdBufInfo);
            Debug.Assert(res == Result.Success);

            ClearValue* clearValues = stackalloc ClearValue[2]
            {
                new ClearValue()
                {
                    Color = new ClearColorValue(0.3f, 0.3f, 0.32f, 1.0f),
                },
                new ClearValue()
                {
                    DepthStencil = new ClearDepthStencilValue(1.0f, 0)
                }
            };

            RenderPassBeginInfo rpBegin = new()
            {
                SType = StructureType.RenderPassBeginInfo,

                RenderPass = fb.RenderPass,
                Framebuffer = fb.Framebuffers[fb.CurrentBuffer],
                RenderArea = new Rect2D()
                {
                    Offset = new Offset2D(0, 0),
                    Extent = new Extent2D(fb.BufferSize.Width, fb.BufferSize.Height)
                },
                ClearValueCount = 2,
                PClearValues = clearValues
            };

            vk.CmdBeginRenderPass(cmdBuffer, rpBegin, SubpassContents.Inline);

            Viewport viewport = new()
            {
                Width = fb.BufferSize.Width,
                Height = fb.BufferSize.Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
                X = rpBegin.RenderArea.Offset.X,
                Y = rpBegin.RenderArea.Offset.Y
            };

            Rect2D scissor = rpBegin.RenderArea;
            vk.CmdSetScissor(cmdBuffer, 0, 1, scissor);
            vk.CmdSetViewport(cmdBuffer, 0, 1, viewport);
        }

        static unsafe void SubmitFrame(Device device, Queue queue, CommandBuffer cmdBuffer, ref FrameBuffers fb)
        {
            vk.CmdEndRenderPass(cmdBuffer);

            ImageMemoryBarrier imageBarrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,

                SrcAccessMask = AccessFlags.AccessMemoryWriteBit,
                DstAccessMask = AccessFlags.AccessMemoryReadBit,
                OldLayout = ImageLayout.ColorAttachmentOptimal,
                NewLayout = ImageLayout.PresentSrcKhr,
                SrcQueueFamilyIndex = uint.MaxValue,
                DstQueueFamilyIndex = uint.MaxValue,
                Image = fb.SwapChainBuffers[fb.CurrentBuffer].Image,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ImageAspectColorBit, 0, 1, 0, 1)
            };
            vk.CmdPipelineBarrier(cmdBuffer, PipelineStageFlags.PipelineStageAllGraphicsBit, PipelineStageFlags.PipelineStageBottomOfPipeBit, 0, 0, null, 0, null, 1, imageBarrier);
            vk.EndCommandBuffer(cmdBuffer);

            PipelineStageFlags pipeStageFlags = PipelineStageFlags.PipelineStageColorAttachmentOutputBit;
            Semaphore presentCompleteSemaphore = fb.PresentCompleteSemaphore;
            Semaphore renderCompleteSemaphore = fb.RenderCompleteSemaphore;

            SubmitInfo submitInfo = new(StructureType.SubmitInfo)
            {
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &presentCompleteSemaphore,
                PWaitDstStageMask = &pipeStageFlags,
                CommandBufferCount = 1,
                PCommandBuffers = &cmdBuffer,
                SignalSemaphoreCount = 1,
                PSignalSemaphores = &renderCompleteSemaphore
            };

            Result res = vk.QueueSubmit(queue, 1, submitInfo, default);
            Debug.Assert(res == Result.Success);

            SwapchainKHR swapchain = fb.SwapChain;
            PresentInfoKHR present = new(StructureType.PresentInfoKhr)
            {
                SwapchainCount = 1,
                PSwapchains = &swapchain,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &renderCompleteSemaphore
            };
            fixed (uint* ptr = &fb.CurrentBuffer)
            {
                present.PImageIndices = ptr;
            }

            res = VkUtil.KhrSwapchain.QueuePresent(queue, present);
            if (res == Result.ErrorOutOfDateKhr)
            {
                res = vk.QueueWaitIdle(queue);
                resizeEvent = true;
                return;
            }
            Debug.Assert(res == Result.Success);

            res = vk.QueueWaitIdle(queue);
        }

        static void Main()
        {
            glfw = Glfw.GetApi();

            if (!glfw.Init())
            {
                Console.Error.WriteLine("Failed to init GLFW");
                Environment.Exit(-1);
            }

            if (!glfw.VulkanSupported())
            {
                Console.Error.WriteLine("Vulkan not supported!");
                Environment.Exit(1);
            }

            glfw.SetErrorCallback(Errorcb);

            glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);

            WindowHandle* window = glfw.CreateWindow(1000, 600, "SilkyNvg", null, null);
            if (window == null)
            {
                glfw.Terminate();
                Environment.Exit(-1);
            }

            glfw.SetKeyCallback(window, Key);

            glfw.SetTime(0);

            vk = VkUtil.Vk;
            Instance instance = VkUtil.CreateInstance(true, glfw);

            VkNonDispatchableHandle surfaceHandle = new();
            Result res = (Result)glfw.CreateWindowSurface(new VkHandle(instance.Handle), window, null, &surfaceHandle);
            SurfaceKHR surface = new(surfaceHandle.Handle);
            if (res != Result.Success)
            {
                Console.Error.WriteLine("glfwCreateWindowSurface failed");
                Environment.Exit(-1);
            }

            PhysicalDevice gpu = default;
            uint gpuCount = 1;
            res = vk.EnumeratePhysicalDevices(instance, ref gpuCount, ref gpu);
            if (res != Result.Success && res != Result.Incomplete)
            {
                Console.Error.WriteLine("vkEnumeratePhysicalDevices failed " + res);
                Environment.Exit(-1);
            }
            VulkanDevice device = new(gpu, vk);
            VkUtil.InitExtensions(instance, device.Device);

            glfw.GetWindowSize(window, out int winWidth, out int winHeight);

            vk.GetDeviceQueue(device.Device, device.GraphicsQueueFamilyIndex, 0, out Queue queue);
            FrameBuffers fb = new(device, surface, queue, winWidth, winHeight, default, vk);

            CommandBuffer cmdBuffer = VkUtil.CreateCmdBuffer(device.Device, device.CommandPool);

            VulkanRendererParams prams = default;
            prams.device = device.Device;
            prams.gpu = device.Gpu;
            prams.renderpass = fb.RenderPass;
            prams.cmdBuffer = cmdBuffer;

            nvg = Nvg.Create(new VulkanRenderer(prams, CreateFlags.Antialias | CreateFlags.StencilStrokes | CreateFlags.Debug, queue, vk));

            Demo demo = new(nvg);

            PerformanceGraph fps = new(PerformanceGraph.GraphRenderStyle.Fps, "Frame Time");

            double prevt = glfw.GetTime();

            while (!glfw.WindowShouldClose(window))
            {
                glfw.GetWindowSize(window, out int cWinWidth, out int cWinHeight);
                if (resizeEvent || (winWidth != cWinWidth) || (winHeight != cWinHeight))
                {
                    winWidth = cWinWidth;
                    winHeight = cWinHeight;
                    fb.Dispose();
                    fb = new FrameBuffers(device, surface, queue, winWidth, winHeight, default, vk);
                    resizeEvent = false;
                }
                else
                {
                    PrepareFrame(device.Device, cmdBuffer, ref fb);
                    if (resizeEvent)
                    {
                        continue;
                    }

                    double t = glfw.GetTime();
                    double dt = t - prevt;
                    prevt = t;
                    fps.Update((float)dt);

                    float pxRatio = (float)fb.BufferSize.Width / (float)winWidth;

                    glfw.GetCursorPos(window, out double mx, out double my);

                    nvg.BeginFrame(winWidth, winHeight, pxRatio);

                    nvg.BeginPath();
                    nvg.Rect(50.0f, 50.0f, 100.0f, 25.0f);
                    nvg.FillColour(Colour.BlueViolet);
                    nvg.Fill();

                    nvg.BeginPath();
                    nvg.Circle(50.0f, 100.0f, 25.0f);
                    nvg.FillColour(Colour.Orange);
                    nvg.Fill();

                    nvg.EndFrame();

                    SubmitFrame(device.Device, queue, cmdBuffer, ref fb);
                }
                glfw.PollEvents();
            }

            demo.Dispose();
            nvg.Dispose();

            fb.Dispose();

            device.Dispose();

            VkUtil.KhrSurface.DestroySurface(instance, surface, null);

            vk.DestroyInstance(instance, null);

            glfw.DestroyWindow(window);

            Console.WriteLine("Average Frame Time: " + (fps.GraphAverage * 1000.0f) + " ms");

            glfw.Terminate();

            VkUtil.KhrSurface.Dispose();
            VkUtil.KhrSwapchain.Dispose();
            VkUtil.ExtDebugReport.Dispose();
            vk.Dispose();

            glfw.Dispose();
        }

    }
}
