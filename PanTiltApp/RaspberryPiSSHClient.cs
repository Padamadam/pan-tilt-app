using Renci.SshNet;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PanTiltApp.Network
{
    public class RaspberryPiSSHClient : UserControl
    {
        private string host;
        private string username;
        private string password;
        private SshClient client;

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
                Console.WriteLine("Połączono z Raspberry Pi!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd połączenia: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Wykonuje podaną komendę na Raspberry Pi
        /// </summary>
        public string ExecuteCommand(string command)
        {
            if (!client.IsConnected)
            {
                Console.WriteLine("Brak połączenia. Połącz się najpierw.");
                return "Błąd: brak połączenia.";
            }

            try
            {
                var cmd = client.CreateCommand(command);
                string output = cmd.Execute();
                Console.WriteLine($"Wynik komendy: {output}");
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas wykonywania komendy: {ex.Message}");
                return $"Błąd: {ex.Message}";
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
                Console.WriteLine("Rozłączono z Raspberry Pi.");
            }
        }

        /// <summary>
        /// Uruchamia serwer TCP na Raspberry Pi
        /// </summary>
        public void StartServer()
        {
            string command = "python3 /home/pan-tilt/Documents/apka/server.py";
            Console.WriteLine("Uruchamiam serwer na Raspberry Pi...");
            string result = ExecuteCommand(command);
            Console.WriteLine(result);
        }
    }
}
