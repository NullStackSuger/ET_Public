using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace ET;

// 我认为ET把BeginInit当作序列化时调用, EndInit当作反序列化时调用有问题
// 正常应该是BeginInit在反序列化之前, EndInit在反序列化之后
// 而ET中会在序列化之前调用BeginInit, 在反序列化之前也调用BeginInit, 但是因为反序列化出来的IScene == null, 就不会执行
public static class MongoHelper
{
    [StaticField]
    private static readonly JsonWriterSettings defaultSettings = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };
        
    public static string ToJson(object obj)
    {
        try
        {
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnSerialize();
            }
            return obj.ToJson(defaultSettings);
        }
        catch (Exception e)
        {
            throw new Exception($"to json error {obj.GetType().FullName}\n{e}");
        }
    }

    public static string ToJson(object obj, JsonWriterSettings settings)
    {
        try
        {
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnSerialize();
            }
            return obj.ToJson(settings);
        }
        catch (Exception e)
        {
            throw new Exception($"to json error {obj.GetType().FullName}\n{e}");
        }
    }

    public static T FromJson<T>(string str)
    {
        try
        {
            T obj = BsonSerializer.Deserialize<T>(str);
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"from json error: {str}\n{e}");
        }
    }

    public static object FromJson(Type type, string str)
    {
        try
        {
            object obj = BsonSerializer.Deserialize(str, type);
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"from json error: {str}\n{e}");
        }
    }

    // 这里要注意,
    // 如果用object作为参数, 比如结构体, 你传进来是当作object了, 走ObjectSerialize, 从而序列化错误
    public static byte[] Serialize<T>(T obj)
    {
        try
        {
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnSerialize();
            }
            return obj.ToBson();
        }
        catch (Exception e)
        {
            throw new Exception($"Serialize error {obj.GetType().FullName}\n{e}");
        }
    }
    public static void Serialize(object obj, MemoryStream stream)
    {
        try
        {
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnSerialize();
            }

            using BsonBinaryWriter bsonWriter = new(stream, BsonBinaryWriterSettings.Defaults);
            
            BsonSerializationContext context = BsonSerializationContext.CreateRoot(bsonWriter);
            BsonSerializationArgs args = default;
            args.NominalType = typeof (object);
            IBsonSerializer serializer = BsonSerializer.LookupSerializer(args.NominalType);
            serializer.Serialize(context, args, obj);
        }
        catch (Exception e)
        {
            throw new Exception($"Serialize error {obj.GetType().FullName}\n{e}");
        }
    }

    public static object Deserialize(Type type, byte[] bytes)
    {
        try
        {
            object obj = BsonSerializer.Deserialize(bytes, type);
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"from bson error: {type.FullName} {bytes.Length}", e);
        }
    }
    public static object Deserialize(Type type, byte[] bytes, int index, int count)
    {
        try
        {
            using MemoryStream memoryStream = new(bytes, index, count);
            object obj = BsonSerializer.Deserialize(memoryStream, type);
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"from bson error: {type.FullName} {bytes.Length} {index} {count}", e);
        }
    }

    public static object Deserialize(Type type, Stream stream)
    {
        try
        {
            object obj = BsonSerializer.Deserialize(stream, type);
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"from bson error: {type.FullName} {stream.Position} {stream.Length}", e);
        }
    }
    public static T Deserialize<T>(byte[] bytes)
    {
        try
        {
            using MemoryStream memoryStream = new(bytes);
            T obj = (T)BsonSerializer.Deserialize(memoryStream, typeof(T));
            if (obj is DisposeObject disposeObject)
            {
                disposeObject.OnDeserialize();
            }
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception($"from bson error: {typeof (T).FullName} {bytes.Length}", e);
        }
    }

    public static T Deserialize<T>(byte[] bytes, int index, int count)
    {
        return (T)Deserialize(typeof (T), bytes, index, count);
    }

    public static T Clone<T>(T t)
    {
        return Deserialize<T>(Serialize(t));
    }
}