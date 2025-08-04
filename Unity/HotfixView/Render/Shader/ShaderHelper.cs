using System.Runtime.CompilerServices;
using Veldrid;

namespace ET.Client;

public static class ShaderHelper
{
    public static void UpdateUniform<T>(this GraphicsDevice device, DeviceBuffer buffer, T value) where T : unmanaged
    {
        device.UpdateBuffer(buffer, 0, ref value);
    }

    public static (DeviceBuffer, ResourceLayoutElementDescription) CreateUniform<T>(this GraphicsDevice device, string name, T value) where T : unmanaged
    {
        uint size = (uint)Unsafe.SizeOf<T>();
        DeviceBuffer buffer = device.ResourceFactory.CreateBuffer(new BufferDescription(size, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        var element = new ResourceLayoutElementDescription(name, ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment);
        
        device.UpdateBuffer(buffer, 0, ref value);
        
        return (buffer, element);
    }

    public static (Sampler, ResourceLayoutElementDescription, ResourceLayoutElementDescription) CreateTexture(this GraphicsDevice device, string name, Texture texture)
    {
        Sampler sampler = device.ResourceFactory.CreateSampler(new SamplerDescription(SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerFilter.MinLinear_MagLinear_MipPoint, null, 0, 0, 0, 0, SamplerBorderColor.OpaqueBlack));
        var textureElement = new ResourceLayoutElementDescription(name, ResourceKind.TextureReadOnly, ShaderStages.Vertex | ShaderStages.Fragment);
        var samplerElement = new ResourceLayoutElementDescription($"{name}Sampler", ResourceKind.Sampler, ShaderStages.Vertex | ShaderStages.Fragment);
        
        return (sampler, textureElement, samplerElement);
    }

    public static void UpdateTexture(this GraphicsDevice device, Texture texture, string name, byte[] value)
    {
        device.UpdateTexture(texture, value, 0, 0, 0, texture.Width, texture.Height, 1, 0, 0);
    }

    public static ResourceLayout CreateResourceLayout(this GraphicsDevice device, ResourceLayoutElementDescription[] arr)
    {
        return device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(arr));
    }

    public static ResourceSet CreateResourceSet(this GraphicsDevice device, ResourceLayout layout, BindableResource[] arr)
    {
        return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout, arr));
    }
}