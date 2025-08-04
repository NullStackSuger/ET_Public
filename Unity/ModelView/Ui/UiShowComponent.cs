using System.Numerics;

namespace ET.Client;

public class UiShowComponent : Entity, IAwake, IUi
{
    public IntPtr renderView;
    public Vector2 size;
}