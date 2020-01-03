using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            int number = zwrocNumber();
            System.Console.WriteLine(number);
        }
        [DllImport("library_empty.dll")]
        public static extern int returnNumber();
        [DllImport("library_empty.dll")]
        public static extern int zwrocNumber();
        [DllImport("library_empty.dll")]
        public static extern unsafe int zwrocPiksel(byte* src, int width);
        [DllImport("library_empty.dll")]
        public static extern unsafe int interpolate(byte* src);

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
