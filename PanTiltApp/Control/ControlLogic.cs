using PanTiltApp.AppConsole;

namespace PanTiltApp.Operate
{
    public class ControlLogic
    {
        private (float x, float y) lastJoystickPosition;
        private System.Threading.Timer? commandLoopTimer;
        private readonly int intervalMs = 500;

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
            lastJoystickPosition = movement;

            bool isCentered = Math.Abs(movement.x) < 0.05f && Math.Abs(movement.y) < 0.05f;

            if (isCentered)
            {
                // Stop sending commands
                commandLoopTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                if (commandLoopTimer == null)
                {
                    commandLoopTimer = new System.Threading.Timer(SendCommandFromJoystick, null, 0, intervalMs);
                }
                else
                {
                    // Restart timer if necessary
                    commandLoopTimer.Change(0, intervalMs);
                }
            }
        }

        private void SendCommandFromJoystick(object? state)
        {
            float x = lastJoystickPosition.x;
            float y = lastJoystickPosition.y;

            // dead zone check again for safety
            if (Math.Abs(x) < 0.05f && Math.Abs(y) < 0.05f)
                return;

            int positionStep = 2;
            int minSpeed = 200;
            int maxSpeed = 1000;

            if (Math.Abs(y) >= 0.05f)
            {
                int pitchDir = y > 0 ? -1 : 1;
                int pitchSpeed = minSpeed + (int)(Math.Min(1.0f, Math.Abs(y)) * (maxSpeed - minSpeed));
                console.dispatcher?.SendServoFrame(1, pitchDir * positionStep, pitchSpeed);
            }

            if (Math.Abs(x) >= 0.05f)
            {
                int yawDir = x > 0 ? 1 : -1;
                int yawSpeed = minSpeed + (int)(Math.Min(1.0f, Math.Abs(x)) * (maxSpeed - minSpeed));
                console.dispatcher?.SendServoFrame(2, yawDir * positionStep, yawSpeed);
            }
        }
    }
}
