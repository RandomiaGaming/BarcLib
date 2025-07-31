using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
public static class BitmapRenderrer
{
    public static void Show(this Bitmap bitmap)
    {
        BitmapDisplayForm bitmapDisplayForm = new BitmapDisplayForm(bitmap);

        bitmapDisplayForm.ShowDialog();
    }
    private class BitmapDisplayForm : Form
    {
        private Bitmap _bitmap;
        public BitmapDisplayForm(Bitmap bitmap)
        {
            _bitmap = bitmap;
            ClientSize = new Size(bitmap.Width, bitmap.Height);
            ResizeRedraw = true;
            DoubleBuffered = true;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.Clear(Color.FromArgb(255, 0, 0));
            e.Graphics.DrawImage(_bitmap, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height), new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), GraphicsUnit.Pixel);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}