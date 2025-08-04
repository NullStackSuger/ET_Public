namespace ET.Editor;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AComponentDetailAttribute : BaseAttribute
{
    public Type Type { get; }

    public AComponentDetailAttribute(Type type)
    {
        Type = type;
    }
}