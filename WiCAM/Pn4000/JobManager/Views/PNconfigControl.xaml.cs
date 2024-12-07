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

namespace WiCAM.Pn4000.JobManager
{
    public partial class PNconfigControl : UserControl, IView, IComponentConnector
    {
        public static PNconfigControl _PNconfigControl;
  

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
            for (int i = 0; pninit.arrMaschinenListe[i, 0] != null; i++)
            {
                text = pninit.arrMaschinenListe[i, 0] + " | " + pninit.arrMaschinenListe[i, 1] + " | " + pninit.arrMaschinenListe[i, 2];
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
            if (pninit.arrPopUpListe[0, 0] != null)
            {
            //    contextPopupListe.Enabled = true;
            }
            for (int i = 0; pninit.arrPopUpListe[i, 0] != null; i++)
            {
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
