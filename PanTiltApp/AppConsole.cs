using System;
using System.Drawing;
using System.Windows.Forms;

namespace PanTiltApp
{
    public class AppConsole : UserControl
    {
        private RichTextBox messageDisplay;

        public AppConsole()
        {
            InitializeConsoleUI();
        }

        private void InitializeConsoleUI()
        {
            this.Dock = DockStyle.Bottom;
            this.Height = 150;
            this.BackColor = Color.Black;

            messageDisplay = new RichTextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BackColor = Color.Black,
                ForeColor = ColorTranslator.FromHtml("#F8F9FA"),
                Font = new Font("Courier", 10, FontStyle.Regular),
                WordWrap = true
            };

            this.Controls.Add(messageDisplay);
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
    }
}
