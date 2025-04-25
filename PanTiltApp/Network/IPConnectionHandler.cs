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
                List<byte> buffer = new();
                byte[] readBuffer = new byte[1024];

                while (_isReceiving && _networkStream != null)
                {
                    int bytesRead = await _networkStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                    if (bytesRead <= 0)
                    {
                        Close();
                        return;
                    }

                    for (int i = 0; i < bytesRead; i++)
                        buffer.Add(readBuffer[i]);

                    while (buffer.Count >= 8)
                    {
                        // Sprawdź, czy mamy potencjalną ramkę binarną
                        if (buffer[0] == 0xAA && (buffer[7] & 0x0F) == 0x05)
                        {
                            byte[] frame = buffer.GetRange(0, 8).ToArray();
                            buffer.RemoveRange(0, 8);
                            // ParseFrame(frame);  // ← binarna analiza
                            // ConsolePrint?.Invoke($"[RX] Binary frame: {BitConverter.ToString(frame)}", "green");
                        }
                        else
                        {
                            // ConsolePrint?.Invoke($"[DEBUG] Non-binary data: {BitConverter.ToString(buffer.ToArray())}", "yellow");
                            break;  // nie binarna → sprawdzimy string
                        }
                    }

                    // Próbujemy sparsować pozostałe dane jako string (debug)
                    if (buffer.Count > 0)
                    {
                        try
                        {
                            string debugText = Encoding.ASCII.GetString(buffer.ToArray());
                            if (!string.IsNullOrWhiteSpace(debugText))
                                ConsolePrint?.Invoke($"[DEBUG] {debugText}", "yellow");

                            buffer.Clear();  // Zakładamy że nie będą się mieszać ramki binarne i stringi
                        }
                        catch (Exception ex)
                        {
                            ConsolePrint?.Invoke($"[DecodeError] {ex.Message}", "red");
                            buffer.Clear(); // czyszczenie na wszelki wypadek
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsolePrint?.Invoke($"Connection lost: {ex.Message}", "red");
                Close();
            }
        }


        private void ParseFrame(byte[] frame)
        {
            byte id = frame[1];
            byte cmd = frame[2];
            short pos = BitConverter.ToInt16(frame, 3);
            short spd = BitConverter.ToInt16(frame, 5);

            if ((id & 0xF0) == 0xB0)
            {
                int servoId = id & 0x0F;
                ConsolePrint?.Invoke($"[MOVING] Servo {servoId}: Pos={pos}, Spd={spd}", "cyan");
            }
            else if ((id & 0xF0) == 0xC0)
            {
                int servoId = id & 0x0F;
                ConsolePrint?.Invoke($"[FRAME PARSED] Servo {servoId}: CMD={cmd}, Pos={pos}, Spd={spd}", "purple");
            }
            else
            {
                ConsolePrint?.Invoke($"[UNKNOWN FRAME] ID={id:X2}, CMD={cmd:X2}, Pos={pos}, Spd={spd}", "gray");
            }
        }


        public void Close()
        {
            if (!IsConnected)
                return;

            try
            {
                _isReceiving = false;
                _networkStream?.Close();
                _client?.Close();
                ConsolePrint?.Invoke("Rozłączono połączenie IP z Raspberry Pi.", "yellow");
            }
            catch
            {
                // NIE pokazujemy błędu jeśli połączenie i tak już nie istniało.
                // Można dodać log do pliku, jeśli chcesz mieć monitoring w tle.
            }
            finally
            {
                _client = null;
                _networkStream = null;
            }
        }

        public void Send(byte[] data)
        {
            if (!IsConnected || _networkStream == null)
                ConsolePrint?.Invoke("Brak połączenia z serwerem.", "red");
            try
            {
                _networkStream.Write(data, 0, data.Length);
                ConsolePrint?.Invoke($"Wysłano ramkę binarną: {BitConverter.ToString(data)}", "green");
            }
            catch (Exception ex)
            {
                ConsolePrint?.Invoke($"Błąd wysyłania ramki binarnej: {ex.Message}", "red");
            }
        }
    }
}
