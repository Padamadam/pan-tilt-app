using PanTiltApp.AppConsole;

namespace PanTiltApp.Operate
{
    public class ControlLogic
    {
        private readonly AppConsoleLogic console;

        public ControlLogic(AppConsoleLogic console)
        {
            this.console = console;
        }

        public void ConnectJoystick(ControlUI ui)
        {
            ui.JoystickMoved += OnJoystickMoved;
        }

        private void OnJoystickMoved(object? sender, (float x, float y) movement)
        {
            float x = movement.x;
            float y = movement.y;

            // Założenie: środek to 2048 (środek zakresu 0–4095)
            int center = 2048;
            int maxDelta = 1500; // jak daleko może się wychylić z centrum

            int pitch = center - (int)(y * maxDelta); // oś Y - góra/dół
            int yaw = center + (int)(x * maxDelta);   // oś X - lewo/prawo

            int speed = 1000; // stała prędkość

            console.SendServoFrame(1, pitch, speed); // ID 1 = pitch
            console.SendServoFrame(2, yaw, speed);   // ID 2 = yaw
        }
    }
}
