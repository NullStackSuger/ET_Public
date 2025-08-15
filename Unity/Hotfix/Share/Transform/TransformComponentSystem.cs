using System.Numerics;

namespace ET.Client;

[EntitySystemOf(typeof(TransformComponent))]
public static partial class TransformComponentSystem
{
    [EntitySystem]
    private static void Awake(this TransformComponent self, Vector3 a, Quaternion b, Vector3 c)
    {
        self.localPosition = a;
        self.localRotation = b;
        self.localScale = c;
    }

    [EntitySystem]
    private static void Awake(this TransformComponent self, Vector3 a)
    {
        self.localPosition = a;
        self.localRotation = Quaternion.Identity;
        self.localScale = Vector3.One;
    }
}