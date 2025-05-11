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

       public void SendDualServoFullFrame(
            ushort pitchPos, ushort yawPos,
            ushort pitchVel, ushort yawVel,
            byte cmd = 0x00)
        {
            logAction($"Sending command: {cmd:X2} | pitch: pos={pitchPos}, vel={pitchVel} | yaw: pos={yawPos}, vel={yawVel}", "green");
            if (!connectionHandler.IsConnected)
            {
                logAction("Connection not established. Cannot send command.", "red");
                return;
            }

            byte[] buffer = new byte[8];

            buffer[0] = 0xAA;
            buffer[1] = (byte)((cmd & 0x0F) << 4 | ((pitchPos >> 8) & 0x0F));
            buffer[2] = (byte)(pitchPos & 0xFF);
            buffer[3] = (byte)((yawPos >> 4) & 0xFF);
            buffer[4] = (byte)(((yawPos & 0x0F) << 4) | ((pitchVel >> 8) & 0x0F));
            buffer[5] = (byte)(pitchVel & 0xFF);
            buffer[6] = (byte)((yawVel >> 4) & 0xFF);
            buffer[7] = (byte)(((yawVel & 0x0F) << 4) | 0x05); // STOP = 0x05 w low nibble

            connectionHandler.Send(buffer);

            logAction($"[TX] CMD={cmd} | pitch: pos={pitchPos}, vel={pitchVel} | yaw: pos={yawPos}, vel={yawVel}", "green");
        }

        public void SendLaserFrame(bool on)
        {
            if (!connectionHandler.IsConnected)
            {
                // logAction("Connection not established. Cannot send laser command.", "red");
                return;
            }

            byte cmd = on ? (byte)0x05 : (byte)0x06;

            byte[] buffer = new byte[8];
            buffer[0] = 0xAA;
            buffer[1] = (byte)((cmd & 0x0F) << 4);  // reszta bajtów = 0
            buffer[7] = 0x05;

            connectionHandler.Send(buffer);
            // logAction($"[TX] Laser {(on ? "ON" : "OFF")} (CMD={cmd})", "green");
        }

    }
}
