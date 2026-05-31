using System.Windows.Input;

namespace TanksGame
{
    public class InputManager
    {
        public bool IsUpPressed { get; private set; }
        public bool IsDownPressed { get; private set; }
        public bool IsLeftPressed { get; private set; }
        public bool IsRightPressed { get; private set; }
        public bool IsShootPressed { get; private set; }

        public void UpdateKey(Key key, bool isPressed)
        {
            switch (key)
            {
                case Key.W:
                case Key.Up:
                    IsUpPressed = isPressed;
                    break;
                case Key.S:
                case Key.Down:
                    IsDownPressed = isPressed;
                    break;
                case Key.A:
                case Key.Left:
                    IsLeftPressed = isPressed;
                    break;
                case Key.D:
                case Key.Right:
                    IsRightPressed = isPressed;
                    break;
                case Key.Space:
                    IsShootPressed = isPressed;
                    break;
            }
        }

        public void Reset()
        {
            IsUpPressed = false;
            IsDownPressed = false;
            IsLeftPressed = false;
            IsRightPressed = false;
            IsShootPressed = false;
        }
    }
}