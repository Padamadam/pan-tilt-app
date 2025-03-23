using System;
using System.Drawing;
using System.Windows.Forms;

namespace PanTiltApp.AppConsole
{
    public class AppConsoleUI : UserControl
    {
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Content)]
        public RichTextBox MessageDisplay { get; private set; }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Content)]
        public TextBox InputBox { get; private set; }

        private Panel consolePanel;

        public event EventHandler<string>? CommandSubmitted;

        public AppConsoleUI()
        {
            InitializeConsoleUI();
        }

        private void InitializeConsoleUI()
        {
            this.Dock = DockStyle.Bottom;
            this.Height = 150;
            this.BackColor = Color.Black;
            this.Load += (s, e) => InputBox.Focus();

            consolePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            InputBox = new TextBox
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
                Padding = new Padding(10)
            };

            inputPanel.Controls.Add(InputBox);

            InputBox.KeyDown += InputBox_KeyDown;

            var messageDisplayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.Black,
            };

            MessageDisplay = new RichTextBox
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

            messageDisplayPanel.Controls.Add(MessageDisplay);
            MessageDisplay.MouseDown += (s, e) => InputBox.Focus();

            consolePanel.Controls.Add(inputPanel);
            consolePanel.Controls.Add(messageDisplayPanel);

            this.Controls.Add(consolePanel);
        }

        private void InputBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string command = InputBox.Text.Trim();
                InputBox.Clear();

                if (!string.IsNullOrWhiteSpace(command))
                {
                    string timestamp = DateTime.Now.ToString("[HH:mm:ss]");
                    AppendColoredText($"{timestamp} {command}", Color.White);
                    CommandSubmitted?.Invoke(this, command); // delegujemy do logiki
                }
            }
        }

        public void AppendColoredText(string message, Color color)
        {
            MessageDisplay.Invoke((MethodInvoker)delegate
            {
                MessageDisplay.SelectionStart = MessageDisplay.Text.Length;
                MessageDisplay.SelectionLength = 0;
                MessageDisplay.SelectionColor = color;
                MessageDisplay.AppendText($"{message}{Environment.NewLine} \u200B{Environment.NewLine}");
                MessageDisplay.SelectionColor = MessageDisplay.ForeColor;
                MessageDisplay.SelectionStart = MessageDisplay.Text.Length;
                MessageDisplay.ScrollToCaret();

            });
        }

        public void ScrollToBottom()
        {
            MessageDisplay.SelectionStart = MessageDisplay.Text.Length;
            MessageDisplay.ScrollToCaret();
        }


    }
}
