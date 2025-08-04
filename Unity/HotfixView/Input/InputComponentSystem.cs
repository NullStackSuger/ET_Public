using Veldrid;

namespace ET.Client;

[EntitySystemOf(typeof(InputComponent))]
public static partial class InputComponentSystem
{
    [EntitySystem]
    private static void Awake(this InputComponent self)
    {
        foreach (Key key in Enum.GetValues(typeof(Key)))
        {
            self.keyStates[key] = InputState.LongTimeUp;
        }

        foreach (MouseButton mouse in Enum.GetValues(typeof(MouseButton)))
        {
            self.mouseStates[mouse] = InputState.LongTimeUp;
        }
    }
    
    // 每次获取的输入都要是上帧的输入, 放在Update的话, 可能EntityA获取旧值 EntityB获取新值
    [EntitySystem]
    private static void LateUpdate(this InputComponent self)
    {
        self.snapshot = self.Scene().GetComponent<WindowComponent>().InputSnapshot();

        foreach (Key key in self.waitToLongTimeUpKeys)
        {
            self.keyStates[key] = InputState.LongTimeUp;
        }

        self.waitToLongTimeUpKeys.Clear();

        foreach (MouseButton mouse in self.waitToLongTimeUpMouses)
        {
            self.mouseStates[mouse] = InputState.LongTimeUp;
        }

        self.waitToLongTimeUpMouses.Clear();

        foreach (Key key in self.waitToLongTimeDownKeys)
        {
            self.keyStates[key] = InputState.LongTimeDown;
        }

        self.waitToLongTimeDownKeys.Clear();

        foreach (MouseButton mouse in self.waitToLongTimeDownMouses)
        {
            self.mouseStates[mouse] = InputState.LongTimeDown;
        }

        self.waitToLongTimeDownMouses.Clear();

        foreach (KeyEvent keyEvent in self.snapshot.KeyEvents)
        {
            Key key = keyEvent.Key;
            bool needDown = keyEvent.Down;

            if (needDown)
            {
                // TODO SDL_ERROR/我16帧按住A, 16帧时KeyEvents里有A, 之后直到21帧, KeyEvents才再次包含A
                // 这里我改了下 用起来是正常的
                // 下面的Mouse也是同理
                //keyStates[key] = (keyStates[key] == InputState.Down || keyStates[key] == InputState.LongTimeDown) ? InputState.LongTimeDown : InputState.Down;
                if (self.keyStates[key] == InputState.Up || self.keyStates[key] == InputState.LongTimeUp)
                {
                    self.keyStates[key] = InputState.Down;
                    self.waitToLongTimeDownKeys.Add(key);
                }
            }
            else
            {
                self.keyStates[key] = InputState.Up;
                self.waitToLongTimeUpKeys.Add(key);
            }
        }

        foreach (MouseEvent mouseEvent in self.snapshot.MouseEvents)
        {
            MouseButton mouse = mouseEvent.MouseButton;
            bool needDown = mouseEvent.Down;

            if (needDown)
            {
                if (self.mouseStates[mouse] == InputState.Up || self.mouseStates[mouse] == InputState.LongTimeUp)
                {
                    self.mouseStates[mouse] = InputState.Down;
                    self.waitToLongTimeDownMouses.Add(mouse);
                }
            }
            else
            {
                self.mouseStates[mouse] = InputState.Up;
                self.waitToLongTimeUpMouses.Add(mouse);
            }
        }
    }
    
    public static InputState Get(this InputComponent self, Key key)
    {
        return self.keyStates[key];
    }
    public static InputState Get(this InputComponent self, MouseButton mouse)
    {
        return self.mouseStates[mouse];
    }
}