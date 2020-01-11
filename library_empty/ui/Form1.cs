using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
            int newWidth = srcImage.Width * 16;
            int newHeight = srcImage.Height * 16;
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
            dstBitmap.Dispose();
            srcBitmap.Dispose();
            srcImage.Dispose();
        }
        [DllImport("library_empty.dll")]
        public static extern unsafe int interpolateC(byte* src, byte* dst, int width, long height, long newWidth, long newHeight);
        [DllImport("library_empty.dll")]
        public static extern unsafe int interpolateAsm(byte* src, byte* dst, int width, long height, long newWidth, long newHeight);
        public static void interpolateWithC(object file)
        {
            string fileS = Convert.ToString(file);
            string[] fileOnly = Convert.ToString(file).Split(new[] { "\\" }, StringSplitOptions.None);
            string extension = fileOnly.Last().Split(new char[] { '.' }).Last();
            string returnFile = String.Join("\\", fileOnly, 0, fileOnly.Count() - 1) + "\\out\\" + fileOnly.Last();
            Image srcImage = Image.FromFile(fileS);
            int newWidth = srcImage.Width * 16;
            int newHeight = srcImage.Height * 8;
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
            dstBitmap.Save(returnFile, ImageFormat.Jpeg);
            dstBitmap.Dispose();
            srcBitmap.Dispose();
            srcImage.Dispose();
        }
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
                    }
                    catch(OutOfMemoryException exp)
                    {

                    }
                }
                folderLabel.Text = "Znaleziono obrazów: " + imagesFilePath.Count;
            }
        }

        private void convertBtn_Click(object sender, EventArgs e)
        {
            List<Thread> threadList = new List<Thread>();
            System.IO.Directory.CreateDirectory(folderBrowserDialog1.SelectedPath + "\\out");
            foreach(var file in imagesFilePath)
            {
                Thread oneImage = new Thread(new ParameterizedThreadStart(interpolateWithC));
                oneImage.Start(file);
                threadList.Add(oneImage);
            }
            foreach(var thread in threadList)
            {
                thread.Join();
            }
        }
    }
}
