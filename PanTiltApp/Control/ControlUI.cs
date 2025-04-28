using System;
using System.Drawing;
using System.Windows.Forms;

namespace PanTiltApp.Operate
{
    public class ControlUI : UserControl
    {
        private Panel joystickBase;
        private Point joystickCenter;
        private Point joystickKnob;
        private bool isDragging = false;
        private const int knobSize = 30; // odsuń kółko od krawędzi

        public event EventHandler<(float x, float y)>? JoystickMoved;
        public event EventHandler<bool>? SwitchToggled;


        public ControlUI()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;

            joystickBase = new BufferedPanel
            {
                Width = 300,
                Height = 300,
                BackColor = ColorTranslator.FromHtml("#06402B"),
                Margin = new Padding(10),
            };


            joystickBase.Paint += JoystickBase_Paint;
            joystickBase.MouseDown += JoystickBase_MouseDown;
            joystickBase.MouseMove += JoystickBase_MouseMove;
            joystickBase.MouseUp += JoystickBase_MouseUp;

            // this.Controls.Add(joystickBase);
            // Layout centrowany
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            // Tworzymy nowy CheckBox
            var sendFramesSwitch = new CheckBox
            {
                Text = "Enable turret control",
                Dock = DockStyle.Top,
                Checked = false, // domyślnie wyłączony
                AutoSize = true,
                Margin = new Padding(10),
            };

            layout.Controls.Add(sendFramesSwitch, 0, 0);
            layout.Controls.Add(joystickBase, 0, 1);
            layout.RowCount = 2;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // dla CheckBox
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // dla Joysticka

            // Dodaj joystickBase do środka komórki
            layout.SetCellPosition(joystickBase, new TableLayoutPanelCellPosition(0, 1));

            // Wyłącz zakotwiczenie joysticka — żeby się nie rozciągał
            joystickBase.Anchor = AnchorStyles.None;
            joystickBase.MinimumSize = new Size(300, 300);
            joystickBase.MaximumSize = new Size(300, 300);
            joystickBase.Size = new Size(300, 300); // kontrolnie



            // Dodaj layout do kontrolki
            this.Controls.Add(layout);


            this.Layout += (s, e) =>
            {
                joystickCenter = new Point(joystickBase.Width / 2, joystickBase.Height / 2);
                joystickKnob = joystickCenter;
                joystickBase.Invalidate(); // odśwież
            };

            sendFramesSwitch.CheckedChanged += (sender, args) =>
            {
                SwitchToggled?.Invoke(this, sendFramesSwitch.Checked);
            };
        }

        private void JoystickBase_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Rysuj bazę (kółko bazowe)
            // g.FillEllipse(Brushes.Black, 0, 0, joystickBase.Width, joystickBase.Height);
            int size = Math.Min(joystickBase.Width, joystickBase.Height);
            g.FillEllipse(Brushes.DimGray,
                (joystickBase.Width - size) / 2,
                (joystickBase.Height - size) / 2,
                size,
                size);            

            // Rysuj uchwyt joysticka (wewnętrzne kółko)
            g.FillEllipse(Brushes.DarkGray,
                joystickKnob.X - knobSize / 2,
                joystickKnob.Y - knobSize / 2,
                knobSize,
                knobSize);
        }

        private void JoystickBase_MouseDown(object? sender, MouseEventArgs e)
        {
            isDragging = true;
        }

        private void JoystickBase_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!isDragging) return;

            var dx = e.X - joystickCenter.X;
            var dy = e.Y - joystickCenter.Y;

            double distance = Math.Sqrt(dx * dx + dy * dy);
            // double maxRadius = joystickBase.Width / 2;
            double maxRadius = (joystickBase.Width - knobSize) / 2.0;


            if (distance > maxRadius)
            {
                dx = (int)(dx / distance * maxRadius);
                dy = (int)(dy / distance * maxRadius);
            }

            joystickKnob = new Point(joystickCenter.X + dx, joystickCenter.Y + dy);
            joystickBase.Invalidate();

            float normX = (float)(dx / maxRadius);
            float normY = (float)(dy / maxRadius);

            JoystickMoved?.Invoke(this, (normX, normY));
        }

        private void JoystickBase_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
            joystickKnob = joystickCenter;
            joystickBase.Invalidate();

            JoystickMoved?.Invoke(this, (0f, 0f)); // reset pozycji
        }
    }
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED - pozwala na rysowanie poza kontrolką
                return cp;
            }
        }
    }
}
