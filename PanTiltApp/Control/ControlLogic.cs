using PanTiltApp.AppConsole;

namespace PanTiltApp.Operate
{
    public class ControlLogic
    {
        private (float x, float y) lastJoystickPosition;
        private System.Threading.Timer? commandLoopTimer;
        private readonly int intervalMs = 200;

        private readonly AppConsoleLogic console;
        private float tolerance = 0.1f; // Tolerancja dla "wycentrowania" joysticka
        private readonly object sendLock = new();
        private bool isSendingEnabled = false;
        


        public ControlLogic(AppConsoleLogic console)
        {
            this.console = console;
            commandLoopTimer = new System.Threading.Timer(SendCommandFromJoystick, null, 0, intervalMs);
        }

        public void EnableSending() => isSendingEnabled = true;
        public void DisableSending() => isSendingEnabled = false;

        public void ConnectJoystick(ControlUI ui)
        {
            ui.JoystickMoved += OnJoystickMoved;
            ui.SwitchToggled += OnSwitchToggled;
        }

        private void OnJoystickMoved(object? sender, (float x, float y) movement)
        {
            lastJoystickPosition = movement;
        }

        private void OnSwitchToggled(object? sender, bool isEnabled)
        {
            if (isEnabled)
                EnableSending();
            else
                DisableSending();
        }


       private void SendCommandFromJoystick(object? state)
        {
            if (!isSendingEnabled)
                return; // Jeśli wysyłka wyłączona, to nic nie rób

            lock (sendLock)
            {
                float x = lastJoystickPosition.x;
                float y = lastJoystickPosition.y;

                bool isCentered = Math.Abs(x) < tolerance && Math.Abs(y) < tolerance;

                if (isCentered)
                {
                        console.dispatcher?.SendDualServoFullFrame(0, 0, 1, 1, 0x00);
                        return;
                }
                // Jeśli joystick wychylony — wysyłamy komendę z aktualną prędkością
                int minPitchSpeed = 1;
                int maxPitchSpeed = 1500;
                int minYawSpeed = 10;
                int maxYawSpeed = 200;

                int pitchSpeed;
                int yawSpeed;
                ushort pitchPos = 150;
                ushort yawPos = 20;

                byte cmd = 0x00;
                if (y < -tolerance) { cmd |= 0b1100; }
                if (x < -tolerance) { cmd |= 0b0011; }

                if (Math.Abs(y) >= tolerance)
                    pitchSpeed = minPitchSpeed + (int)(Math.Min(1.0f, Math.Abs(y)) * (maxPitchSpeed - minPitchSpeed));
                else{
                    console.PrintMessage($"Pitch centered: {y}", "yellow");
                    pitchSpeed = 1;
                    pitchPos = 0;
                }

                if (Math.Abs(x) >= tolerance)
                    yawSpeed = minYawSpeed + (int)(Math.Min(1.0f, Math.Abs(x)) * (maxYawSpeed - minYawSpeed));
                else{
                    yawSpeed = 1;
                    yawPos = 0;
                }

                ushort pitchVel = (ushort)Math.Clamp(Math.Abs(pitchSpeed), 0, 1500);
                ushort yawVel = (ushort)Math.Clamp(Math.Abs(yawSpeed), 0, 1500);

                console.dispatcher?.SendDualServoFullFrame(
                    pitchPos, yawPos,
                    pitchVel, yawVel,
                    cmd
                );
            }
        }


    }

    public class StatusFrame
{
    public byte Cmd { get; private set; }
    public ushort PitchPos { get; private set; }
    public ushort PitchVel { get; private set; }
    public ushort YawPos { get; private set; }
    public ushort YawVel { get; private set; }

    public static StatusFrame? Decode(byte[] buffer)
    {
        if (buffer.Length != 8) return null;
        if (buffer[0] != 0xAA || (buffer[7] & 0x0F) != 0x05) return null;

        StatusFrame frame = new StatusFrame
        {
            Cmd = (byte)((buffer[1] >> 4) & 0x0F),
            PitchPos = (ushort)(((buffer[1] & 0x0F) << 8) | buffer[2]),
            YawPos = (ushort)((buffer[3] << 4) | ((buffer[4] >> 4) & 0x0F)),
            PitchVel = (ushort)(((buffer[4] & 0x0F) << 8) | buffer[5]),
            YawVel = (ushort)((buffer[6] << 4) | ((buffer[7] >> 4) & 0x0F))
        };

        return frame;
    }
}

}
