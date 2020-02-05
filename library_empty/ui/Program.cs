using System;
using System.Collections.Generic;
using System.Linq;
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
    static class Program
    {

        [System.Runtime.InteropServices.DllImport("interpolationLibrary.dll")]
        public static extern bool checkForSSE();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool featureSSE4_1 = checkForSSE();
            if (featureSSE4_1 == false)
            {
                MessageBox.Show("Procesor nie obsługuje SSE4.1. Program zakończy pracę.");
            }
            else {
                Application.Run(new Skalowanie());
            }
            
        }
    }
}
