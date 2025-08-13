using System.Numerics;
using ET.Client;
using Veldrid;

namespace ET.Editor;

[EntitySystemOf(typeof(EditorComponent))]
public static partial class EditorComponentSystem
{
    [EntitySystem]
    private static void Awake(this EditorComponent self)
    {
        
    }

    [EntitySystem]
    private static void LateUpdate(this EditorComponent self)
    {
        PerspectiveCameraComponent camera = PerspectiveCameraComponent.Main;
        if (camera == null) return;

        var transform = camera.Parent.GetComponent<TransformComponent>();
        var input = self.Scene().GetComponent<InputComponent>();
        
        if (input.Get(Key.W) == InputState.LongTimeDown)
        {
            transform.position += Vector3.Transform(Vector3.UnitZ, transform.rotation) * 0.2f;
        }

        if (input.Get(Key.S) == InputState.LongTimeDown)
        {
            transform.position -= Vector3.Transform(Vector3.UnitZ, transform.rotation) * 0.2f;
        }
        
        if (input.Get(Key.A) == InputState.LongTimeDown)
        {
            transform.position -= Vector3.Transform(Vector3.UnitX, transform.rotation) * 0.2f;
        }
        
        if (input.Get(Key.D) == InputState.LongTimeDown)
        {
            transform.position += Vector3.Transform(Vector3.UnitX, transform.rotation) * 0.2f;
        }
        
        if (input.Get(Key.ShiftLeft) == InputState.LongTimeDown)
        {
            transform.position -= Vector3.Transform(Vector3.UnitY, transform.rotation) * 0.2f;
        }
        
        if (input.Get(Key.Space) == InputState.LongTimeDown)
        {
            transform.position += Vector3.Transform(Vector3.UnitY, transform.rotation) * 0.2f;
        }

        if (input.Get(Key.E) == InputState.LongTimeDown)
        {
            transform.rotation *= (Vector3.UnitY * 1f).ToQuaternion();
        }
        
        if (input.Get(Key.Q) == InputState.LongTimeDown)
        {
            transform.rotation *= (-Vector3.UnitY * 1f).ToQuaternion();
        }
    }
}

public class EditorComponent : Entity, IAwake, ILateUpdate
{
    
}