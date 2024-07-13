using System;
using System.Drawing;
using System.Windows.Forms;

namespace WowMove
{
    public class OverlayForm : Form
    {
        private Rectangle bounds;

        public OverlayForm(Rectangle bounds)
        {
            this.bounds = bounds;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.Paint += OverlayForm_Paint;
        }   

        private void OverlayForm_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Red, 2))
            {
                e.Graphics.DrawRectangle(pen, bounds);
            }
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW
                return cp;
            }
        }
    }
}
