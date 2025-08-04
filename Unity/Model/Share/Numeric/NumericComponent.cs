using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET;

public partial class NumericComponent : Entity, IAwake, ISerialize, IDeserialize
{
    [BsonElement]
    [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
    public Dictionary<int, long> NumericDic = new();
}