using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PanTiltApp.Network
{
    public class IPConnectionHandler
    {
        private string _deviceIp;
        private int _port;
        private TcpClient? _client;
        private NetworkStream? _networkStream;
        private bool _isReceiving;
        public bool IsConnected => _client?.Connected ?? false;
        public event Action<string, string>? ConsolePrint; // Event to print to the app console

        public IPConnectionHandler(string deviceIp, int port)
        {
            _deviceIp = deviceIp;
            _port = port;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_deviceIp, _port);
                _networkStream = _client.GetStream();
                _isReceiving = true;

                ConsolePrint?.Invoke($"Connected to {_deviceIp}:{_port}", "green");
                StartReceiving();

                return true;
            }
            catch (SocketException ex)
            {
                ConsolePrint?.Invoke($"Connection failed: {ex.Message}", "red");
                return false;
            }
        }

        private async void StartReceiving()
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (_isReceiving && _networkStream != null)
                {
                    int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        ConsolePrint?.Invoke($"Received: {receivedData}", "yellow");
                    }
                    else
                    {
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                ConsolePrint?.Invoke($"Connection lost: {ex.Message}", "red");
                Close();
            }
        }

        public void Close()
        {
            try
            {
                _isReceiving = false;
                _networkStream?.Close();
                _client?.Close();
                ConsolePrint?.Invoke("Disconnected", "red");
            }
            catch (Exception ex)
            {
                ConsolePrint?.Invoke($"Error closing connection: {ex.Message}", "red");
            }
            finally
            {
                _client = null;
                _networkStream = null;
            }
        }
    }
}
