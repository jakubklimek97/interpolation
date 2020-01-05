using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ui
{
    public partial class Form1 : Form
    {
        List<String> imagesFilePath = new List<String>();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Image srcImage = Image.FromFile("test.jpg");
            int newWidth = srcImage.Width/2;
            int newHeight = srcImage.Height/2;
            Bitmap srcBitmap = new Bitmap(srcImage);
            Bitmap dstBitmap = new Bitmap(newWidth, newHeight);
            BitmapData srcData = srcBitmap.LockBits(
                new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format32bppArgb);
            
            BitmapData dstData = dstBitmap.LockBits(
                new Rectangle(0, 0, dstBitmap.Width, dstBitmap.Height), 
                ImageLockMode.WriteOnly, 
                PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                interpolateAsm(srcPtr, dstPtr, srcImage.Width,
                    srcImage.Height, newWidth, newHeight);
            }
            srcBitmap.UnlockBits(srcData);
            dstBitmap.UnlockBits(dstData);
            dstBitmap.Save("out.png", ImageFormat.Png);
            /*
            Image img = Image.FromFile("test.png");
            int newWidth = 272;
            int newHeight = 240;
            int width = img.Width;
            int height = img.Height;

            Bitmap dest = new Bitmap(img);
            //Bitmap dest = new Bitmap(2, 2,PixelFormat.Format32bppArgb);

            unsafe
            {
                BitmapData destPixels = dest.LockBits(new Rectangle(0, 0, dest.Width, dest.Height), ImageLockMode.WriteOnly, dest.PixelFormat);
                byte* ptr = (byte*)destPixels.Scan0;
                interpolate(ptr);
                dest.UnlockBits(destPixels);
                Color piksel = dest.GetPixel(0, 0);
                dest.Save("test2.jpg", ImageFormat.Jpeg);

                Bitmap jpg = new Bitmap(img);
                Graphics tmp = Graphics.FromImage(jpg);
                tmp.DrawImage(dest, new Rectangle(0, 0, 2, 2));
                piksel = jpg.GetPixel(0, 0);
                
                
               img.Save("test.jpg", ImageFormat.Jpeg);
                

            }*/
        }
        [DllImport("library_empty.dll")]
        public static extern int returnNumber();
        [DllImport("library_empty.dll")]
        public static extern int zwrocNumber();
        [DllImport("library_empty.dll")]
        public static extern unsafe int zwrocPiksel(byte* src, int width);
        [DllImport("library_empty.dll")]
        public static extern unsafe int interpolate(byte* src);
        [DllImport("library_empty.dll")]
        public static extern unsafe int interpolateC(byte* src, byte* dst, int width, long height, long newWidth, long newHeight);
        [DllImport("library_empty.dll")]
        public static extern unsafe int interpolateAsm(byte* src, byte* dst, int width, long height, long newWidth, long newHeight);

        private void button2_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(folderBrowserDialog1.SelectedPath);
                imagesFilePath.Clear();
                String[] files = System.IO.Directory.GetFiles(folderBrowserDialog1.SelectedPath);
                foreach(var file in files)
                {
                    try
                    {
                        Bitmap source = new Bitmap(Image.FromFile(file));
                        imagesFilePath.Add(file);
                        int width = source.Width;
                        int height = source.Height;
                        System.Drawing.Imaging.BitmapData pixels = source.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, source.PixelFormat);
                        unsafe
                        {
                            byte* ptr = (byte*)pixels.Scan0;
                            int wartosc = interpolate(ptr);
                            wartosc = 50;
                        }                        
                    }
                    catch(OutOfMemoryException exp)
                    {

                    }
                }
                folderLabel.Text = "Znaleziono obrazów: " + imagesFilePath.Count;
            }
        }
    }
}
