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
    class Polecenie
    {
        public string inputFile, outputFile;
    }

    public partial class Skalowanie : Form
    {
        public Delegate updateProgress;
        List<String> imagesFilePath = new List<String>();
        List<string> selectedImages = new List<string>();
        string srcDir, dstDir;
        static int language = 0; //0 - asm, 1 - C++
        static int dstWidth, dstHeight;
        public Skalowanie()
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

        public void interpolate(object param)
        {
            Polecenie paramP = param as Polecenie;
            string src = paramP.inputFile;
            string dst = paramP.outputFile;
            string[] split = src.Split(new[] { "\\" }, StringSplitOptions.None);
            string extension = split.Last().Split(new char[] { '.' }).Last();
            ImageFormat format;
            switch (extension)
            {
                case "jpg":
                    {
                        format = ImageFormat.Jpeg;
                        break;
                    }
                case "bmp":
                    {
                        format = ImageFormat.Bmp;
                        break;
                    }
                case "png":
                    {
                        format = ImageFormat.Png;
                        break;
                    }
                default:
                    format = ImageFormat.Jpeg;
                    break;
            }

            dst = dst + "\\" +  split.Last();

            Image srcImage = Image.FromFile(src);
            Bitmap srcBitmap = new Bitmap(srcImage);
            Bitmap dstBitmap = new Bitmap(dstWidth, dstHeight);

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
                if(language == 0)
                {
                    interpolateAsm(srcPtr, dstPtr, srcImage.Width,
                    srcImage.Height, dstWidth, dstHeight);
                }
                else
                {
                    interpolateC(srcPtr, dstPtr, srcImage.Width,
                    srcImage.Height, dstWidth, dstHeight);
                }
                
            }
            srcBitmap.UnlockBits(srcData);
            dstBitmap.UnlockBits(dstData);
            dstBitmap.Save(dst, format);
            dstBitmap.Dispose();
            srcBitmap.Dispose();
            srcImage.Dispose();
            this.progressBar1.Invoke(new Action(() => progressBar1.PerformStep()));
        }
        public static void interpolateWithC(object file)
        {
            string fileS = Convert.ToString(file);
            string[] fileOnly = Convert.ToString(file).Split(new[] { "\\" }, StringSplitOptions.None);
            string extension = fileOnly.Last().Split(new char[] { '.' }).Last();
            string returnFile = String.Join("\\", fileOnly, 0, fileOnly.Count() - 1) + "\\out\\" + fileOnly.Last();
            Image srcImage = Image.FromFile(fileS);
            int newWidth = srcImage.Width * 20;
            int newHeight = srcImage.Height * 20;
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
                interpolateC(srcPtr, dstPtr, srcImage.Width,
                    srcImage.Height, newWidth, newHeight);
            }
            srcBitmap.UnlockBits(srcData);
            dstBitmap.UnlockBits(dstData);
            dstBitmap.Save(returnFile, ImageFormat.Jpeg);
            dstBitmap.Dispose();
            srcBitmap.Dispose();
            srcImage.Dispose();
            
        }

        private void convertBtn_Click(object sender, EventArgs e)
        {
            dstWidth = (int)widthBox.Value;
            dstHeight = (int)heightBox.Value;

            infoLabel.Text = "Sprawdzenie zasobów...";
            Bitmap tempDst;
            try
            {
                tempDst = new Bitmap((int)widthBox.Value, (int)heightBox.Value);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Nie można zaalokować pamięci na obrazek docelowy.");
                return;
            }
            List<string> imagesToProcess = new List<string>();
            foreach(string imgPath in selectedImages)
            {
                try
                {
                    Image srcImg = Image.FromFile(imgPath);
                    Bitmap bmp = new Bitmap(srcImg);
                    imagesToProcess.Add(imgPath);
                    bmp.Dispose();
                    srcImg.Dispose();
                }
                catch(Exception ex) //łapiemy out of memory i przy tworzeniu bitmapy
                {
                    //nic nie robimy, po prostu na koniec obsluzymy mniej obrazow;
                }
            }
            if(imagesToProcess.Count != selectedImages.Count)
            {
                MessageBox.Show("Zasoby sprzetowe pozwolą obsłużyć " + imagesToProcess.Count
                                + " z " + selectedImages.Count + " wybranych zdjęć.");
            }

            progressBar1.Value = 0;
            progressBar1.Maximum = imagesToProcess.Count;
            ThreadPool.SetMaxThreads(4, 4);

            foreach(string image in imagesToProcess)
            {
                Polecenie param = new Polecenie();
                param.inputFile = image;
                param.outputFile = dstDir;

                ThreadPool.QueueUserWorkItem(new WaitCallback(interpolate), param);
            }

           /* List<Thread> threadList = new List<Thread>();
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
            }*/
        }
        private void checkDirs()
        {
            if(srcDir != null && dstDir!= null && srcDir.Equals(dstDir))
            {
                MessageBox.Show("Wybrano ten sam folder źródłowy i docelowy.\nZostanie utworzony podkatalog \"out\" na przerobione zdjęcia.");
            }
        }

        private void destBtn_Click(object sender, EventArgs e)
        {
            if(this.folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                dstDir = folderBrowserDialog1.SelectedPath;
                checkDirs();
            }
        }

        private void selectSource(object sender, EventArgs e)
        {
            //jezeli uzytkownik wybral wybor plikow zrodlowych
            if(filesRadioButton.Checked == true)
            {
                if(this.fileSelectDIalog.ShowDialog(this) == DialogResult.OK)
                {
                    infoLabel.Text = "Weryfikowanie plikow...";
                    selectedImages.Clear();
                    string[] files = fileSelectDIalog.FileNames;
                    progressBar1.Value = 0;
                    progressBar1.Maximum = files.Length;
                    srcDir = System.IO.Path.GetDirectoryName(files.First());
                    foreach(string file in files)
                    {
                        try
                        {
                            Image img = Image.FromFile(file);
                            img.Dispose();
                            selectedImages.Add(file);
                            Console.Out.WriteLine(file);
                        }
                        catch(OutOfMemoryException ex)
                        {
                            //Jezeli to nie obraz, to nie dodawaj do listy
                        }
                        progressBar1.PerformStep();
                    }
                    infoLabel.Text = "Wybrano plików: " + selectedImages.Count;
                    checkDirs();
                }
            }
            //jezeli uzytkownik wybral wybor folderu zrodlowego
            else
            {
                if (this.folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    infoLabel.Text = "Weryfikowanie plikow...";
                    selectedImages.Clear();
                    string[] files = System.IO.Directory.EnumerateFiles(folderBrowserDialog1.
                        SelectedPath).Where(file => file.ToLower().EndsWith(".jpg")
                        || file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".bmp"))
                        .ToArray();
                    progressBar1.Value = 0;
                    progressBar1.Maximum = files.Length;
                    srcDir = System.IO.Path.GetDirectoryName(files.First());
                    foreach (string file in files)
                    {
                        try
                        {
                            Image img = Image.FromFile(file);
                            img.Dispose();
                            selectedImages.Add(file);
                            Console.Out.WriteLine(file);
                        }
                        catch (OutOfMemoryException ex)
                        {
                            //Jezeli to nie obraz, to nie dodawaj do listy
                        }
                        progressBar1.PerformStep();
                    }
                    infoLabel.Text = "Wybrano plików: " + selectedImages.Count;
                    checkDirs();
                }
            }
        }
    }
}
