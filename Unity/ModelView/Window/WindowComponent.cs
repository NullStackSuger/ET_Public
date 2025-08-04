using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace ET.Client;

// 注意要和InputComponent一起使用, 不InputSnapshot() window会未响应
public class WindowComponent : Entity, IAwake<Vector2, Vector2, WindowState, string>, IAwake, ILateUpdate
{
    public Sdl2Window window;
}