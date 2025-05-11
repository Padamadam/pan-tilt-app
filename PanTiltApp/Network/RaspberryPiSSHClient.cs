using Renci.SshNet;
using System;
using System.Drawing;
using System.Windows.Forms;
using PanTiltApp.AppConsole;

namespace PanTiltApp.Network
{
    public class RaspberryPiSSHClient : UserControl
    {
        private string host;
        private string username;
        private string password;
        private SshClient client;
        public bool IsConnected => client.IsConnected;
        public event Action<string, string>? ConsolePrint;

        public RaspberryPiSSHClient(string host, string username, string password)
        {
            this.host = host;
            this.username = username;
            this.password = password;
            client = new SshClient(host, username, password);
        }


        /// <summary>
        /// Connects to Raspberry Pi via SSH
        /// </summary>
        public bool Connect()
        {
            try
            {
                client.Connect();
                ConsolePrint?.Invoke("Connected to the turret.", "green");
                return true;
            }
            catch (Exception ex)
            {
                ConsolePrint?.Invoke($"Connection error: {ex.Message}", "red");
                return false;
            }
        }

        /// <summary>
        /// Executes a given command on Raspberry Pi
        /// </summary>
        public bool ExecuteCommand(string command)
        {
            if (!client.IsConnected)
                return false;           

            try
            {
                var cmd = client.CreateCommand(command);
                string output = cmd.Execute();
                if (!string.IsNullOrEmpty(output))
                    ConsolePrint?.Invoke(output, "gray");
                return true;
            }
            catch (Exception ex)
            {
                ConsolePrint?.Invoke($"Error while executing command: {ex.Message}", "red");
                return false;
            }
        }

        /// <summary>
        /// Starts the TCP server on Raspberry Pi
        /// </summary>
        public void StartServer()
        {
            if (!client.IsConnected)
                return;

            try
            {
                ConsolePrint?.Invoke("Stopping old server.py processes...", "yellow");
                ExecuteCommand("sudo pkill -f /home/pan-tilt/Documents/pan-tilt-rpi/server.py");  // kill old servers
                System.Threading.Thread.Sleep(500);  // give half a second to terminate the process

                ConsolePrint?.Invoke("Checking USB devices...", "yellow");
                ExecuteCommand("ls /dev/ttyUSB* || echo 'No USB device found'");  // check if ESP32 is connected

                // Start server
                if (ExecuteCommand("sudo python3 /home/pan-tilt/Documents/pan-tilt-rpi/server.py"))
                    ConsolePrint?.Invoke("TCP server started on Raspberry Pi...", "green");
            }
            catch (Exception ex)
            {
                ConsolePrint?.Invoke($"Error while starting the server: {ex.Message}", "red");
            }
        }
    }
}
