using System;
using System.Drawing;
using System.Windows.Forms;

namespace PanTiltApp
{
    public class AppConsole : UserControl
    {
        private RichTextBox messageDisplay;
        private TextBox inputBox;
        private Panel consolePanel;


        public AppConsole()
        {
            InitializeConsoleUI();
        }

        private void InitializeConsoleUI()
        {
            this.Dock = DockStyle.Bottom;
            this.Height = 150;
            this.BackColor = Color.Black;
            this.Load += (s, e) => inputBox.Focus();

            consolePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            inputBox = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Courier", 10, FontStyle.Regular),
                TextAlign = HorizontalAlignment.Left
            };



            var inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.Black,
                Padding = new Padding(10)};

            inputPanel.Controls.Add(inputBox);

            inputBox.KeyDown += InputBox_KeyDown;

            var messageDisplayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.Black,
            };

            messageDisplay = new RichTextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BackColor = Color.Black,
                ForeColor = ColorTranslator.FromHtml("#F8F9FA"),
                Font = new Font("Courier", 10, FontStyle.Regular),
                WordWrap = true
                
            };

            messageDisplayPanel.Controls.Add(messageDisplay);
            messageDisplay.MouseDown += (s, e) => inputBox.Focus();

            consolePanel.Controls.Add(inputPanel);       // potem input
            consolePanel.Controls.Add(messageDisplayPanel); // najpierw output

            this.Controls.Add(consolePanel); // dodaj cały panel do UI

        }

        public void PrintMessage(string message, string color = "white")
        {
            Color messageColor = color.ToLower() switch
            {
                "green" => Color.Green,
                "yellow" => Color.Yellow,
                "red" => Color.Red,
                _ => Color.White
            };

            AppendColoredText(message, messageColor);
        }

        private void AppendColoredText(string message, Color color)
        {
            messageDisplay.Invoke((MethodInvoker)delegate
            {
                messageDisplay.SelectionStart = messageDisplay.Text.Length;
                messageDisplay.SelectionLength = 0;
                messageDisplay.SelectionColor = color;
                messageDisplay.AppendText($"{message}{Environment.NewLine}");
                messageDisplay.SelectionColor = messageDisplay.ForeColor;
                messageDisplay.ScrollToCaret();
            });
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string command = inputBox.Text.Trim();
                inputBox.Clear();

                if (string.IsNullOrWhiteSpace(command))
                    return;

                string timestamp = DateTime.Now.ToString("[HH:mm:ss]");

                AppendColoredText($"{timestamp} {command}", Color.White);

                string response = ExecuteCommand(command);
                AppendColoredText(response, Color.Gray);
            }
        }

        private string ExecuteCommand(string command)
        {
            switch (command.ToLower())
            {
                case "help":
                    return "Available commands: help, status, clear";
                case "status":
                    return "System status: All systems operational.";
                case "clear":
                    messageDisplay.Clear();
                    return "";
                default:
                    return $"'{command}' is not recognized as an internal or external command,\noperable program or batch file.";
            }
        }
    }
}
