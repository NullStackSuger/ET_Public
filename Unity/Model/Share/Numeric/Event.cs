namespace ET;

public struct NumericChange
{
    public EntityRef<Entity> entity;
    public int numericType;
    public long oldValue;
    public long newValue;
}

public struct NumericAffect
{
    public Dictionary<int, long> numericDic;
    
    public int numericType;
    public long oldValue;
    public long newValue;
    
    public int affectNumericType;
    public long affectValue;

    public Dictionary<int, long> D => numericDic;
    public int NT => numericType;
    public long O => oldValue;
    public long N => newValue;
    public int AT => affectNumericType;
    public long AV => affectValue;
}

[Event(SceneType.All)]
public partial class NumericChangeHandler : AEvent<Scene, NumericChange>
{
    protected override async ETTask Run(Scene scene, NumericChange args)
    {
        await NumericHandlerDispatcher.Instance.Run(args);
    }
}