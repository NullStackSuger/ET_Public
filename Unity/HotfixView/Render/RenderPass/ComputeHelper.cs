using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.SPIRV;

namespace ET.Client;

public static class ComputeHelper
{
    public static void TestComputeHandler(RenderComponent renderComponent)
    {
        List<ResourceLayoutElementDescription> elements = new();
        List<BindableResource> binds = new();
        Dictionary<string, DeviceBuffer> uniformBuffers = new();
        Dictionary<string, Texture> textures = new(); 
        Dictionary<string, Sampler> samplers = new();
        
        float[] arr = [0, 4, 9, 3, 6, 12, 1, 8, 11, 5, 15, 2, 7, 10, 14, 13];
        renderComponent.AddInputBuffer("InputBuffer", arr, ref elements, ref binds);
        DeviceBuffer outputBuffer = renderComponent.AddOutputBuffer<float>("OutBuffer", arr.Length, ref elements, ref binds);
        
        var resourceLayout = renderComponent.device.CreateResourceLayout(elements.ToArray());
        var resourceSet = renderComponent.device.CreateResourceSet(resourceLayout, binds.ToArray());
        Shader computeShader = renderComponent.LoadCompute("Shaders/Compute");
        var pipeline = renderComponent.device.ResourceFactory.CreateComputePipeline(new ComputePipelineDescription(computeShader, resourceLayout, 1, 0, 0));
        
        renderComponent.commandList.Begin();
        renderComponent.commandList.SetPipeline(pipeline);
        renderComponent.commandList.SetComputeResourceSet(0, resourceSet);
        renderComponent.commandList.Dispatch((uint)arr.Length, 1, 1);
        renderComponent.commandList.End();
        renderComponent.device.SubmitCommands(renderComponent.commandList);
        renderComponent.device.SwapBuffers();
        
        MappedResourceView<float> outputReadView = renderComponent.GetReadback<float>(outputBuffer);
        for (int i = 0; i < arr.Length; i++)
        {
            Log.Instance.Info($"Output: {outputReadView[i]}");
        }
    }

    public static void TestCompute1Handler(RenderComponent renderComponent)
    {
        Texture computeOutput = renderComponent.device.ResourceFactory.CreateTexture(TextureDescription.Texture3D(16, 16, 2, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Sampled | TextureUsage.Storage));
        
        var resourceLayout = renderComponent.device.CreateResourceLayout([new ResourceLayoutElementDescription("img", ResourceKind.TextureReadWrite, ShaderStages.Compute)]);
        var resourceSet = renderComponent.device.CreateResourceSet(resourceLayout, [computeOutput]);
        Shader computeShader = renderComponent.LoadCompute("Shaders/Compute1");
        var pipeline = renderComponent.device.ResourceFactory.CreateComputePipeline(new ComputePipelineDescription(computeShader, resourceLayout, 16, 16, 2));
        
        renderComponent.commandList.Begin();
        renderComponent.commandList.SetPipeline(pipeline);
        renderComponent.commandList.SetComputeResourceSet(0, resourceSet);
        renderComponent.commandList.Dispatch(1, 1, 1);
        renderComponent.commandList.End();
        renderComponent.device.SubmitCommands(renderComponent.commandList);
        renderComponent.device.SwapBuffers();
        
        Texture readback = renderComponent.GetReadback(computeOutput);
        MappedResourceView<RgbaFloat> readView = renderComponent.device.Map<RgbaFloat>(readback, MapMode.Read);
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    if (readView[x, y, z] != RgbaFloat.Green)
                    {
                        Log.Instance.Warning($"({x}, {y}, {z})");
                    }
                }
            }
        }
    }

    /*public static void Atmosphere(RenderComponent renderComponent)
    {
        List<ResourceLayoutElementDescription> elements = new();
        List<BindableResource> binds = new();
        Dictionary<string, DeviceBuffer> uniformBuffers = new();
        Dictionary<string, Texture> textures = new(); 
        Dictionary<string, Sampler> samplers = new();

        // 把太阳辐射度和天空辐射度转为亮度值的系数
        // 类似直接光照和间接光照(天空本身没辐射度, 但是太阳光会和大气中粒子反射)
        Vector3 skySpectralRadianceToLuminance, sunSpectralRadianceToLuminance;
        SkySunRadianceToLuminance(out skySpectralRadianceToLuminance, out sunSpectralRadianceToLuminance);
        // 太阳辐射度
        Vector3 solarIrradiance = ToVector(Wavelengths, SolarIrradiance, lambdas, 1.0);
        // 设置rayleigh相关参数
        Vector3 rayleighScattering = ToVector(Wavelengths, RayleighScattering, lambdas, LengthUnitInMeters);
        BindDensityLayer(compute, RayleighDensity);
        // 设置mie相关参数
        Vector3 mieScattering = ToVector(Wavelengths, MieScattering, lambdas, LengthUnitInMeters);
        Vector3 mieExtinction = ToVector(Wavelengths, MieExtinction, lambdas, LengthUnitInMeters);
        BindDensityLayer(compute, MieDensity);
        // 臭氧吸收, 分为2个区域, 10-25浓度线性增加, 25-40逐渐减少
        Vector3 absorptionExtinction = ToVector(Wavelengths, AbsorptionExtinction, lambdas, LengthUnitInMeters);
        BindDensityLayer(compute, AbsorptionDensity[0]);
        BindDensityLayer(compute, AbsorptionDensity[1]);
        // 地面反射
        Vector3 groundAlbedo = ToVector(Wavelengths, GroundAlbedo, lambdas, 1.0);
        
        Atmosphere_InputBuffer inputBuffer = new Atmosphere_InputBuffer()
        {
            TRANSMITTANCE_TEXTURE_WIDTH = 256,
            TRANSMITTANCE_TEXTURE_HEIGHT = 64,
            SCATTERING_TEXTURE_R_SIZE = 32,
            SCATTERING_TEXTURE_MU_SIZE = 128,
            SCATTERING_TEXTURE_MU_S_SIZE = 32,
            SCATTERING_TEXTURE_NU_SIZE = 8,
            SCATTERING_TEXTURE_WIDTH = 8 * 32,
            SCATTERING_TEXTURE_HEIGHT = 128,
            SCATTERING_TEXTURE_DEPTH = 32,
            IRRADIANCE_TEXTURE_WIDTH = 64,
            IRRADIANCE_TEXTURE_HEIGHT = 16,
            
            SKY_SPECTRAL_RADIANCE_TO_LUMINANCE = skySpectralRadianceToLuminance,
            SUN_SPECTRAL_RADIANCE_TO_LUMINANCE = sunSpectralRadianceToLuminance,
            
            solar_irradiance = solarIrradiance,
            
            rayleigh_scattering = rayleighScattering,
            
            mie_scattering = mieScattering,
            mie_extinction = mieExtinction,
            
            absorption_extinction = absorptionExtinction,
            
            ground_albedo = groundAlbedo,
            
            luminanceFromRadiance = ToMatrix(luminance_from_radiance),
            sun_angular_radius = SunAngularRadius,
            bottom_radius = BottomRadius / LengthUnitInMeters,
            top_radius = TopRadius / LengthUnitInMeters,
            mie_phase_function_g = MiePhaseFunctionG,
            mu_s_min = Math.Cos(MaxSunZenithAngle),
        };
        renderComponent.AddUniformBuffer("inputBuffer", inputBuffer, ref elements, ref binds, ref uniformBuffers);

        Texture transmittance = renderComponent.AddTexture("transmittance", TextureDescription.Texture2D(256, 64, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Sampled | TextureUsage.Storage), ref elements, ref binds);
        
        var resourceLayout = renderComponent.device.CreateResourceLayout(elements.ToArray());
        var resourceSet = renderComponent.device.CreateResourceSet(resourceLayout, binds.ToArray());
        Shader computeShader = renderComponent.LoadCompute("Shaders/ComputeTransmittance");
        var pipeline = renderComponent.device.ResourceFactory.CreateComputePipeline(new ComputePipelineDescription(computeShader, resourceLayout, 8, 8, 1));

        renderComponent.commandList.Begin();
        renderComponent.commandList.SetPipeline(pipeline);
        renderComponent.commandList.SetComputeResourceSet(0, resourceSet);
        renderComponent.commandList.Dispatch(256 / 8, 64 / 8, 1);
        renderComponent.commandList.End();
        renderComponent.device.SubmitCommands(renderComponent.commandList);
        renderComponent.device.SwapBuffers();
        
        Texture readback = renderComponent.GetReadback(transmittance);
        MappedResourceView<RgbaFloat> readView = renderComponent.device.Map<RgbaFloat>(readback, MapMode.Read);
    }*/

    private static DeviceBuffer AddInputBuffer<T>(this RenderComponent renderComponent, string name, T[] resources, ref List<ResourceLayoutElementDescription> elements, ref List<BindableResource> binds) where T : unmanaged
    {
        DeviceBuffer buffer = renderComponent.device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<T>() * resources.Length), BufferUsage.StructuredBufferReadWrite, (uint)Unsafe.SizeOf<T>(), true));
        renderComponent.device.UpdateBuffer(buffer, 0, resources);
        binds.Add(buffer);
        elements.Add(new ResourceLayoutElementDescription(name, ResourceKind.StructuredBufferReadWrite, ShaderStages.Compute));
        return buffer;
    }

    private static DeviceBuffer AddOutputBuffer<T>(this RenderComponent renderComponent, string name, int length, ref List<ResourceLayoutElementDescription> elements, ref List<BindableResource> binds) where T : unmanaged
    {
        DeviceBuffer buffer = renderComponent.device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<T>() * length), BufferUsage.StructuredBufferReadWrite, (uint)Unsafe.SizeOf<T>(), true));
        binds.Add(buffer);
        elements.Add(new ResourceLayoutElementDescription(name, ResourceKind.StructuredBufferReadWrite, ShaderStages.Compute));
        return buffer;
    }

    private static DeviceBuffer AddUniformBuffer<T>(this RenderComponent renderComponent, string name, T resources, ref List<ResourceLayoutElementDescription> elements, ref List<BindableResource> binds, ref Dictionary<string, DeviceBuffer> uniformBuffers) where T : unmanaged
    {
        (DeviceBuffer buffer, ResourceLayoutElementDescription element) = renderComponent.device.CreateUniform(name, resources);
        uniformBuffers[name] = buffer;
        binds.Add(buffer);
        elements.Add(element);
        return buffer;
    }

    private static Texture AddTexture(this RenderComponent renderComponent, string name, TextureDescription textureDescription, ref List<ResourceLayoutElementDescription> elements, ref List<BindableResource> binds)
    {
        Texture texture = renderComponent.device.ResourceFactory.CreateTexture(textureDescription);
        elements.Add(new ResourceLayoutElementDescription(name, ResourceKind.TextureReadWrite, ShaderStages.Compute));
        binds.Add(texture);
        return texture;
    }
    
    private static Shader LoadCompute(this RenderComponent renderComponent, string setName)
    {
        return renderComponent.device.ResourceFactory.CreateFromSpirv(
            new ShaderDescription(ShaderStages.Compute, File.ReadAllBytes($"{setName}.comp.spv"), "main"),
            new CrossCompileOptions(false, false, new SpecializationConstant[]
            {
                new SpecializationConstant(100, false)
            }));
    }
    
    private static MappedResourceView<T> GetReadback<T>(this RenderComponent renderComponent, DeviceBuffer buffer) where T : unmanaged
    {
        DeviceBuffer readback;
        if ((buffer.Usage & BufferUsage.Staging) != 0)
        {
            readback = buffer;
        }
        else
        {
            readback = renderComponent.device.ResourceFactory.CreateBuffer(new BufferDescription(buffer.SizeInBytes, BufferUsage.Staging));
            CommandList cl = renderComponent.device.ResourceFactory.CreateCommandList();
            cl.Begin();
            cl.CopyBuffer(buffer, 0, readback, 0, buffer.SizeInBytes);
            cl.End();
            renderComponent.device.SubmitCommands(cl);
            renderComponent.device.WaitForIdle();
        }

        return renderComponent.device.Map<T>(readback, MapMode.Read);
    }
    
    private static Texture GetReadback(this RenderComponent renderComponent, Texture texture)
    {
        if ((texture.Usage & TextureUsage.Staging) != 0)
        {
            return texture;
        }
        else
        {
            uint layers = texture.ArrayLayers;
            if ((texture.Usage & TextureUsage.Cubemap) != 0)
            {
                layers *= 6;
            }
            TextureDescription desc = new TextureDescription(
                texture.Width, texture.Height, texture.Depth,
                texture.MipLevels, layers,
                texture.Format,
                TextureUsage.Staging, texture.Type);
            Texture readback = renderComponent.device.ResourceFactory.CreateTexture(ref desc);
            CommandList cl = renderComponent.device.ResourceFactory.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(texture, readback);
            cl.End();
            renderComponent.device.SubmitCommands(cl);
            renderComponent.device.WaitForIdle();
            return readback;
        }
    }
}