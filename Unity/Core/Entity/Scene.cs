using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace ET;

[EnableMethod]
[ChildOf]
public class Scene: Entity, IScene, ISerialize, IDeserialize
{
    [BsonIgnore]
    public Fiber Fiber { get; set; }
        
    public int SceneType { get; set; }

    public Scene()
    {
    }

    public Scene(Fiber fiber, int sceneType, long id, long instanceId)
    {
        this.Fiber = fiber;
        this.SceneType = sceneType;
        this.Id = id;
        this.InstanceId = instanceId;
        
        this.IsNew = true;

        this.IScene = this;
        
        Log.Instance.Info($"scene create: {this.SceneType} {this.Id} {this.InstanceId}");
    }

    public override void Dispose()
    {
        base.Dispose();
            
        Log.Instance.Info($"scene dispose: {this.SceneType} {this.Id} {this.InstanceId}");
    }
}