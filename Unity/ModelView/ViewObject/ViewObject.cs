namespace ET.Client;

public class ViewObject : Entity, IAwake<string>, ISerialize, IDeserialize
{
    public string name;
}