using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Diagnostics;

namespace Vulkan_Example
{
    public static unsafe class VkUtil
    {

        public static Vk Vk { get; }

        static VkUtil()
        {
            Vk = Vk.GetApi();
        }

        public static ExtDebugReport ExtDebugReport { get; private set; }

        public static KhrSurface KhrSurface { get; private set; }

        public static KhrSwapchain KhrSwapchain { get; private set; }

        public static void InitExtensions(Instance instance, Device device)
        {
            Vk.TryGetInstanceExtension(instance, out ExtDebugReport extDebugReport);
            ExtDebugReport = extDebugReport;

            Vk.TryGetInstanceExtension(instance, out KhrSurface khrSurface);
            KhrSurface = khrSurface;

            Vk.TryGetDeviceExtension(instance, device, out KhrSwapchain khrSwapchain);
            KhrSwapchain = khrSwapchain;
        }

        public static Instance CreateInstance(bool enableDebugLayer, Glfw glfw)
        {
            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,

                PApplicationName = (byte*)SilkMarshal.StringToPtr("SilkyNvg"),
                ApplicationVersion = 1,
                PEngineName = (byte*)SilkMarshal.StringToPtr("SilkyNvg"),
                EngineVersion = 1,
                ApiVersion = Vk.Version12
            };

            byte** appendExtensions = stackalloc byte*[]
            {
                (byte*)SilkMarshal.StringToPtr(ExtDebugReport.ExtensionName),
                (byte*)SilkMarshal.StringToPtr(KhrSurface.ExtensionName),
            };
            uint appendExtensionsCount = 1;
            if (!enableDebugLayer)
            {
                appendExtensionsCount = 0;
            }

            byte** glfwExtensions = glfw.GetRequiredInstanceExtensions(out uint extensionsCount);

            byte** extensions = stackalloc byte*[(int)(extensionsCount + appendExtensionsCount)];

            for (uint i = 0; i < extensionsCount; i++)
            {
                extensions[i] = glfwExtensions[i];
            }
            for (uint i = 0; i < appendExtensionsCount; i++)
            {
                extensions[extensionsCount++] = appendExtensions[i];
            }

            InstanceCreateInfo instInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,

                PApplicationInfo = &appInfo,
                EnabledExtensionCount = extensionsCount,
                PpEnabledExtensionNames = extensions
            };

            if (enableDebugLayer)
            {
                uint layerCount = 0;
                Vk.EnumerateInstanceLayerProperties(ref layerCount, null);
                LayerProperties* layerProp = stackalloc LayerProperties[(int)layerCount];
                Vk.EnumerateInstanceLayerProperties(ref layerCount, layerProp);
                Console.Write("supported layers: ");
                for (uint i = 0; i < layerCount; i++)
                {
                    Console.Write(SilkMarshal.PtrToString((nint)layerProp[i].LayerName) + " ,");
                }
                Console.Write(Environment.NewLine);

                byte** instanceValidationLayers = stackalloc byte*[]
                {
                    (byte*)SilkMarshal.StringToPtr("VK_LAYER_KHRONOS_validation")
                };
                uint instanceValidationLayerCount = 1;
                instInfo.EnabledLayerCount = instanceValidationLayerCount;
                instInfo.PpEnabledLayerNames = instanceValidationLayers;
            }

            Result res = Vk.CreateInstance(instInfo, null, out Instance inst);

            if (res == Result.ErrorIncompatibleDriver)
            {
                Console.Error.WriteLine("cannot find a compatible Vulkan ICD!");
                Environment.Exit(-1);
            }
            else if (res != Result.Success)
            {
                switch (res)
                {
                    case Result.ErrorOutOfHostMemory:
                        Console.Error.WriteLine("VK_ERROR_OUT_OF_HOST_MEMORY");
                        break;
                    case Result.ErrorOutOfDeviceMemory:
                        Console.Error.WriteLine("VK_ERROR_OUT_OF_DEVICE_MEMORY");
                        break;
                    case Result.ErrorInitializationFailed:
                        Console.Error.WriteLine("VK_ERROR_INITIALIZATION_FAILED");
                        break;
                    case Result.ErrorLayerNotPresent:
                        Console.Error.WriteLine("VK_ERROR_LAYER_NOT_PRESENT");
                        break;
                    case Result.ErrorExtensionNotPresent:
                        Console.Error.WriteLine("VK_ERROR_EXTENSION_NOT_PRESENT");
                        break;
                    default:
                        Console.Error.WriteLine("uknown error " + res);
                        break;
                }
                Environment.Exit(-1);
            }

            return inst;
        }

        public static CommandBuffer CreateCmdBuffer(Device device, CommandPool cmdPool)
        {
            CommandBufferAllocateInfo cmd = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,

                CommandPool = cmdPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1
            };

            Result res = Vk.AllocateCommandBuffers(device, cmd, out CommandBuffer cmdBuffer);
            Debug.Assert(res == Result.Success);
            return cmdBuffer;
        }

        public static RenderPass CreateRenderPass(Device device, Format colourFormat, Format depthFormat)
        {
            AttachmentDescription* attachments = stackalloc AttachmentDescription[2]
            {
                new AttachmentDescription()
                {
                    Format = colourFormat,
                    Samples = SampleCountFlags.SampleCount1Bit,
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.Store,
                    StencilLoadOp = AttachmentLoadOp.Clear,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = ImageLayout.Undefined,
                    FinalLayout = ImageLayout.ColorAttachmentOptimal
                },

                new AttachmentDescription()
                {
                    Format = depthFormat,
                    Samples = SampleCountFlags.SampleCount1Bit,
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.DontCare,
                    StencilLoadOp = AttachmentLoadOp.Clear,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = ImageLayout.Undefined,
                    FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
                }
            };

            AttachmentReference colourReference = new(0, ImageLayout.ColorAttachmentOptimal);

            AttachmentReference depthReference = new(1, ImageLayout.DepthStencilAttachmentOptimal);

            SubpassDescription subpass = new()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                InputAttachmentCount = 0,
                PInputAttachments = null,
                ColorAttachmentCount = 1,
                PColorAttachments = &colourReference,
                PResolveAttachments = null,
                PDepthStencilAttachment = &depthReference,
                PreserveAttachmentCount = 0,
                PPreserveAttachments = null
            };

            RenderPassCreateInfo rpInfo = new()
            {
                SType = StructureType.RenderPassCreateInfo,

                AttachmentCount = 2,
                PAttachments = attachments,
                SubpassCount = 1,
                PSubpasses = &subpass
            };

            Result res = Vk.CreateRenderPass(device, rpInfo, null, out RenderPass renderPass);
            Debug.Assert(res == Result.Success);

            return renderPass;
        }

    }
}
