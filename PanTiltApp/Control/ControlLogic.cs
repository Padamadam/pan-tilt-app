using PanTiltApp.AppConsole;


// TODO sterowanie predkosciami a nie polozeniami
// TODO zmiana struktury ramek - rownolegle info o obu serwach!!!!

namespace PanTiltApp.Operate
{
    public class ControlLogic
    {
        private (float x, float y) lastJoystickPosition;
        private System.Threading.Timer? commandLoopTimer;
        private readonly int intervalMs = 50;

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

            if (Math.Abs(x) < 0.05f && Math.Abs(y) < 0.05f)
                return;

            int minSpeed = 200;
            int maxSpeed = 1000;

            int pitchSpeed = 0;
            int yawSpeed = 0;
            int pitchDir = 1;
            int yawDir = 1;

            byte cmd = 0x00;
            if (y < -0.05f) { pitchDir = -1; cmd |= 0b1100; }
            if (x < -0.05f) { yawDir   = -1; cmd |= 0b0011; }


            if (Math.Abs(y) >= 0.05f)
                pitchSpeed = minSpeed + (int)(Math.Min(1.0f, Math.Abs(y)) * (maxSpeed - minSpeed));

            if (Math.Abs(x) >= 0.05f)
                yawSpeed = minSpeed + (int)(Math.Min(1.0f, Math.Abs(x)) * (maxSpeed - minSpeed));

                ushort pitchVel = (ushort)Math.Clamp(Math.Abs(pitchSpeed), 0, 4095);
                ushort yawVel   = (ushort)Math.Clamp(Math.Abs(yawSpeed), 0, 4095);

                ushort pitchPos = 0; // joystick nie ustawia pozycji
                ushort yawPos   = 0;

                console.dispatcher?.SendDualServoFullFrame(
                    pitchPos, yawPos,
                    pitchVel, yawVel,
                    cmd
             );
        }
    }
}
