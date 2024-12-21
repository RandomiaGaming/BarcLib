using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace BarcLib
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Code128BarcodeGenerator generator = new Code128BarcodeGenerator();
            generator.Append("");
            Bitmap barcode = generator.Generate(10 * 2 * 2, 1 * 2);
            Bitmap help = new Bitmap(barcode.Width + 100, barcode.Height + 100);
            Graphics sob = Graphics.FromImage(help);
            sob.Clear(Color.White);
            sob.DrawImageUnscaled(barcode, new Point(50, 50));
            sob.Dispose();
            help.Show();
        }
    }
}