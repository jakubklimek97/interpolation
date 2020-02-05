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
///////////////////////////////////////////////////////////
/*
* Autor: Jakub Klimek
* Informatyka
* Semestr: 5
* Grupa dziekanska: 1-2
*
* Temat: Program zmieniający rozdzielczość wielu zdjęć 
*		 do wybranego rozmiaru
*/
///////////////////////////////////////////////////////////
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
        //ilosc watkow do ustawienia ThreadPool
        int threadCount;
        public Skalowanie()
        {
            InitializeComponent();
            int coreCount = 0;
            //dla każdego procesora...
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                //dodaj do liczby rdzeni jego rdzenie
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            //liczba wątków
            threadCount = Environment.ProcessorCount;
            //wyświetlenie liczy rdzeni i wątków w interfejsie
            coreLabel.Text += coreCount.ToString();
            threadLabel.Text += threadCount.ToString();
        }
        
        //funkcje wykorzystywane z biblioteki      
        [DllImport("interpolationLibrary.dll")]
        public static extern unsafe int interpolateC(byte* src, byte* dst, int width, long height, long newWidth, long newHeight);
        [DllImport("interpolationLibrary.dll")]
        public static extern unsafe int interpolateAsm(byte* src, byte* dst, int width, long height, long newWidth, long newHeight);
        
 
        //Funkcja wywoływana dla każdego obrazu. Odpowiada za skalowanie
        public void interpolate(object param)
        {
            //Rzutowanie typu - otrzymanie parametrów funkcji
            Polecenie paramP = param as Polecenie;
            string src = paramP.inputFile;
            string dst = paramP.outputFile;
            //Podział ścieżki źródłowej na segmenty foldery i pliki
            string[] split = src.Split(new[] { "\\" }, StringSplitOptions.None);
            //Wyłuskanie rozszerzenia pliku z ostatniego segmentu ścieżki źródłowej
            string extension = split.Last().Split(new char[] { '.' }).Last().ToLower();
            //Ustawienie formatu pliku wynikowego
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
            //Dodanie do ścieżki docelowej nazwy pliku z rozszerzeniem
            dst = dst + "\\" +  split.Last();

            //Tylko 1 wątek na raz może podjąć próbę zaalokowania pamięci
            memoryAllocation.WaitOne();
            Image srcImage = null;
            Bitmap srcBitmap = null;
            Bitmap dstBitmap = null;
            //Tak długo, jak nie udało się zaalokować pamięci na obraz źródłowy, 
            //jego bitmapę i bitmapę docelową...
            while(srcBitmap == null || dstBitmap == null || srcImage == null)
            {
                if(srcImage == null)
                {
                    try
                    {
                        //Załaduj obraz z dysku do pamięci
                        srcImage = Image.FromFile(src);
                    }
                    catch (OutOfMemoryException)
                    {
                        //Jeżeli się nie udało, przypisz null
                        srcImage = null;
                    }
                }
                if(srcImage != null && srcBitmap == null)
                {
                    try
                    {
                        //Załaduj bitmapę z obrazu źródłowego
                        srcBitmap = new Bitmap(srcImage);
                    }
                    catch(Exception)
                    {
                        //Jeżeli się nie udało, przypisz null
                        srcBitmap = null;
                    }
                }
                if (dstBitmap == null)
                {
                    try
                    {
                        //Utworz bitmapę na obraz docelowy
                        dstBitmap = new Bitmap(dstWidth, dstHeight);
                    }
                    catch (Exception)
                    {
                        //Jeżeli się nie udało, przypisz null
                        dstBitmap = null;
                    }
                }
                //Jeżeli nie udało się zaalokować pamięci na obraz źródłowy i bitmapy
                if(srcBitmap == null || dstBitmap == null || srcImage == null)
                {
                    //"Zabierz" dla siebie semaforę oczekującą na zwolniene pamięci
                    awaitingMemory.WaitOne();
                    //Czekaj, aż inny wątek zwolni semaforę (zrobi to po zwolnieniu używanych zasobów)
                    //Górny limit czasu oczekiwania wynosi 5 minut
                    awaitingMemory.WaitOne(300000);
                    //Zwolnij zabraną semaforę
                    try { awaitingMemory.Release(); }
                    catch (SemaphoreFullException) { }
                }

            }
            //Po udanej alokacji pamięci, zwolnij semaforę
            memoryAllocation.Release();

            //Zablokuj bitmapy do użytku przez niezarządzalny kod
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
                //Wskaźniki na tablice pikseli
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                //Wybór funkcji asm/C
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
            //Odblokowanie bitmap po ich wykorzystaniu przez funkcje interpolacji
            srcBitmap.UnlockBits(srcData);
            dstBitmap.UnlockBits(dstData);
            try
            {
                //Zapis docelowego obrazu do pliku
                dstBitmap.Save(dst, format);
            }
            catch(ExternalException)
            {
                MessageBox.Show("Nie mozna zapisac do pliku\n" + dst);
            }
            //Zwolnienie zasobów
            dstBitmap.Dispose();
            srcBitmap.Dispose();
            srcImage.Dispose();
            dstBitmap = null;
            srcBitmap = null;
            srcImage = null;
            //Tylko 1 wątek na raz może poinformować o zwolnieniu swoich zasobów
            checkingAwaitingMemory.WaitOne();
            //Zwolnij semaforę, informując wątek oczekujący na zasoby o ich zwolnieniu
            awaitingMemory.WaitOne(0);
            awaitingMemory.Release();
            //Zwolnij semaforę - kolejny wątek może informować o zwolnieniu zasobów
            checkingAwaitingMemory.Release();

            this.progressBar1.Invoke(new Action(() => {
                //Zwiększ postęp na pasku postępu
                progressBar1.PerformStep();
                //Jeżeli pasek się wypełnił, to zadanie zostało wykonane. 
                //Wykonaj podsumowanie
                if(progressBar1.Value == progressBar1.Maximum)
                {
                    //Skończ liczyć czas
                    watch.Stop();
                    //Uaktywnij przyciski w GUI
                    convertBtn.Enabled = true;
                    destBtn.Enabled = true;
                    sourceSelectBtn.Enabled = true;
                    //Informacja tekstowa o zakończonej pracy
                    infoLabel.Text = "Skalowanie zakończone!";

                    //Okno informujące o czasie trwania operacji
                    MessageBox.Show("Konwersja trwala :" + watch.ElapsedMilliseconds.ToString() + "ms.");
                }
                }));
        }
        //Funkcja wywoływana po naciśnięciu przycisku "Skaluj"
        private void convertBtn_Click(object sender, EventArgs e)
        {
            //Jeżeli nie wybrano obrazow
            if(selectedImages.Count == 0)
            {
                //Nie rób nic
                return;
            }
            //Ustawienie liczby pracujących wątków na dostępne wątki
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(threadCount, threadCount);
            ThreadPool.SetMinThreads(threadCount, threadCount);
            //Przypisanie do zmiennej wyboru użytkownika dot. funkcji skalującej
            if (asmRadioBtn.Checked == true)
            {
                language = 0;
            }
            else
            {
                language = 1;
            }
            //Zablokowanie możliwości klikania przycisków
            convertBtn.Enabled = false;
            destBtn.Enabled = false;
            sourceSelectBtn.Enabled = false;

            //Utworzenie semafor
            memoryAllocation = new Semaphore(1, 1);
            awaitingMemory = new Semaphore(1, 1);
            checkingAwaitingMemory = new Semaphore(1, 1);
            //Pobranie z kontrolek rozdzielczości docelowej zdjęć
            dstWidth = (int)widthBox.Value;
            dstHeight = (int)heightBox.Value;

            infoLabel.Text = "Sprawdzanie zasobów...";
            //Tymczasowa bitmapa
            Bitmap tempDst;
            try
            {
                //Sprawdzenie, czy można zaalokować wystarczająco pamięci na obrazek docelowy
                tempDst = new Bitmap((int)widthBox.Value, (int)heightBox.Value);
            }
            catch(Exception)
            {
                MessageBox.Show("Nie można zaalokować pamięci na obrazek docelowy.");
                //Jeżeli nie można, nie ma sensu liczyć dalej
                return;
            }
            //Struktura na pliki, które będą obsługiwane, tj. wystarczy pamięci na ich skalowanie
            List<string> imagesToProcess = new List<string>();
            //Ustawienie paska postępu
            progressBar1.Maximum = selectedImages.Count;
            progressBar1.Value = 0;
            new Thread(() =>
            {
                //Dla każdego obrazka
                foreach (string imgPath in selectedImages)
                {
                    try
                    {
                        //Spróbuj załadować do pamięci i utworzyć z niego bitmapę
                        Image srcImg = Image.FromFile(imgPath);
                        Bitmap bmp = new Bitmap(srcImg);
                        //Jeżeli udało się zarezerwować zasoby, dodaj obrazek do listy
                        imagesToProcess.Add(imgPath);
                        //Zwolnij użyte zasoby
                        bmp.Dispose();
                        srcImg.Dispose();
                        progressBar1.Invoke(new Action(() =>
                        {
                            //Aktualizuj postęp
                            progressBar1.PerformStep();
                            //Po przetworzeniu wszystkich zdjęć
                            if (progressBar1.Value == progressBar1.Maximum)
                            {
                                //Przygotuj i uruchom zegar zliczający czas trwania skalowania
                                watch = new System.Diagnostics.Stopwatch();
                                watch.Start();
                                infoLabel.Text = "Sprawdzono pomyślnie. Trwa skalowanie...";
                                //Jeżeli nie wszystkie obrazy uda się przetworzyć, wyświetl komunikat
                                if (imagesToProcess.Count != selectedImages.Count)
                                {
                                    MessageBox.Show("Zasoby sprzetowe pozwolą obsłużyć " + imagesToProcess.Count
                                                    + " z " + selectedImages.Count + " wybranych zdjęć.");
                                }
                                //Ustawienie paska postępu
                                progressBar1.Value = 0;
                                progressBar1.Maximum = imagesToProcess.Count;
                                progressBar1.Step = 1;
                                //Jeżeli katalog źródłowy = katalog docelowy
                                if (createOut)
                                {
                                    createOut = false;
                                    //Dodaj do katalogu docelowego końcówkę "out"
                                    dstDir += "\\out";
                                    try
                                    {
                                        //Spróbuj utworzyć katalog docelowy
                                        System.IO.Directory.CreateDirectory(dstDir);
                                    }
                                    catch (UnauthorizedAccessException)
                                    {
                                        //Jeżeli jest to niemożliwe, wyświetl odpowiedni komunikat i zakończ
                                        MessageBox.Show("Nie mam uprawnien do utworzenia katalogu out...");
                                        destBtn.Invoke(new Action(() => {
                                            destBtn.Enabled = true;
                                            sourceSelectBtn.Enabled = true;
                                            convertBtn.Enabled = true;
                                        }));
                                        return;
                                    }
                                }
                                //Dla każdego obrazu do przetworzenia
                                foreach (string image in imagesToProcess)
                                {
                                    //Przygotuj obiekt zawierający argumenty funkcji
                                    Polecenie param = new Polecenie();
                                    param.inputFile = image;
                                    param.outputFile = dstDir;
                                    //Kolejkuj zdjęcie 
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(interpolate), param);
                                }
                            }
                        }));
                        
                        
                        

                    }
                    catch (Exception) //łapiemy out of memory i przy tworzeniu bitmapy
                    {
                        //nic nie robimy, po prostu na koniec obsluzymy mniej obrazow;
                    }
                }
            }).Start();
           
            

            
        }
        //Funkcja sprawdzająca, czy wybrane foldery źródłowy i docelowy są tym samym folderem
        private void checkDirs()
        {
            createOut = false;
            if(srcDir != null && dstDir!= null && srcDir.Equals(dstDir))
            {
                //Jeżeli ścieżki wskazują na ten sam folder, wypisz stosowny komunikat i ustaw zmienną createOut na true - jej stan pozwoli
                //utworzyć w późniejszym etapie folder docelowy
                MessageBox.Show("Wybrano ten sam folder źródłowy i docelowy.\nZostanie utworzony podkatalog \"out\" na przerobione zdjęcia.");
                createOut = true;
            }
        }
        //Funkcja wywoływana po kliknięciu przycisku wyboru folderu docelowego
        private void destBtn_Click(object sender, EventArgs e)
        {
            //Jeżeli użytkownik wybrał ścieżkę
            if(this.folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                //zachowaj ścieżkę
                dstDir = folderBrowserDialog1.SelectedPath;
                //sprawdź, czy nie pokrywa się z folderem źródłowym
                checkDirs();
                dstSelected = true;
                convertBtn.Enabled = dstSelected && srcSelected;
            }
        }
        //Funkcja wywoływana przy kliknięciu przycku wyboru pliku/folderu źródłowego
        private void selectSource(object sender, EventArgs e)
        {
            //jezeli uzytkownik wybral wybor plikow zrodlowych
            if(filesRadioButton.Checked == true)
            {
                //Jeżeli użytkownik wybrał ścieżkę
                if (this.fileSelectDIalog.ShowDialog(this) == DialogResult.OK)
                {
                    //Zablokowanie akcji GUI
                    sourceSelectBtn.Enabled = false;
                    destBtn.Enabled = false;
                    convertBtn.Enabled = false;
                    infoLabel.Text = "Weryfikowanie plikow...";
                    //Wyczyszczenie listy wybranych obrazów
                    selectedImages.Clear();
                    //Obrazy wybrane w oknie dialogowym
                    string[] files = fileSelectDIalog.FileNames;
                    //Przygotowanie paska postępu
                    progressBar1.Value = 0;
                    progressBar1.Maximum = files.Length;
                    //Pobranie ścieżki do folderu, w którym znajdują się wybrane pliki
                    srcDir = System.IO.Path.GetDirectoryName(files.First());
                    new Thread(() =>
                    {
                        //Dla każdego wybranego pliku
                        foreach (string file in files)
                        {
                            try
                            {
                                //Spróbuj go załadować
                                Image img = Image.FromFile(file);
                                img.Dispose();
                                //Jeżeli się udało, dodaj do listy obrazów do przetworzenia
                                selectedImages.Add(file);
                            }
                            catch (OutOfMemoryException)
                            {
                                //Jezeli to nie obraz, to nie dodawaj do listy
                            }
                            //Aktualizuj pasek postępu
                            progressBar1.Invoke(new Action(() => { progressBar1.PerformStep(); }));
                        }
                        //Poinformuj o zakończeniu sprawdzania i przywróć akcje GUI
                        infoLabel.Invoke(new Action(() => {
                            infoLabel.Text = "Wybrano plików: " + selectedImages.Count;
                            sourceSelectBtn.Enabled = true;
                            destBtn.Enabled = true;
                            srcSelected = true;
                            convertBtn.Enabled = dstSelected && srcSelected;
                        }));
                        //sprawdź, czy folder źródłowy pokrywa się z folderem docelowym
                        checkDirs();
                    }).Start();       
                    
                    
                }
            }
            //jezeli uzytkownik wybral wybor folderu zrodlowego
            else
            {
                //Jeżeli użytkownik wybrał ścieżkę
                if (this.folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    //Zablokowanie akcji GUI
                    sourceSelectBtn.Enabled = false;
                    destBtn.Enabled = false;
                    convertBtn.Enabled = false;
                    infoLabel.Text = "Weryfikowanie plikow...";
                    //Wyczyszczenie listy wybranych obrazów
                    selectedImages.Clear();
                    //Obrazy w folderze wybranym w oknie dialogowym
                    string[] files = System.IO.Directory.EnumerateFiles(folderBrowserDialog1.
                        SelectedPath).Where(file => file.ToLower().EndsWith(".jpg")
                        || file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".bmp"))
                        .ToArray();
                    //Przygotowanie paska postępu
                    progressBar1.Value = 0;
                    progressBar1.Maximum = files.Length;
                    //Pobranie ścieżki do folderu, w którym znajdują się wybrane pliki
                    srcDir = System.IO.Path.GetDirectoryName(files.First());

                    new Thread(() =>
                    {
                        //Dla każdego wybranego pliku
                        foreach (string file in files)
                        {
                            try
                            {
                                //Spróbuj go załadować
                                Image img = Image.FromFile(file);
                                img.Dispose();
                                //Jeżeli się udało, dodaj do listy obrazów do przetworzenia
                                selectedImages.Add(file);
                            }
                            catch (OutOfMemoryException)
                            {
                                //Jezeli to nie obraz, to nie dodawaj do listy
                            }
                            //Aktualizuj pasek postępu
                            progressBar1.Invoke(new Action(() => { progressBar1.PerformStep(); }));
                        }
                        //Poinformuj o zakończeniu sprawdzania i przywróć akcje GUI
                        infoLabel.Invoke(new Action(() => {
                            infoLabel.Text = "Wybrano plików: " + selectedImages.Count;
                            sourceSelectBtn.Enabled = true;
                            destBtn.Enabled = true;
                            srcSelected = true;
                            convertBtn.Enabled = dstSelected && srcSelected;
                        }));
                        //sprawdź, czy folder źródłowy pokrywa się z folderem docelowym
                        checkDirs();
                    }).Start();
                }
            }
        }
    }
    //Klasa, której obiekt służy do przekazania parametrów do funkcji interpolującej
    class Polecenie
    {
        public string inputFile, outputFile;
    }
}
