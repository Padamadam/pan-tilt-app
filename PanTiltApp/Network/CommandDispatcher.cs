using System;
using PanTiltApp.Network;

namespace PanTiltApp.Communication
{
    public class CommandDispatcher
    {
        private readonly IPConnectionHandler connectionHandler;
        private readonly Action<string, string> logAction;

        public CommandDispatcher(IPConnectionHandler handler, Action<string, string> logCallback)
        {
            connectionHandler = handler;
            logAction = logCallback;
        }

        public void SendServoFrame(int id, int position, int speed)
        {
            if (!connectionHandler.IsConnected)
            {
                logAction("Connection not established. Cannot send command.", "red");
                return;
            }

            byte[] frame = new byte[8];
            frame[0] = 0xAA;
            frame[1] = (byte)id;

            byte command = (byte)(position >= 0 ? 0x01 : 0x02);
            frame[2] = command;

            short absPos = (short)Math.Abs(position);
            byte[] posBytes = BitConverter.GetBytes(absPos);
            frame[3] = posBytes[0];
            frame[4] = posBytes[1];

            byte[] spdBytes = BitConverter.GetBytes((short)speed);
            frame[5] = spdBytes[0];
            frame[6] = spdBytes[1];

            frame[7] = 0x55;

            connectionHandler.Send(frame);
            logAction($"Sent Binary Frame: ID={id}, CMD={command} POS={absPos}, SPD={speed}", "green");
        }

        public void SendLaserFrame(bool on)
        {
            if (!connectionHandler.IsConnected)
            {
                logAction("Connection not established. Cannot send laser command.", "red");
                return;
            }

            byte[] frame = new byte[8];
            frame[0] = 0xAA;
            frame[1] = 0x32;
            frame[2] = 0x01;
            frame[3] = on ? (byte)0x01 : (byte)0x00;
            frame[4] = 0x00;
            frame[5] = 0x00;
            frame[6] = 0x00;
            frame[7] = 0x55;

            connectionHandler.Send(frame);
            logAction($"Sent Laser Frame: {(on ? "ON" : "OFF")}", "green");
        }
    }
}
