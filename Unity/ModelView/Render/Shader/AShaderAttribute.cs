namespace ET.Client;

public class AShaderAttribute : BaseAttribute
{
    public Type Type { get; }

    public AShaderAttribute(Type renderPass)
    {
        this.Type = renderPass;
    }
}