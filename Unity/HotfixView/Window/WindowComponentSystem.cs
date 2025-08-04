using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;

namespace ET.Client;

[EntitySystemOf(typeof(WindowComponent))]
public static partial class WindowComponentSystem
{
    [EntitySystem]
    private static void Awake(this WindowComponent self)
    {
        self.window = VeldridStartup.CreateWindow(new WindowCreateInfo(50, 50, 1920, 1080, WindowState.Maximized, "Game Window"));
    }

    [EntitySystem]
    private static void Awake(this WindowComponent self, Vector2 a, Vector2 b, WindowState c, string d)
    {
        self.window = VeldridStartup.CreateWindow(new WindowCreateInfo((int)a.X, (int)a.Y, (int)b.X, (int)b.Y, c, d));
    }

    [EntitySystem]
    private static void LateUpdate(this WindowComponent self)
    {
        if (self.window is null || !self.window.Exists)
        {
            Options.Instance.NeedClose = true;
            return;
        }
    }

    public static InputSnapshot InputSnapshot(this WindowComponent self)
    {
        return self.window.PumpEvents();
    }

    public static float Aspect(this WindowComponent self)
    {
        return (float)self.window.Width / self.window.Height;
    }
}