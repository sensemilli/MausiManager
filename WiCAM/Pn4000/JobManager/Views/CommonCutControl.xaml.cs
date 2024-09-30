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
using System.Windows.Controls.Ribbon;


namespace WiCAM.Pn4000.JobManager
{
    public partial class CommonCutControl : UserControl, IView, IComponentConnector
    {
        public static CommonCutControl _CommonCutControl;
        private bool traced;
        private string defaultCCPop = "POPT07_0001";
        public static string pathToCCsettings = "C:\\pnusers\\default\\puserpop";
        private string pathTodefaultUserCCsettings = "C:\\pnusers\\default\\pn.pop";
        private bool sortX = false;


        public string pathToPNdrive;
        public string pathToFPR;
        private string pathToUserFolder;
        public string pathToFPR998 = "\\u\\pn\\pfiles\\00\\FPR998";
        public string pathToFPR999 = "\\u\\pn\\pfiles\\00\\FPR999";
        private string pnMPLfolder;
        private string trimToolSelected;

        public CommonCutControl()
        {
            this.InitializeComponent();
            _CommonCutControl = this;
            pathToPNdrive = PnPathBuilder.PnDrive;
            pathToFPR = pathToPNdrive + pathToFPR998;
            pathToUserFolder = PnPathBuilder.PnHome;

        //    CheckToolBar();
            // Initialize MenuFlyouts on startup with the appropriate MenuFlyout object

        }



        public void ClickMultiShearButton()
        {
            File.Copy(Path.Combine(pathToCCsettings, "POPT07_yMultishear"), Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), true);
            MainWindow.Instance.xStandart76.IsChecked = false;
            MainWindow.Instance.xStandart56.IsChecked = false;
            MainWindow.Instance.xStandart46.IsChecked = false;
            MainWindow.Instance.xStandart30.IsChecked = false;
            if (sortX)
            {
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  1  1 103        .0000        0   430     0 Teilesortierung in X"));
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  0  1 104        .0000        0   430     0 Teilesortierung in Y"));
            }
            ChangeZerstanzen();
        }
        public void ClickStandartButton()
        {
            File.Copy(Path.Combine(pathToCCsettings, "POPT07_yStandart"), Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), true);
            MainWindow.Instance.xMultiShear.IsChecked = false;
            MainWindow.Instance.xStandart56.IsChecked = false;
            MainWindow.Instance.xStandart46.IsChecked = false;
            MainWindow.Instance.xStandart30.IsChecked = false;
            if (sortX)
            {
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  1  1 103        .0000        0   430     0 Teilesortierung in X"));
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  0  1 104        .0000        0   430     0 Teilesortierung in Y"));
            }
            ChangeZerstanzen();
        }
        public void ClickSechsundfuenzigButton()
        {
            File.Copy(Path.Combine(pathToCCsettings, "POPT07_yStahl56"), Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), true);
            MainWindow.Instance.xMultiShear.IsChecked = false;
            MainWindow.Instance.xStandart76.IsChecked = false;
            MainWindow.Instance.xStandart46.IsChecked = false;
            MainWindow.Instance.xStandart30.IsChecked = false;
            if (sortX)
            {
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  1  1 103        .0000        0   430     0 Teilesortierung in X"));
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  0  1 104        .0000        0   430     0 Teilesortierung in Y"));
            }
            ChangeZerstanzen();
        }
        public void ClickSechsundvierzigButton()
        {
            File.Copy(Path.Combine(pathToCCsettings, "POPT07_yStahl46"), Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), true);
            MainWindow.Instance.xMultiShear.IsChecked = false;
            MainWindow.Instance.xStandart56.IsChecked = false;
            MainWindow.Instance.xStandart76.IsChecked = false;
            MainWindow.Instance.xStandart30.IsChecked = false;
            if (sortX)
            {
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  1  1 103        .0000        0   430     0 Teilesortierung in X"));
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  0  1 104        .0000        0   430     0 Teilesortierung in Y"));
            }
            ChangeZerstanzen();
        }
        public void ClickDreissigButton()
        {
            File.Copy(Path.Combine(pathToCCsettings, "POPT07_yStahl30"), Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), true);
            MainWindow.Instance.xMultiShear.IsChecked = false;
            MainWindow.Instance.xStandart56.IsChecked = false;
            MainWindow.Instance.xStandart46.IsChecked = false;
            MainWindow.Instance.xStandart76.IsChecked = false;
            if (sortX)
            {
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  1  1 103        .0000        0   430     0 Teilesortierung in X"));
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  0  1 104        .0000        0   430     0 Teilesortierung in Y"));
            }
            ChangeZerstanzen();
        }

            public void ClickSortXButton()
        {
            if ((bool)MainWindow.Instance.xSortX.IsChecked)
            {
                sortX = true;
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  0  1 103        .0000        0   430     0 Teilesortierung in X", " 6  1  1 103        .0000        0   430     0 Teilesortierung in X"));
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  0  1 104        .0000        0   430     0 Teilesortierung in Y"));
            }
            else
            {
                sortX = false;
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  1  1 103        .0000        0   430     0 Teilesortierung in X", " 6  1  1 103        .0000        0   430     0 Teilesortierung in X", " 6  0  1 103        .0000        0   430     0 Teilesortierung in X"));
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 6  0  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  0  1 104        .0000        0   430     0 Teilesortierung in Y", " 6  1  1 104        .0000        0   430     0 Teilesortierung in Y"));
            }
            Console.WriteLine("sortX =  " + sortX);
        }

        private void ChangeZerstanzen()
        {
            int userVal = int.Parse(MainWindow.Instance.txtZerstanzen.Text);
            if (userVal == 120)
                return;
            if (userVal <= 99)
            {
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 2  0  0 111     120.0000        0    30   230", " 2  0  0 111     120.0000        0    30   230", " 2  0  0 111     " + "0" + userVal + ".0000        0    30   230"));
                Console.WriteLine("Zerstanzen =  " + userVal);
            }
            else if (userVal >= 100)
            {
                Console.WriteLine(FindLineNumber(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 2  0  0 111     120.0000        0    30   230", " 2  0  0 111     120.0000        0    30   230", " 2  0  0 111     " + userVal + ".0000        0    30   230"));
                Console.WriteLine("Zerstanzen =  " + userVal);
            }
        }


        private int FindLineNumber(string fileName, string trackText, string oldText, string newText)
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
            return lineNumber;
        }


        public void ClickSelectVerticalTrimTool15x5Button()
        {
            string trimTool = "15.0000";
            ChangeVerticalTrimTool(trimTool);
        }

        public void ClickSelectVerticalTrimTool30x5Button()
        {
            string trimTool = "30.0000";
            ChangeVerticalTrimTool(trimTool);
        }

        public void ClickSelectVerticalTrimTool46x5Button()
        {
            string trimTool = "46.0000";
            ChangeVerticalTrimTool(trimTool);
        }

        public void ClickSelectVerticalTrimTool56x5Button()
        {
            string trimTool = "56.0000";
            ChangeVerticalTrimTool(trimTool);
        }

        public void ClickSelectVerticalTrimTool76x5Button()
        {
            string trimTool = "76.0000";
            ChangeVerticalTrimTool(trimTool);
        }

        public void ClickSelectVerticalTrimTool762x5Button()
        {
            string trimTool = "76.2000";
            ChangeVerticalTrimTool(trimTool);
        }

        private void ChangeVerticalTrimTool(string userVal)
        {
            string lineName = CheckLineName(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), " 2  0  0 321      ");
            Console.WriteLine(lineName);
            Console.WriteLine(ChangeInLine(Path.Combine(pathTodefaultUserCCsettings, defaultCCPop), lineName, lineName, " 2  0  0 321      " + userVal + "        0   440   620 Dimension in X"));

            Console.WriteLine("Vertical Trim Tool =  " + userVal);
        }

        public void ClickSelectTrimTool762x5Button()
        {
            SetTrimTool("76.2000");
        }

        public void ClickSelectTrimTool76x5Button()
        {
            SetTrimTool("76.0000");
        }

        public void ClickSelectTrimTool56x5Button()
        {
            SetTrimTool("56.0000");
        }

        public void ClickSelectTrimTool46x5Button()
        {
            SetTrimTool("46.0000");
        }

        public void ClickSelectTrimTool30x5Button()
        {
            SetTrimTool("30.0000");
        }

        private void SetTrimTool(string v)
        {
            pnMPLfolder = Path.Combine(pathToUserFolder, "pn.mpl");
            string[] files = Directory.GetFiles(pnMPLfolder);
            int iForFiles = 1;
            bool resulting;
            trimToolSelected = v;
            foreach (string file in files)
            {
                //Console.WriteLine("Nr " + iForFiles + "  " + Path.GetFileName(file));

                resulting = Path.GetFileName(file).Contains("QUELLE");
                if (resulting == true)
                {
                    Console.WriteLine("Nr " + iForFiles + "  " + Path.GetFileName(file));
                    string lineName = CheckLineName(file, "X-DIM OF PARTING TOOL........................");
                    Console.WriteLine(lineName);
                    Console.WriteLine(ChangeInLine(file, lineName, lineName, "      " + trimToolSelected + "        X-DIM OF PARTING TOOL........................"));

                    //xlWorksheetFileNames.Cells[iForFiles, 1] = Path.GetFileNameWithoutExtension(file);      76.2000        X-DIM OF PARTING TOOL........................
                    iForFiles++;
                }
            }
        }

        public void Click76zu762Button()
        {
            SetMSTool();
        }

        private void SetMSTool()
        {
            pnMPLfolder = "C:\\pnusers\\default\\pn.mpl";
            string[] files = Directory.GetFiles(pnMPLfolder);
            int iForFiles = 1;
            bool resulting;
            //trimToolSelected = v;
            foreach (string file in files)
            {
                //Console.WriteLine("Nr " + iForFiles + "  " + Path.GetFileName(file));

                resulting = Path.GetFileName(file).Contains("QUELLE");
                if (resulting == true)
                {
                    while (CheckLineNumberChangeMSTool(file, "04760050") == 1)
                    {
                        Console.WriteLine("Nr " + iForFiles + "  " + Path.GetFileName(file));
                        string lineName = CheckLineName(file, "04760050");
                        Console.WriteLine(lineName);
                        Console.WriteLine(ChangeInLine(file, lineName, lineName, "04762050"));
                        string lineName1 = CheckLineName(file, "   1   76.0000    5.0000");
                        Console.WriteLine(lineName1);
                        Console.WriteLine(ChangeInLine(file, lineName1, lineName1, "   1   76.2000    5.0000"));

                        //xlWorksheetFileNames.Cells[iForFiles, 1] = Path.GetFileNameWithoutExtension(file);      76.2000        X-DIM OF PARTING TOOL........................
                        iForFiles++;
                    }
                }
            }
        }

        internal void ClickWLASTCommandButton(string xTool)
        {
            Console.WriteLine(xTool);
            File.Copy(Path.Combine(pathToCCsettings, xTool), "C:\\pnusers\\default\\WLAST", true);
        }

        private int CheckLineNumberChangeMSTool(string fileName, string trackText)
        {
            int lineNumber = 0;
            // fileName = "P:" + fileName;
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

        private string CheckLineName(string fileName, string trackText)
        {
            int lineNumber = 0;
            string[] textLine = System.IO.File.ReadAllLines(Path.Combine(pathToPNdrive, fileName));
            string lineName = "";
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
                        lineName = textLine[i];
                        break; //go out from loop
                    }
            }
            return lineName;
        }

        private int ChangeInLine(string fileName, string trackText, string oldText, string newText)
        {
            int lineNumber = 0;
            //   fileName = "P:" + fileName;
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
            return lineNumber;
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
