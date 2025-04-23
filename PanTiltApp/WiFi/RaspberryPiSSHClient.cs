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
        /// Łączy się z Raspberry Pi przez SSH
        /// </summary>
        public bool Connect()
        {
            try
            {
                client.Connect();
                ConsolePrint?.Invoke("Połączono z Raspberry Pi!", "green");
                return true;
            }
            catch (Exception ex)
            {
                ConsolePrint?.Invoke($"Błąd połączenia: {ex.Message}", "red");
                return false;
            }
        }

        /// <summary>
        /// Wykonuje podaną komendę na Raspberry Pi
        /// </summary>
        public void ExecuteCommand(string command)
        {
            if (!client.IsConnected)
                return;           

            try
            {
                var cmd = client.CreateCommand(command);
                string output = cmd.Execute();
                return;
            }
            catch (Exception ex)
            {
                ConsolePrint?.Invoke($"Błąd podczas wykonywania komendy: {ex.Message}", "red");
            }
        }

        /// <summary>
        /// Rozłącza się z Raspberry Pi
        /// </summary>
        public void Disconnect()
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
        }

        /// <summary>
        /// Uruchamia serwer TCP na Raspberry Pi
        /// </summary>
        public void StartServer()
        {
            string command = "sudo python3 /home/pan-tilt/Documents/apka/server.py";
            ExecuteCommand(command);
        }
    }
}
