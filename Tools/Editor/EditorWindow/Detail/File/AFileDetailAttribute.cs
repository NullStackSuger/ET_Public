namespace ET.Editor;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AFileDetailAttribute : BaseAttribute
{
    public string Extension { get; }

    public AFileDetailAttribute(string extension)
    {
        Extension = extension;
    }
}