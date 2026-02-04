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
using ListViewItem = System.Windows.Forms.ListViewItem;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;
using WiCAM.Pn4000.BendModel.GeometryTools;
using MahApps.Metro.Controls;

namespace WiCAM.Pn4000.JobManager
{
    public partial class PNconfigControl : UserControl, IView, IComponentConnector
    {
        public static PNconfigControl _PNconfigControl;
        private ObservableCollection<MachineControlGridData> machineList;
        private ObservableCollection<PopUpControlGridData> popupList;

        public PNconfigControl()
        {
            this.InitializeComponent();
            _PNconfigControl = this;

            pninit.get_environment();
            pninit.get_pnSprache();
            pninit.get_popup_texte();
            //  lblLokalDir.Text = pninit.PNHomeDrive + pninit.PNHomePath;
            //  lblPNDir.Text = pninit.PNDrive + "\\u\\pn";
            //   lblArchiveDir.Text = pninit.ARDrive + "\\u\\ar";
            //   pninit.strLastPickDirectory = lblPNDir.Text;
            pninit.get_pn_machines();
            maschinen_Liste_Fill("");
            Popup_Liste_Fill("");
        }


        private void maschinen_Liste_Fill(string suchtext)
        {
            string text = "";
            listeMaschinen.Items.Clear();
            listePopup.Items.Clear();
            comboBoxSelMachine.Items.Clear();
            pninit.intAnzahlAktiveMaschinen = 0;
            if (machineList != null)
            {
                machineList.Clear();
            }
            machineList = new ObservableCollection<MachineControlGridData>();
            for (int i = 0; pninit.arrMaschinenListe[i, 0] != null; i++)
            {
                text = pninit.arrMaschinenListe[i, 0] + " | " + pninit.arrMaschinenListe[i, 1] + " | " + pninit.arrMaschinenListe[i, 2];
                Console.WriteLine(text);
                machineList.Add(new MachineControlGridData
                {
                    MaschNummer = pninit.arrMaschinenListe[i, 0],
                    MaschName = pninit.arrMaschinenListe[i, 1],
                    MaschModel = pninit.arrMaschinenListe[i, 2],
                    MaschSteuerung = pninit.arrMaschinenListe[i, 3],
                    MaschBemerkung = pninit.arrMaschinenListe[i, 4]
                });

                if (pninit.arrMaschinenListe[i, 3] != "")
                {
                    text = text + " | " + pninit.arrMaschinenListe[i, 3];
                }
                if (pninit.arrMaschinenListe[i, 4] != "")
                {
                    text = text + " | " + pninit.arrMaschinenListe[i, 4];
                }
                if (pninit.bool_Maschinen_Filter_Aktiv)
                {
                    string text2 = text.ToUpper();
                    suchtext = suchtext.ToUpper();
                    if (text2.Contains(suchtext))
                    {
                        listeMaschinen.Items.Add(new ListViewItem(new string[5]
                        {
                        pninit.arrMaschinenListe[i, 0],
                        pninit.arrMaschinenListe[i, 1],
                        pninit.arrMaschinenListe[i, 2],
                        pninit.arrMaschinenListe[i, 3],
                        pninit.arrMaschinenListe[i, 4]
                        }));
                        comboBoxSelMachine.Items.Add(text);
                        pninit.intAnzahlAktiveMaschinen++;
                    }
                }
                else
                {
                    listeMaschinen.Items.Add(new ListViewItem(new string[5]
                    {
                    pninit.arrMaschinenListe[i, 0],
                    pninit.arrMaschinenListe[i, 1],
                    pninit.arrMaschinenListe[i, 2],
                    pninit.arrMaschinenListe[i, 3],
                    pninit.arrMaschinenListe[i, 4]
                    }));
                    comboBoxSelMachine.Items.Add(text);
                    pninit.intAnzahlAktiveMaschinen++;
                }

            }
            if (comboBoxSelMachine.Items.Count != 0)
            {
                comboBoxSelMachine.SelectedIndex = 0;
            }
            listeMaschinen.Items.Clear();

            listeMaschinen.ItemsSource = machineList;
        }

        private void Popup_Liste_Fill(string suchtext)
        {
            if (comboBoxSelMachine.Text == "")
            {
                System.Windows.Forms.MessageBox.Show(Sprachen.arrSprache[135, pninit.Sprache]);
            }
            else
            {
                pninit.get_pnSprache();
                pninit.get_popup_texte();
                pninit.reset_array_popup();
                popupCheck.get_popups(comboBoxSelMachine.Text.Substring(0, 4));
                update_popup_listview();
            }
            listePopup.Focus();
        }

        private void update_popup_listview()
        {
            listePopup.Items.Clear();
            if (popupList != null)
            {
                popupList.Clear();
            }
            popupList = new ObservableCollection<PopUpControlGridData>();
            if (pninit.arrPopUpListe[0, 0] != null)
            {
                //    contextPopupListe.Enabled = true;
            }
            for (int i = 0; pninit.arrPopUpListe[i, 0] != null; i++)
            {
                Console.WriteLine(pninit.arrPopUpListe[i, 0]);
                popupList.Add(new PopUpControlGridData
                {
                    PopupDatei = pninit.arrPopUpListe[i, 0],
                    PopupTitel = pninit.arrPopUpListe[i, 1],
                    PopupVerzeichniss = pninit.arrPopUpListe[i, 2],
                    PopupAuto = pninit.arrPopUpListe[i, 3],
                });
                if (pninit.boolPopupFilterActive)
                {
                    //   if (pninit.arrPopUpListe[i, 0].ToUpper().Contains(tbPopupFilter.Text.ToUpper()))

                    if (pninit.arrPopUpListe[i, 0].ToUpper().Contains("Text"))
                    {
                        listePopup.Items.Add(new ListViewItem(new string[4]
                        {
                        pninit.arrPopUpListe[i, 0],
                        pninit.arrPopUpListe[i, 1],
                        pninit.arrPopUpListe[i, 2],
                        pninit.arrPopUpListe[i, 3]
                        }));
                    }
                }
                else
                {
                    listePopup.Items.Add(new ListViewItem(new string[4]
                    {
                    pninit.arrPopUpListe[i, 0],
                    pninit.arrPopUpListe[i, 1],
                    pninit.arrPopUpListe[i, 2],
                    pninit.arrPopUpListe[i, 3]
                    }));
                }
            }
            listePopup.Items.Clear();

            listePopup.ItemsSource = popupList;
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

        private void listeMaschinen_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MachineControlGridData selectedPopup = (MachineControlGridData)listeMaschinen.SelectedItem;
            // comboBoxSelMachine.SelectedIndex = selectedPopup.ToIEnumerable<>()
            Console.WriteLine(selectedPopup.MaschNummer);
            //  popupCheck.get_popups(selectedPopup.MaschNummer);
            //  comboBoxSelMachine.Text.Substring(0, 4);
            int index = listeMaschinen.SelectedIndex; // int.Parse(selectedPopup.MaschNummer.Substring(0, 4)) - 1;
            comboBoxSelMachine.SelectedIndex = index;
            Console.WriteLine(selectedPopup.MaschNummer);
            
        }

        private void listePopup_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            PopUpControlGridData selectedPopup = (PopUpControlGridData)listePopup.SelectedItem;
            Console.WriteLine(selectedPopup.PopupTitel);
            //foreach (ListViewItem selectedItem in listePopup.SelectedItems)
            //{
            //    text = selectedItem.Text;
            //}
            //for (int i = 0; pninit.arrPopUpListe[i, 0] != null; i++)
            //{
            //    if (pninit.arrPopUpListe[i, 0] == text)
            //    {
            //        text2 = pninit.arrPopUpListe[i, 2];
            //    }
            //}
            string arguments = selectedPopup.PopupVerzeichniss + "\\" + selectedPopup.PopupDatei;
            string textEditor = pninit.TextEditor;
            Process.Start(textEditor, arguments);
        }

        private void comboBoxSelMachine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listeMaschinen.SelectedIndex = comboBoxSelMachine.SelectedIndex;
            Console.WriteLine(comboBoxSelMachine.SelectedValue);

        }
    }

    public class MachineControlGridData
    {
        //   public bool IsChecked { get; set; }
        public string? MaschNummer { get; set; }
        public string? MaschName { get; set; }
        public string? MaschModel { get; set; }
        public string? MaschSteuerung { get; set; }
        public string? MaschBemerkung { get; set; }

        //     public int IntValue { get; set; }
    }

    public class PopUpControlGridData
    {
        //   public bool IsChecked { get; set; }
        public string? PopupDatei { get; set; }
        public string? PopupTitel { get; set; }
        public string? PopupVerzeichniss { get; set; }
        public string? PopupAuto { get; set; }

        //     public int IntValue { get; set; }
    }
}
