using NvgExample;
using Silk.NET.Core.Native;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering.Vulkan;
using System;
using System.Diagnostics;

namespace Vulkan_Example
{
    class Program
    {

        public const uint COMMAND_POOL_RESET_THRESHOLD = 10;

        private static Extent2D windowExtent = new(1000, 600);
        private static IWindow window;

        private static Vk vk;
        private static Nvg nvg;

        private static Instance instance;
        private static PhysicalDevice physicalDevice;
        private static Device device;
        private static SurfaceKHR surface;

        private static Swapchain swapchain;

        private static Queue graphicsQueue, presentQueue;
        private static uint graphicsQueueFamily, presentQueueFamily;

        private static ulong frameIndex = 0;

        private static Frame[] frames;

        private static RenderPass renderPass;
        private static Framebuffer[] framebuffers;

        private static DepthImage depthImage;

        private static bool blowup = false;
        private static bool premult = false;
        private static bool resize = false, resized = false;
        private static float mx, my;

        private static double prevTime = 0;
        private static double cpuTime = 0;

        private static PerformanceGraph frameGraph;
        private static PerformanceGraph cpuGraph;

        private static Demo demo;

        private static Stopwatch timer;

        #region Input
        private static unsafe void KeyDown(IKeyboard _, Key key, int _2)
        {
            if (key == Key.Escape)
                window.Close();
            else if (key == Key.Space)
                blowup = !blowup;
            else if (key == Key.P)
                premult = !premult;
        }

        private static void MouseMove(IMouse _, System.Numerics.Vector2 mousePosition)
        {
            mx = mousePosition.X;
            my = mousePosition.Y;
        }

        private static void LoadInput()
        {
            IInputContext input = window.CreateInput();
            foreach (IKeyboard keyboard in input.Keyboards)
            {
                keyboard.KeyDown += KeyDown;
            }
            foreach (IMouse mouse in input.Mice)
            {
                mouse.MouseMove += MouseMove;
            }
        }
        #endregion

        #region Vulkan Initialization
        private static unsafe void InitInstance()
        {
            string[] instanceLayers =
            {
                "VK_LAYER_KHRONOS_validation"
            };
            instance = VkUtil.CreateInstance(instanceLayers, Array.Empty<string>(), window.VkSurface);
            VkUtil.GetInstanceExtensions();
            surface = window.VkSurface.Create<uint>(new VkHandle(instance.Handle), null).ToSurface();
        }

        private static (SurfaceCapabilitiesKHR, SurfaceFormatKHR[], PresentModeKHR[]) InitDevice()
        {
            string[] deviceExtensions =
            {
                KhrSwapchain.ExtensionName
            };
            physicalDevice = VkUtil.PickPhysicalDevice(deviceExtensions, surface, out graphicsQueueFamily,
                out presentQueueFamily, out (SurfaceCapabilitiesKHR, SurfaceFormatKHR[], PresentModeKHR[]) swapchainData);
            device = VkUtil.CreateDevice(deviceExtensions, physicalDevice, graphicsQueueFamily, presentQueueFamily);
            vk.GetDeviceQueue(device, graphicsQueueFamily, 0, out graphicsQueue);
            vk.GetDeviceQueue(device, presentQueueFamily, 0, out presentQueue);
            VkUtil.GetDeviceExtensions();
            return swapchainData;
        }

        private static unsafe void InitRenderPass()
        {
            AttachmentDescription nvgColourAttachment = new()
            {
                Format = swapchain.Format,

                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,

                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,

                Samples = SampleCountFlags.SampleCount1Bit,

                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr
            };

            AttachmentDescription nvgDepthStencilAttachment = new()
            {
                Format = depthImage.Format,

                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.DontCare,

                StencilLoadOp = AttachmentLoadOp.Clear,
                StencilStoreOp = AttachmentStoreOp.DontCare,

                Samples = SampleCountFlags.SampleCount1Bit,

                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
            };

            Span<AttachmentDescription> attachments = stackalloc AttachmentDescription[] { nvgColourAttachment, nvgDepthStencilAttachment };

            AttachmentReference colourAttachmentRef = new()
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal
            };

            AttachmentReference depthStencilAttachmentRef = new()
            {
                Attachment = 1,
                Layout = ImageLayout.DepthStencilAttachmentOptimal
            };

            SubpassDescription subpass = new()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,

                ColorAttachmentCount = 1,
                PColorAttachments = &colourAttachmentRef,
                InputAttachmentCount = 0,
                PInputAttachments = null,
                PDepthStencilAttachment = &depthStencilAttachmentRef,
                PreserveAttachmentCount = 0,
                PPreserveAttachments = null,
                PResolveAttachments = null
            };

            SubpassDependency dependency = new()
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.PipelineStageColorAttachmentOutputBit | PipelineStageFlags.PipelineStageEarlyFragmentTestsBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.PipelineStageColorAttachmentOutputBit | PipelineStageFlags.PipelineStageEarlyFragmentTestsBit,
                DstAccessMask = AccessFlags.AccessColorAttachmentWriteBit | AccessFlags.AccessDepthStencilAttachmentWriteBit
            };

            RenderPassCreateInfo renderPassCreateInfo = VkInit.RenderPassCreateInfo(attachments, dependency, subpass);
            VkUtil.AssertVulkan(vk.CreateRenderPass(device, renderPassCreateInfo, null, out renderPass));
        }

        private static unsafe void InitDepthImage()
        {
            depthImage = new DepthImage(windowExtent.Width, windowExtent.Height, physicalDevice, device);
        }

        private static unsafe void InitFramebuffers()
        {
            FramebufferCreateInfo framebufferCreateInfo = VkInit.FramebufferCreateInfo(2, renderPass, windowExtent);
            ImageView* imageViews = stackalloc ImageView[2];
            imageViews[1] = depthImage.Handle;

            framebuffers = new Framebuffer[swapchain.Images.Length];
            for (int i = 0; i < framebuffers.Length; i++)
            {
                imageViews[0] = swapchain.ImageViews[i];
                framebufferCreateInfo.PAttachments = imageViews;
                VkUtil.AssertVulkan(vk.CreateFramebuffer(device, framebufferCreateInfo, null, out framebuffers[i]));
            }
        }

        private static void InitFrames()
        {
            frames = new Frame[swapchain.Images.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new Frame(graphicsQueueFamily, device, vk);
            }
        }

        private static unsafe void LoadVulkan()
        {
            vk = Vk.GetApi();
            VkUtil.Vk = vk;

            InitInstance();
            (SurfaceCapabilitiesKHR, SurfaceFormatKHR[], PresentModeKHR[]) swapchainData = InitDevice();
            swapchain = new Swapchain(swapchainData, PresentModeKHR.PresentModeFifoKhr, 3, windowExtent,
                graphicsQueueFamily, presentQueueFamily, surface);
            InitDepthImage();
            InitRenderPass();
            InitFramebuffers();
            InitFrames();
        }
        #endregion

        private static void LoadExample()
        {
            VulkanRendererParams @params = new()
            {
                PhysicalDevice = physicalDevice,
                Device = device,
                AllocationCallbacks = IntPtr.Zero,

                InitialCommandBuffer = default,

                FrameCount = (uint)frames.Length,
                AdvanceFrameIndexAutomatically = true,

                RenderPass = renderPass,
                SubpassIndex = 0,

                ImageQueueFamily = graphicsQueueFamily,
                ImageQueueFamilyIndex = 1
            };
            VulkanRenderer nvgRenderer = new(CreateFlags.Antialias | CreateFlags.StencilStrokes | CreateFlags.TriangleListFill | CreateFlags.Debug, @params, vk);
            nvg = Nvg.Create(nvgRenderer);

            demo = new Demo(nvg);

            timer = Stopwatch.StartNew();

            timer.Restart();

            prevTime = timer.Elapsed.TotalMilliseconds;
        }

        private static void Load()
        {
            LoadInput();
            LoadVulkan();
            LoadExample();
        }

        #region Swapchain Recreation
        private static unsafe void DestroySwapchain()
        {
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].FreeCommandBuffers();
            }
            foreach (Framebuffer framebuffer in framebuffers)
            {
                vk.DestroyFramebuffer(device, framebuffer, null);
            }
            depthImage.Dispose();
            vk.DestroyRenderPass(device, renderPass, null);
        }

        private static void RecreateSwapchain()
        {
            windowExtent = new Extent2D((uint)window.Size.X, (uint)window.Size.Y);
            VkUtil.AssertVulkan(vk.DeviceWaitIdle(device));

            DestroySwapchain();

            swapchain.Recreate(windowExtent);
            InitDepthImage();
            InitRenderPass();
            InitFramebuffers();
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].CreateCommandBuffers();
            }
        }

        private static void FramebufferResize(Vector2D<int> _)
        {
            resize = true;
        }
        #endregion

        private static uint GetNextSwapchainImageIndex(Frame frame)
        {
            uint swapchainImageIdx = 0;
            Result result = VkUtil.KhrSwapchain.AcquireNextImage(device, swapchain.Handle, 1_000_000_000, frame.PresentSemaphore, default, ref swapchainImageIdx);
            if (result == Result.ErrorOutOfDateKhr)
            {
                resized = true;
                RecreateSwapchain();
                return swapchainImageIdx;
            }
            else if (result != Result.Success && result != Result.SuboptimalKhr)
            {
                VkUtil.AssertVulkan(result);
            }
            return swapchainImageIdx;
        }

        private static unsafe void BeginRender(CommandBuffer cmd, uint swapchainImageIdx)
        {
            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,

                Flags = CommandBufferUsageFlags.CommandBufferUsageOneTimeSubmitBit,
                PInheritanceInfo = null
            };

            VkUtil.AssertVulkan(vk.BeginCommandBuffer(cmd, beginInfo));

            ClearValue* clearValues = stackalloc ClearValue[2];
            if (premult)
            {
                clearValues[0] = new ClearValue()
                {
                    Color = new ClearColorValue(0.0f, 0.0f, 0.0f, 0.0f)
                };
            }
            else
            {
                clearValues[0] = new ClearValue()
                {
                    Color = new ClearColorValue(0.3f, 0.3f, 0.32f, 1.0f)
                };
            }
            clearValues[1] = new ClearValue()
            {
                DepthStencil = new ClearDepthStencilValue(1.0f, 0)
            };

            RenderPassBeginInfo renderPassBeginInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,

                RenderPass = renderPass,
                RenderArea = new Rect2D()
                {
                    Offset = new Offset2D(0, 0),
                    Extent = windowExtent
                },
                Framebuffer = framebuffers[swapchainImageIdx],

                ClearValueCount = 2,
                PClearValues = clearValues
            };

            vk.CmdBeginRenderPass(cmd, renderPassBeginInfo, SubpassContents.Inline);
        }

        private static unsafe void EndRender(Semaphore presentSemaphore, Semaphore renderSemaphore, Fence renderFence, CommandBuffer cmd)
        {
            vk.CmdEndRenderPass(cmd);

            VkUtil.AssertVulkan(vk.EndCommandBuffer(cmd));

            PipelineStageFlags waitStage = PipelineStageFlags.PipelineStageColorAttachmentOutputBit;
            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,

                PWaitDstStageMask = &waitStage,

                WaitSemaphoreCount = 1,
                PWaitSemaphores = &presentSemaphore,
                SignalSemaphoreCount = 1,
                PSignalSemaphores = &renderSemaphore,

                CommandBufferCount = 1,
                PCommandBuffers = &cmd
            };

            VkUtil.AssertVulkan(vk.QueueSubmit(graphicsQueue, 1, submitInfo, renderFence));
        }

        private static unsafe void Present(Semaphore renderSemaphore, uint swapchainImageIdx)
        {
            PresentInfoKHR presentInfo = new()
            {
                SType = StructureType.PresentInfoKhr,

                SwapchainCount = 1,

                WaitSemaphoreCount = 1,
                PWaitSemaphores = &renderSemaphore,

                PImageIndices = &swapchainImageIdx
            };
            fixed (SwapchainKHR* ptr = &swapchain.Handle)
            {
                presentInfo.PSwapchains = ptr;
            }

            Result result = VkUtil.KhrSwapchain.QueuePresent(presentQueue, presentInfo);
            if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || resize)
            {
                resize = false;
                RecreateSwapchain();
            }
            else if (result != Result.Success)
            {
                VkUtil.AssertVulkan(result);
            }
        }

        private static unsafe void Render(double _)
        {
            Frame frame = frames[(int)frameIndex % frames.Length];

            VkUtil.AssertVulkan(vk.WaitForFences(device, 1, frame.RenderFence, true, 1_000_000_000));
            VkUtil.AssertVulkan(vk.ResetFences(device, 1, frame.RenderFence));

            CommandBuffer cmd = frame.GetCommandBuffer();

            uint swapchainImageIdx = GetNextSwapchainImageIndex(frame);
            if (resized)
            {
                resized = false;
                return;
            }

            BeginRender(cmd, swapchainImageIdx);
            ////////////////////////////////////////////////////////////
            double t = timer.Elapsed.TotalSeconds;
            double dt = t - prevTime;
            prevTime = t;

            Vector2D<float> wSize = window.Size.As<float>();
            Vector2D<float> fbSize = window.FramebufferSize.As<float>();

            float devicePxRatio = fbSize.X / wSize.X;

            nvg.BeginFrame(wSize, devicePxRatio);

            demo.Render(mx, my, wSize.X, wSize.Y, (float)t, blowup);

            frameGraph.Render(5.0f, 5.0f, nvg);
            cpuGraph.Render(5.0f + 200.0f + 5.0f, 5.0f, nvg);

            nvg.EndFrame(cmd);

            cpuTime = timer.Elapsed.TotalSeconds - t;

            frameGraph.Update((float)dt);
            cpuGraph.Update((float)cpuTime);

            ////////////////////////////////////////////////////////////
            EndRender(frame.PresentSemaphore, frame.RenderSemaphore, frame.RenderFence, cmd);
            Present(frame.RenderSemaphore, swapchainImageIdx);

            frameIndex++;
        }

        private static unsafe void DisposeVulkan()
        {
            DestroySwapchain();
            swapchain.Dispose();
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].Dispose();
            }
            vk.DestroyDevice(device, null);
            VkUtil.KhrSurface.DestroySurface(instance, surface, null);
            vk.DestroyInstance(instance, null);

            VkUtil.KhrSurface.Dispose();
            VkUtil.KhrSwapchain.Dispose();
            vk.Dispose();
        }

        private static void Close()
        {
            timer.Stop();

            VkUtil.AssertVulkan(vk.DeviceWaitIdle(device));
            demo.Dispose();
            nvg.Dispose();

            DisposeVulkan();
        }

        static void Main()
        {
            frameGraph = new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Fps, "Frame Time");
            cpuGraph = new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Ms, "CPU Time");

            WindowOptions windowOptions = WindowOptions.DefaultVulkan;
            windowOptions.FramesPerSecond = -1;
            windowOptions.Size = new Vector2D<int>((int)windowExtent.Width, (int)windowExtent.Height);
            windowOptions.Title = "SilkyNvg";

            window = Window.Create(windowOptions);
            window.Load += Load;
            window.FramebufferResize += FramebufferResize;
            window.Render += Render;
            window.Closing += Close;
            window.Run();

            window.Dispose();
        }

    }
}
