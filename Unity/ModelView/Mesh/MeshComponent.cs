using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET.Client;

// 这里定义MeshInfo和MeshRenderInfo是为了像数值组件一样, 方便序列化合并等
public class MeshComponent : Entity, IAwake<string>, IAwake<ushort[], Vector3[], Vector2[], Vector3[], Dictionary<Type, Type>>, ISerialize, IDeserialize
{
    public MeshInfo meshInfo = new();
    
    // RenderPassType, ShaderType
    [BsonDictionaryOptions(DictionaryRepresentation.Document)]
    public Dictionary<Type, Type> shaders;
    
    // ShaderType, Info
    [BsonIgnore]
    public Dictionary<Type, MeshRenderInfo> renderInfos = new();
}