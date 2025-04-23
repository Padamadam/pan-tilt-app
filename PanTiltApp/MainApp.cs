using System;
using System.Drawing;
using System.Windows.Forms;
using PanTiltApp.Network;
using PanTiltApp.AppConsole;

namespace PanTiltApp
{
    public class MainApp : Form
    {
        private TabControl tabControl;
        private BasicControlsTab basicmainPanel;
        private AdvancedControlsTab advancedmainPanel;
        public MainApp()
        {
            InitializeUI();
            this.KeyPreview = true;
            this.KeyPress += (sender, e) =>
            {
                var wifiUI = basicmainPanel.mainPanel?.WiFiUI;
                var inputBox = basicmainPanel.consoleLogic?.UI.InputBox;
                
                if (!char.IsControl(e.KeyChar) &&
                    !(wifiUI?.IpAddressField.Focused ?? false) &&
                    !(wifiUI?.PortNumberField.Focused ?? false))
                {
                    bool inputFocused = inputBox?.Focused ?? false;

                    basicmainPanel.consoleLogic?.HandleGlobalKeyPress(e.KeyChar);
                    if (!inputFocused)
                    {
                        // jeśli InputBox NIE był sfokusowany – dodaliśmy znak ręcznie → zablokuj beep
                        e.Handled = true;
                    }
                }
            };
        }

        private void InitializeUI()
        {
            this.Text = "Pan-Tilt Connection Manager";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = ColorTranslator.FromHtml("#06402B"); // Green PCB-like background
            
            this.Icon = new Icon("Assets/ikona.ico");

            // Initialize TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Courier New", 10, FontStyle.Regular),
                TabIndex = 0
            };

            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += TabControl_DrawItem;

            basicmainPanel = new BasicControlsTab();
            advancedmainPanel = new AdvancedControlsTab();

            tabControl.TabPages.Add(basicmainPanel);
            tabControl.TabPages.Add(advancedmainPanel);

            this.Controls.Add(tabControl);
        }

        private void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var tabControl = sender as TabControl;
            var tabPage = tabControl?.TabPages[e.Index];

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backgroundColor = isSelected
                ? ColorTranslator.FromHtml("#003300") // aktywna zakładka
                : ColorTranslator.FromHtml("#006600"); // nieaktywna zakładka

            using (var brush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            TextRenderer.DrawText(
                e.Graphics,
                tabPage?.Text,
                tabControl?.Font ?? SystemFonts.DefaultFont,
                e.Bounds,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }
    }
}
