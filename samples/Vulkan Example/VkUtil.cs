using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Vulkan_Example
{
    public static class VkUtil
    {

        public static Vk Vk { get; set; }

        public static KhrSurface KhrSurface { get; private set; }

        public static KhrSwapchain KhrSwapchain { get; private set; }

        public static void AssertVulkan(Result result)
        {
            if (result != Result.Success)
            {
                StackFrame frame = new(1);
                Console.Error.WriteLine("Vulkan function failed with result " + result + " in method " + frame.GetMethod().Name
                    + " in class " + frame.GetMethod().DeclaringType.FullName + " at line " + frame.GetFileLineNumber() + ".");
                Environment.Exit((int)result);
            }
        }

        #region Instance
        private static unsafe void CheckInstanceExtensionsPresent(byte** enabledInstanceExtensions, int enabledInstanceExtensionCount)
        {
            uint instanceExtensionCount = 0;
            AssertVulkan(Vk.EnumerateInstanceExtensionProperties((byte*)null, ref instanceExtensionCount, null));
            Span<ExtensionProperties> properties = stackalloc ExtensionProperties[(int)instanceExtensionCount];
            AssertVulkan(Vk.EnumerateInstanceExtensionProperties((byte*)null, ref instanceExtensionCount, ref properties[0]));
            List<string> names = new();
            foreach (ExtensionProperties prop in properties)
            {
                names.Add(SilkMarshal.PtrToString((nint)prop.ExtensionName));
            }
            for (int i = 0; i < enabledInstanceExtensionCount; i++)
            {
                string name = SilkMarshal.PtrToString((nint)enabledInstanceExtensions[i]);
                if (!names.Contains(name))
                {
                    Console.Error.WriteLine("WARNING: Extension not present! Name: '" + name + "'");
                }
            }
        }

        private static unsafe void CheckInstanceLayersPresent(string[] enabledInstanceLayers)
        {
            uint instanceLayerCount = 0;
            AssertVulkan(Vk.EnumerateInstanceLayerProperties(ref instanceLayerCount, null));
            Span<LayerProperties> props = stackalloc LayerProperties[(int)instanceLayerCount];
            AssertVulkan(Vk.EnumerateInstanceLayerProperties(ref instanceLayerCount, ref props[0]));
            List<string> names = new();
            foreach (LayerProperties prop in props)
            {
                names.Add(SilkMarshal.PtrToString((nint)prop.LayerName));
            }
            foreach (string layer in enabledInstanceLayers)
            {
                if (!names.Contains(layer))
                {
                    Console.Error.WriteLine("WARNING: Layer not present! Name: '" + layer + "'");
                }
            }
        }

        public static unsafe Instance CreateInstance(string[] instanceLayers, string[] instanceExtensions, IVkSurface windowSurface)
        {
            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,

                PApplicationName = (byte*)SilkMarshal.StringToPtr("SilkyNvg-Vulkan-Example"),
                ApplicationVersion = Vk.MakeVersion(1, 0, 0),
                PEngineName = (byte*)SilkMarshal.StringToPtr("SilkyNvg (Renderer: Vulkan)"),
                EngineVersion = Vk.MakeVersion(1, 0, 0),
                ApiVersion = Vk.Version12
            };

            byte** windowExtensionsPtr = windowSurface.GetRequiredExtensions(out uint windowExtensionCount);
            byte** instanceExtensionsPtr = (byte**)SilkMarshal.StringArrayToPtr(instanceExtensions);
            byte** extensions = stackalloc byte*[(int)windowExtensionCount + instanceExtensions.Length];
            int i = 0;
            for (; i < windowExtensionCount; i++)
            {
                extensions[i] = windowExtensionsPtr[i];
            }
            for (; i < windowExtensionCount + instanceExtensions.Length; i++)
            {
                extensions[i] = instanceExtensionsPtr[i];
            }
            CheckInstanceExtensionsPresent(extensions, (int)windowExtensionCount + instanceExtensions.Length);

            CheckInstanceLayersPresent(instanceLayers);
            byte** layers = (byte**)SilkMarshal.StringArrayToPtr(instanceLayers);

            InstanceCreateInfo instanceCreateInfo = VkInit.InstanceCreateInfo(appInfo, layers,
                (uint)instanceLayers.Length, extensions, windowExtensionCount);

            AssertVulkan(Vk.CreateInstance(instanceCreateInfo, null, out Instance instance));
            return instance;
        }

        public static void GetInstanceExtensions()
        {
            Instance instance = Vk.CurrentInstance.Value;

            if (!Vk.TryGetInstanceExtension(instance, out KhrSurface khrSurface))
            {
                throw new Exception();
            }
            KhrSurface = khrSurface;
        }
        #endregion

        #region Device and PhyiscalDevice
        private static unsafe bool CheckDeviceExtensionsPresent(string[] deviceExtensions, PhysicalDevice physicalDevice)
        {
            uint deviceExtensionCount = 0;
            AssertVulkan(Vk.EnumerateDeviceExtensionProperties(physicalDevice, (byte*)null, ref deviceExtensionCount, null));
            Span<ExtensionProperties> props = stackalloc ExtensionProperties[(int)deviceExtensionCount];
            AssertVulkan(Vk.EnumerateDeviceExtensionProperties(physicalDevice, (byte*)null, ref deviceExtensionCount, ref props[0]));
            List<string> names = new();
            foreach (ExtensionProperties prop in props)
            {
                names.Add(SilkMarshal.PtrToString((nint)prop.ExtensionName));
            }
            foreach (string extension in deviceExtensions)
            {
                if (!names.Contains(extension))
                {
                    Console.Error.WriteLine("WARNING: Extension not present. Name: " + extension + ".");
                    return false;
                }
            }
            return true;
        }

        private static unsafe bool FindQueueFamilyIndices(PhysicalDevice physicalDevice, SurfaceKHR surface, out uint? graphicsQueueFamily, out uint? presentQueueFamily)
        {
            graphicsQueueFamily = presentQueueFamily = null;

            uint queueFamilyCount = 0;
            Vk.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, ref queueFamilyCount, null);
            Span<QueueFamilyProperties> props = stackalloc QueueFamilyProperties[(int)queueFamilyCount];
            Vk.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, ref queueFamilyCount, out props[0]);

            for (int i = 0; i < queueFamilyCount; i++)
            {
                QueueFamilyProperties prop = props[i];
                if (prop.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit) && !graphicsQueueFamily.HasValue)
                {
                    graphicsQueueFamily = (uint)i;
                }

                AssertVulkan(KhrSurface.GetPhysicalDeviceSurfaceSupport(physicalDevice, (uint)i, surface, out Bool32 supported));
                if (supported && !presentQueueFamily.HasValue)
                {
                    presentQueueFamily = (uint)i;
                }

                if (graphicsQueueFamily.HasValue && presentQueueFamily.HasValue)
                {
                    return true;
                }
            }

            return false;
        }

        private static unsafe bool CheckSwapchainSupported(PhysicalDevice physicalDevice, SurfaceKHR surface, out (SurfaceCapabilitiesKHR, SurfaceFormatKHR[], PresentModeKHR[]) swapchainData)
        {
            swapchainData = default;

            AssertVulkan(KhrSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, out swapchainData.Item1));

            uint surfaceFormatCount = 0;
            KhrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref surfaceFormatCount, null);
            if (surfaceFormatCount < 1)
            {
                return false;
            }
            swapchainData.Item2 = new SurfaceFormatKHR[surfaceFormatCount];
            AssertVulkan(KhrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref surfaceFormatCount, out swapchainData.Item2[0]));

            uint presentModeCount = 0;
            AssertVulkan(KhrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, null));
            if (presentModeCount == 0)
            {
                return false;
            }
            swapchainData.Item3 = new PresentModeKHR[presentModeCount];
            AssertVulkan(KhrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, out swapchainData.Item3[0]));

            return true;
        }

        private static uint GetPhysicalDeviceScore(PhysicalDevice physicalDevice, string[] deviceExtensions, SurfaceKHR surface,
            out uint graphicsQueueFamily, out uint presentQueuFamily, out (SurfaceCapabilitiesKHR, SurfaceFormatKHR[], PresentModeKHR[]) swapchainData)
        {
            graphicsQueueFamily = presentQueuFamily = 0;
            swapchainData = default;

            Vk.GetPhysicalDeviceProperties(physicalDevice, out PhysicalDeviceProperties properties);

            uint score = uint.MinValue;

            // Extension Presence
            if (!CheckDeviceExtensionsPresent(deviceExtensions, physicalDevice))
            {
                return score;
            }

            // Queue Families
            if (!FindQueueFamilyIndices(physicalDevice, surface, out uint? gQueueFamily, out uint? pQueueFamily))
            {
                return score;
            }
            graphicsQueueFamily = gQueueFamily.Value;
            presentQueuFamily = pQueueFamily.Value;

            // Swapchain Data
            if (!CheckSwapchainSupported(physicalDevice, surface, out swapchainData))
            {
                return score;
            }

            score = 0; // if it has the queue families, it is a valid GPU. Rate depending on non-essential stuff now.

            // Add Score depending on type now.
            uint[] gpuTypeScores =
            {
                5, // Use Other if exists. (If Other exists, most likely want to use it).
                2, // Use Integrated GPU only if nothing else is available.
                3, // Discrete GPU is standard and should be used rather than Integrated GPU or CPU.
                4, // If Virtual GPU Exists, most likely want to use it.
                1 // Use CPU only if nothing else is available.
            };
            score += gpuTypeScores[(int)properties.DeviceType];

            return score;
        }

        public static unsafe PhysicalDevice PickPhysicalDevice(string[] deviceExtensions, SurfaceKHR surface, out uint graphicsQueueFamily,
            out uint presentQueueFamily, out (SurfaceCapabilitiesKHR, SurfaceFormatKHR[], PresentModeKHR[]) swapchainData)
        {
            Instance instance = Vk.CurrentInstance.Value;

            graphicsQueueFamily = presentQueueFamily = 0;
            swapchainData = default;

            uint physicalDeviceCount = 0;
            AssertVulkan(Vk.EnumeratePhysicalDevices(instance, ref physicalDeviceCount, null));
            Span<PhysicalDevice> physicalDevices = stackalloc PhysicalDevice[(int)physicalDeviceCount];
            AssertVulkan(Vk.EnumeratePhysicalDevices(instance, ref physicalDeviceCount, ref physicalDevices[0]));

            PhysicalDevice bestPhysicalDevice = default;
            uint bestPhysicalDeviceScore = uint.MinValue;

            foreach (PhysicalDevice physicalDevice in physicalDevices)
            {
                uint score;
                if ((score = GetPhysicalDeviceScore(physicalDevice, deviceExtensions, surface, out graphicsQueueFamily, out presentQueueFamily, out swapchainData)) > bestPhysicalDeviceScore)
                {
                    bestPhysicalDevice = physicalDevice;
                    bestPhysicalDeviceScore = score;
                }
            }

            if (bestPhysicalDeviceScore < 0)
            {
                throw new Exception("No suitable physical device found.");
            }

            return bestPhysicalDevice;
        }

        public static unsafe Device CreateDevice(string[] deviceExtensions, PhysicalDevice physicalDevice, uint graphicsQueueFamily, uint presentQueueFamily)
        {
            DeviceCreateInfo deviceCreateInfo;
            if (graphicsQueueFamily == presentQueueFamily)
            {
                DeviceQueueCreateInfo queueCreateInfo = VkInit.QueueCreateInfo(graphicsQueueFamily, 2, 1.0f, 1.0f);
                deviceCreateInfo = VkInit.DeviceCreateInfo(deviceExtensions, queueCreateInfo);
            }
            else
            {
                DeviceQueueCreateInfo graphicsQueueCreateInfo = VkInit.QueueCreateInfo(graphicsQueueFamily, 1, 1.0f);
                DeviceQueueCreateInfo presentQueueCreateInfo = VkInit.QueueCreateInfo(presentQueueFamily, 1, 1.0f);
                deviceCreateInfo = VkInit.DeviceCreateInfo(deviceExtensions, graphicsQueueCreateInfo, presentQueueCreateInfo);
            }

            AssertVulkan(Vk.CreateDevice(physicalDevice, deviceCreateInfo, null, out Device device));
            return device;
        }

        public static void GetDeviceExtensions()
        {
            Instance instance = Vk.CurrentInstance.Value;
            Device device = Vk.CurrentDevice.Value;

            if (!Vk.TryGetDeviceExtension(instance, device, out KhrSwapchain khrSwapchain))
            {
                throw new Exception();
            }
            KhrSwapchain = khrSwapchain;
        }
        #endregion


        public static uint FindMemoryTypeIndex(uint typeFilter, MemoryPropertyFlags properties, PhysicalDeviceMemoryProperties memoryProperties)
        {
            for (uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
            {
                if (((typeFilter & 1) == 1) & ((memoryProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties))
                {
                    return i;
                }
            }

            throw new MissingMemberException("No compatible memory type found!");
        }

    }
}
