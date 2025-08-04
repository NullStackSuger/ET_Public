using Veldrid;

namespace ET.Client;

public enum InputState
{
    Up, Down, LongTimeDown, LongTimeUp
}

public class InputComponent : Entity, IAwake, ILateUpdate
{
    public readonly Dictionary<Key, InputState> keyStates = new();
    public readonly HashSet<Key> waitToLongTimeUpKeys = new();
    public readonly HashSet<Key> waitToLongTimeDownKeys = new();
    public readonly Dictionary<MouseButton, InputState> mouseStates = new();
    public readonly HashSet<MouseButton> waitToLongTimeUpMouses = new();
    public readonly HashSet<MouseButton> waitToLongTimeDownMouses = new();
    
    public InputSnapshot snapshot;
}