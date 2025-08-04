namespace ET.Client;

public class DirtyMeshComponent : Entity, IAwake
{
    public Queue<EntityRef<ViewObject>> dirtyMeshes = new();
}