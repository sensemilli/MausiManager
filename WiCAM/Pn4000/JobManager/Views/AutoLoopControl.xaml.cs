using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using WiCAM.Pn4000.Common;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Windows.Media;

//using System.Runtime.InteropServices.WindowsRuntime;
using File = System.IO.File;
using WiCAM.Pn4000.TechnoTable;


namespace WiCAM.Pn4000.JobManager
{
    public partial class AutoLoopControl : UserControl, IView, IComponentConnector
    {
        public static AutoLoopControl _AutoLoopControl;
        private bool traced;
        public string pathToPNdrive;
        public string pathToFPR;
        public string pathToFPR996 = "\\u\\pn\\pfiles\\00\\FPR996";
        public string pathToFPR997 = "\\u\\pn\\pfiles\\00\\FPR997";
        public string pathToFPR998 = "\\u\\pn\\pfiles\\00\\FPR998";
        public string pathToFPR999 = "\\u\\pn\\pfiles\\00\\FPR999";

        public AutoLoopControl()
        {
            this.InitializeComponent();
            _AutoLoopControl = this;
            pathToPNdrive = PnPathBuilder.PnDrive;
            pathToFPR = pathToPNdrive + pathToFPR998;
            CheckToolBar();
            ReadFPRtoEditBox();
            editor.Foreground = Brushes.White;

        }

        public void ReadFPRtoEditBox()
        {
            int _index = 0;
            StringBuilder sb = new StringBuilder();
            // textBoxFPR.Text = File.ReadAllText("P:\\u\\pn\\pfiles\\00\\FPR998");
             using (var streamReader = File.OpenText((pathToFPR)))
             {
                /*    
                var lines = streamReader.ReadToEnd().Split("\n".ToCharArray());
              foreach (var line in lines)
              {
                sb.AppendLine(line);
            //    Console.WriteLine(File.ReadAllText("P:\\u\\pn\\pfiles\\00\\FPR998"));
              _index++;
             }
                */
                sb.AppendLine(streamReader.ReadToEnd());

                editor.Text = sb.ToString();
                //   textBoxFPR.Text = sb.ToString();
            }          
        }

        public int FindLineNumberFPR(string fileName, string trackText, string oldText, string newText)
        {
            int lineNumber = 0;
            //fileName = "P:" + fileName;
            string[] textLine = System.IO.File.ReadAllLines(fileName);
            for (int i = 0; i < textLine.Length; i++)
            {
                if (textLine[i].Contains(trackText)) //start finding matching text after.
                    traced = true;
                if (traced)
                    if (textLine[i].Contains(oldText)) // Match text
                    {
                        textLine[i] = newText; // replace text with new one.
                        traced = false;
                        System.IO.File.WriteAllLines(fileName, textLine);
                        lineNumber = i;
                        break; //go out from loop
                    }
            }
            ReadFPRtoEditBox();
            return lineNumber;
        }

        //****************************************************************
        // RichEditBox Open/Save File Button Click Events
        //****************************************************************
        private async void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            //// Open a text file.
            //Windows.Storage.Pickers.FileOpenPicker open =
            //    new Windows.Storage.Pickers.FileOpenPicker();
            //open.SuggestedStartLocation =
            //    Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            //open.FileTypeFilter.Add(".rtf");
            //open.FileTypeFilter.Add(".txt");

            //Windows.Storage.StorageFile file = await open.PickSingleFileAsync();
            //if (file != null)
            //{
            //    try
            //    {
            //        Windows.Storage.Streams.IRandomAccessStream randAccStream =
            //    await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            //        //// Load the file into the Document property of the RichEditBox.
            //        if (file.FileType == ".rtf")
            //            editor.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);
            //        else if(file.FileType == ".txt")
            //            editor.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.None, randAccStream);
            //    }
            //    catch (Exception)
            //    {
            //        ContentDialog errorDialog = new ContentDialog()
            //        {
            //            Title = "File open error",
            //            Content = "Sorry, I couldn't open the file.",
            //            PrimaryButtonText = "Ok"
            //        };

            //        await errorDialog.ShowAsync();
            //    }
            //}
            /*
            // Open a text file.
            Windows.Storage.Pickers.FileOpenPicker open =
                new Windows.Storage.Pickers.FileOpenPicker();
            open.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".rtf");
            open.FileTypeFilter.Add(".txt");
            open.FileTypeFilter.Add(".docx");

            Windows.Storage.StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    var reader = DataReader.FromBuffer(buffer);
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    string text = reader.ReadString(buffer.Length);
                    //// Load the file into the Document property of the RichEditBox.
                    editor.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, text);
                }
                catch (Exception)
                {
                    ContentDialog errorDialog = new ContentDialog()
                    {
                        Title = "File open error",
                        Content = "Sorry, I couldn't open the file.",
                        PrimaryButtonText = "Ok"
                    };
                    await errorDialog.ShowAsync();
                }
            }
            */
        }

        public void SaveFPRedit() 
        {
            Console.WriteLine("saveEditor");
            Console.WriteLine(editor.Text);
            File.WriteAllText(pathToFPR, editor.Text);
            ReadFPRtoEditBox();
        }



        public void CheckToolBar()
        {
            Console.WriteLine("CheckToolBar");
            int iLage = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       20     0     0 AusrichtenLage");
            if (iLage == 0)
                MainWindow.mainWindow.xLage.IsChecked = true;
            else MainWindow.mainWindow.xLage.IsChecked = false;
            Console.WriteLine("Lage =   " + MainWindow.mainWindow.xLage.IsChecked);

            int iXrichten = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       10     0     0 xRichten");
            if (iXrichten == 0)
                MainWindow.mainWindow.xXrichten.IsChecked = true;
            else MainWindow.mainWindow.xXrichten.IsChecked = false;
            Console.WriteLine("Xrichten =   " + MainWindow.mainWindow.xXrichten.IsChecked);

            int iYrichten = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       10     0     0 yRichten");
            if (iYrichten == 0)
                MainWindow.mainWindow.xYrichten.IsChecked = true;
            else MainWindow.mainWindow.xYrichten.IsChecked = false;
            Console.WriteLine("Yrichten =   " + MainWindow.mainWindow.xYrichten.IsChecked);

            int iTrimmen = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       10     0     0 Trimmen");
            if (iTrimmen == 0)
                MainWindow.mainWindow.xTrimmen.IsChecked = true;
            else MainWindow.mainWindow.xTrimmen.IsChecked = false;
            Console.WriteLine("Trimmen =   " + MainWindow.mainWindow.xTrimmen.IsChecked);

            int iGleicheD = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       10     0     0 GleicheD");
            if (iGleicheD == 0)
                MainWindow.mainWindow.xGleicheD.IsChecked = true;
            else MainWindow.mainWindow.xGleicheD.IsChecked = false;
            Console.WriteLine("GleicheD =   " + MainWindow.mainWindow.xGleicheD.IsChecked);

            int iCADdelete = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       10     0     0 CADdelete");
            if (iCADdelete == 0)
                MainWindow.mainWindow.xCADdelete.IsChecked = true;
            else MainWindow.mainWindow.xCADdelete.IsChecked = false;
            Console.WriteLine("CADdelete =   " + MainWindow.mainWindow.xCADdelete.IsChecked);

            int iGravur1 = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       20     0     0 Gravur1");
            if (iGravur1 == 0)
                MainWindow.mainWindow.xGravur.IsChecked = true;
            else MainWindow.mainWindow.xGravur.IsChecked = false;
            Console.WriteLine("Gravur =   " + MainWindow.mainWindow.xGravur.IsChecked);

            int iStanzenInnen = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       20     0     0 STANZEN-Innen");
            if (iStanzenInnen == 0)
                MainWindow.mainWindow.xStanzenInnen.IsChecked = true;
            else MainWindow.mainWindow.xStanzenInnen.IsChecked = false;
            Console.WriteLine("STANZEN-Innen =   " + MainWindow.mainWindow.xStanzenInnen.IsChecked);

            int iKonturaussen = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       20     0     0 Konturaussen");
            if (iKonturaussen == 0)
                MainWindow.mainWindow.xKonturAussen.IsChecked = true;
            else MainWindow.mainWindow.xKonturAussen.IsChecked = false;
            Console.WriteLine("Konturaussen =   " + MainWindow.mainWindow.xKonturAussen.IsChecked);

            int iFlaecheAussen = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       20     0     0 FlaecheAussen");
            if (iFlaecheAussen == 0)
                MainWindow.mainWindow.xFlaecheAussen.IsChecked = true;
            else MainWindow.mainWindow.xFlaecheAussen.IsChecked = false;
            Console.WriteLine("FlaecheAussen =   " + MainWindow.mainWindow.xFlaecheAussen.IsChecked);

            int iStanzenAuto = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       20     0     0 STANZEN-Auto");
            if (iStanzenAuto == 0)
                MainWindow.mainWindow.xStanzenAuto.IsChecked = true;
            else MainWindow.mainWindow.xStanzenAuto.IsChecked = false;
            Console.WriteLine("StanzenAuto =   " + MainWindow.mainWindow.xStanzenAuto.IsChecked);

            int iFlStanzen = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       20     0     0 FlStanzen");
            if (iFlStanzen == 0)
                MainWindow.mainWindow.xFlaecheStanzen.IsChecked = true;
            else MainWindow.mainWindow.xFlaecheStanzen.IsChecked = false;
            Console.WriteLine("FlStanzen =   " + MainWindow.mainWindow.xFlaecheStanzen.IsChecked);

            int iChangeWKZ = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       20     0     0 ChangeWKZ");
            if (iChangeWKZ == 0)
                MainWindow.mainWindow.xChangeWKZ.IsChecked = true;
            else MainWindow.mainWindow.xChangeWKZ.IsChecked = false;
            Console.WriteLine("ChangeWKZ =   " + MainWindow.mainWindow.xChangeWKZ.IsChecked);

            int iSortManual = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       20     0     0 SortManual");
            if (iSortManual == 0)
                MainWindow.mainWindow.xSortieren.IsChecked = true;
            else MainWindow.mainWindow.xSortieren.IsChecked = false;
            Console.WriteLine("SortManual =   " + MainWindow.mainWindow.xSortieren.IsChecked);

            int iNCLoeschen = CheckLineNumberToolBar(pathToFPR, " 1  0  0   0        .0000       20     0     0 Loeschen");
            if (iNCLoeschen == 0)
                MainWindow.mainWindow.xNCLoeschen.IsChecked = true;
            else MainWindow.mainWindow.xNCLoeschen.IsChecked = false;
            Console.WriteLine("NCLoeschen =   " + MainWindow.mainWindow.xNCLoeschen.IsChecked);
        }

        private int CheckLineNumberToolBar(string fileName, string trackText)
        {
            int lineNumber = 0;
          
            Console.WriteLine(fileName);

            string[] textLine = System.IO.File.ReadAllLines(fileName);
            for (int i = 0; i < textLine.Length; i++)
            {
                if (textLine[i].Contains(trackText)) //start finding matching text after.
                    traced = true;
                if (traced)
                    if (textLine[i].Contains(trackText)) // Match text
                    {
                        //textLine[i] = newText; // replace text with new one.
                        traced = false;
                        //System.IO.File.WriteAllLines(fileName, textLine);
                        lineNumber = 1;

                        break; //go out from loop
                    }
            }
            return lineNumber;
        }


        [SpecialName]
        object IView.DataContext()
        {
            return this.DataContext;
        }

        [SpecialName]
        void IView.DataContext(object value)
        {
            Console.WriteLine(value);
            this.DataContext = value;
        }
    }
}
