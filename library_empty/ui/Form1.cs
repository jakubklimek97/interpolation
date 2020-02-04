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
     public partial class Skalowanie : Form
    {
        //delegat do aktualizacji paska postępu
        public Delegate updateProgress;
        //lista ścieżek do plików
        List<String> imagesFilePath = new List<String>();
        //lista plikow bedacych obrazami
        List<string> selectedImages = new List<string>();
        //sciezka katalogu zrodlowego i docelowego
        string srcDir, dstDir;
        //czy wybrano katalog zrodlowy i docelowy
        bool srcSelected = false, dstSelected = false;
        //czy trzeba tworzyc katalog out(kat. wejsciowy i wyjsciowy taki sam)
        bool createOut = false;
        //miernik czasu
        System.Diagnostics.Stopwatch watch;
        //0 - asm, 1 - C++
        static int language = 0;
        //szerokosc i wysokosc docelowego obrazu
        static int dstWidth, dstHeight;
        //semafory uzywane przy alokacji pamieci na obraz i podczas czekania na zwolnienie zasobow
        private static Semaphore memoryAllocation, awaitingMemory, checkingAwaitingMemory;
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

        //funkcje wykorzystywane z biblioteki
        
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
            string extension = split.Last().Split(new char[] { '.' }).Last().ToLower();
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

            
            memoryAllocation.WaitOne();
            Image srcImage = null;
            Bitmap srcBitmap = null;
            Bitmap dstBitmap = null;
            while(srcBitmap == null || dstBitmap == null || srcImage == null)
            {
                if(srcImage == null)
                {
                    try
                    {
                        srcImage = Image.FromFile(src);
                    }
                    catch (OutOfMemoryException ex)
                    {
                        srcImage = null;
                    }
                }
                if(srcBitmap == null)
                {
                    try
                    {
                        srcBitmap = new Bitmap(srcImage);
                    }
                    catch(Exception ex)
                    {
                        srcBitmap = null;
                    }
                }
                if (dstBitmap == null)
                {
                    try
                    {
                        dstBitmap = new Bitmap(dstWidth, dstHeight);
                    }
                    catch (Exception ex)
                    {
                        dstBitmap = null;
                    }
                }
                if(srcBitmap == null || dstBitmap == null || srcImage == null)
                {
                    awaitingMemory.WaitOne();
                    awaitingMemory.WaitOne(300000);
                    awaitingMemory.Release();
                }

            }
            memoryAllocation.Release();

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
            try
            {
                dstBitmap.Save(dst, format);
            }
            catch(ExternalException ex)
            {
                MessageBox.Show("Nie mozna zapisac do pliku\n" + dst);
            }
            
            dstBitmap.Dispose();
            srcBitmap.Dispose();
            srcImage.Dispose();
            checkingAwaitingMemory.WaitOne();
            awaitingMemory.WaitOne(0);
            awaitingMemory.Release();
            checkingAwaitingMemory.Release();
            this.progressBar1.Invoke(new Action(() => {
                progressBar1.PerformStep();
                if(progressBar1.Value == progressBar1.Maximum)
                {
                    watch.Stop();
                    convertBtn.Enabled = true;
                    destBtn.Enabled = true;
                    sourceSelectBtn.Enabled = true;

                    infoLabel.Text = "Skalowanie zakończone!";
                    MessageBox.Show("Konwersja trwala :" + watch.ElapsedMilliseconds.ToString() + "ms.");
                }
                }));
        }
        private void convertBtn_Click(object sender, EventArgs e)
        {

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(4, 4);
            if(asmRadioBtn.Checked == true)
            {
                language = 0;
            }
            else
            {
                language = 1;
            }

            convertBtn.Enabled = false;
            destBtn.Enabled = false;
            sourceSelectBtn.Enabled = false;

            memoryAllocation = new Semaphore(1, 1);
            awaitingMemory = new Semaphore(1, 1);
            checkingAwaitingMemory = new Semaphore(1, 1);
            dstWidth = (int)widthBox.Value;
            dstHeight = (int)heightBox.Value;

            infoLabel.Text = "Sprawdzanie zasobów...";
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
            progressBar1.Maximum = selectedImages.Count;
            progressBar1.Value = 0;
            new Thread(() =>
            {
                foreach (string imgPath in selectedImages)
                {
                    try
                    {
                        Image srcImg = Image.FromFile(imgPath);
                        Bitmap bmp = new Bitmap(srcImg);
                        imagesToProcess.Add(imgPath);
                        bmp.Dispose();
                        srcImg.Dispose();
                        progressBar1.Invoke(new Action(() =>
                        {
                            progressBar1.PerformStep();
                            if (progressBar1.Value == progressBar1.Maximum)
                            {
                                watch = new System.Diagnostics.Stopwatch();
                                watch.Start();
                                infoLabel.Text = "Sprawdzono pomyślnie. Trwa skalowanie...";
                                if (imagesToProcess.Count != selectedImages.Count)
                                {
                                    MessageBox.Show("Zasoby sprzetowe pozwolą obsłużyć " + imagesToProcess.Count
                                                    + " z " + selectedImages.Count + " wybranych zdjęć.");
                                }
                                progressBar1.Value = 0;
                                progressBar1.Maximum = imagesToProcess.Count;
                                progressBar1.Step = 1;
                                if (createOut)
                                {
                                    createOut = false;
                                    dstDir += "\\out";
                                    try
                                    {
                                        System.IO.Directory.CreateDirectory(dstDir);
                                    }
                                    catch (UnauthorizedAccessException ex)
                                    {
                                        MessageBox.Show("Nie mam uprawnien do utworzenia katalogu out...");
                                        destBtn.Invoke(new Action(() => {
                                            destBtn.Enabled = true;
                                            sourceSelectBtn.Enabled = true;
                                            convertBtn.Enabled = true;
                                        }));
                                        return;
                                    }
                                }
                                foreach (string image in imagesToProcess)
                                {
                                    Polecenie param = new Polecenie();
                                    param.inputFile = image;
                                    param.outputFile = dstDir;

                                    ThreadPool.QueueUserWorkItem(new WaitCallback(interpolate), param);
                                }
                            }
                        }));
                        
                        
                        

                    }
                    catch (Exception ex) //łapiemy out of memory i przy tworzeniu bitmapy
                    {
                        //nic nie robimy, po prostu na koniec obsluzymy mniej obrazow;
                    }
                }
            }).Start();
           
            

            
        }
        private void checkDirs()
        {
            createOut = false;
            if(srcDir != null && dstDir!= null && srcDir.Equals(dstDir))
            {
                MessageBox.Show("Wybrano ten sam folder źródłowy i docelowy.\nZostanie utworzony podkatalog \"out\" na przerobione zdjęcia.");
                createOut = true;
            }
        }

        private void destBtn_Click(object sender, EventArgs e)
        {
            if(this.folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                dstDir = folderBrowserDialog1.SelectedPath;
                checkDirs();
                dstSelected = true;
                convertBtn.Enabled = dstSelected && srcSelected;
            }
        }

        private void selectSource(object sender, EventArgs e)
        {
            //jezeli uzytkownik wybral wybor plikow zrodlowych
            if(filesRadioButton.Checked == true)
            {
                if(this.fileSelectDIalog.ShowDialog(this) == DialogResult.OK)
                {

                    sourceSelectBtn.Enabled = false;
                    destBtn.Enabled = false;
                    convertBtn.Enabled = false;
                    infoLabel.Text = "Weryfikowanie plikow...";
                    selectedImages.Clear();
                    string[] files = fileSelectDIalog.FileNames;
                    progressBar1.Value = 0;
                    progressBar1.Maximum = files.Length;
                    srcDir = System.IO.Path.GetDirectoryName(files.First());
                    new Thread(() =>
                    {
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
                        progressBar1.Invoke(new Action(() => { progressBar1.PerformStep(); }));
                        }
                        infoLabel.Invoke(new Action(() => {
                            infoLabel.Text = "Wybrano plików: " + selectedImages.Count;
                            sourceSelectBtn.Enabled = true;
                            destBtn.Enabled = true;
                            srcSelected = true;
                            convertBtn.Enabled = dstSelected && srcSelected;
                        }));
                        checkDirs();
                    }).Start();       
                    
                    
                }
            }
            //jezeli uzytkownik wybral wybor folderu zrodlowego
            else
            {
                if (this.folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    sourceSelectBtn.Enabled = false;
                    destBtn.Enabled = false;
                    convertBtn.Enabled = false;
                    infoLabel.Text = "Weryfikowanie plikow...";
                    selectedImages.Clear();
                    string[] files = System.IO.Directory.EnumerateFiles(folderBrowserDialog1.
                        SelectedPath).Where(file => file.ToLower().EndsWith(".jpg")
                        || file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".bmp"))
                        .ToArray();
                    progressBar1.Value = 0;
                    progressBar1.Maximum = files.Length;
                    srcDir = System.IO.Path.GetDirectoryName(files.First());

                    new Thread(() =>
                    {
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
                            progressBar1.Invoke(new Action(() => { progressBar1.PerformStep(); }));
                        }
                        infoLabel.Invoke(new Action(() => {
                            infoLabel.Text = "Wybrano plików: " + selectedImages.Count;
                            sourceSelectBtn.Enabled = true;
                            destBtn.Enabled = true;
                            srcSelected = true;
                            convertBtn.Enabled = dstSelected && srcSelected;
                        }));
                        checkDirs();
                    }).Start();
                }
            }
        }
    }
    class Polecenie
    {
        public string inputFile, outputFile;
    }
}
