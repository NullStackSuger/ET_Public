using Veldrid;

namespace ET.Client;

public class MeshRenderInfo
{
    public Pipeline pipeline;
    public ResourceSet resourceSet;
    
    public DeviceBuffer vertexBuffer;
    public DeviceBuffer indexBuffer;
    
    public Dictionary<string, DeviceBuffer> uniformBuffers = new();
    public Dictionary<string, Texture> textures = new();
    public Dictionary<string, Sampler> samplers = new();

    public List<ResourceLayoutElementDescription> elements = new();
    public List<BindableResource> binds = new();
}