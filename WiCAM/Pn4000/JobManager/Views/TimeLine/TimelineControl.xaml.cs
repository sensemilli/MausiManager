using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WiCAM.Pn4000.JobManager
{
    public partial class TimelineControl : UserControl
    {
        public static DependencyProperty DaysProperty = 
            DependencyProperty.Register("Days", typeof(string), typeof(TimelineControl));

        public string Days
        {
            get { return (string)GetValue(DaysProperty); }
            set { SetValue(DaysProperty, value); }
        }

        public TimelineControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(TimelineControl_Loaded);
        }

        void TimelineControl_Loaded(object sender, RoutedEventArgs e)
        {
            string daysList = Days;
            if (!string.IsNullOrEmpty(daysList))
            {
                string[] timeWords = daysList.Split(',');
                foreach (string item in timeWords)
                {
                    switch (item.Trim())
                    {
                        case "0": Day00.Visibility = Visibility.Visible;
                            break;
                        case "1": Day01.Visibility = Visibility.Visible;
                            break;
                        case "2": Day02.Visibility = Visibility.Visible;
                            break;
                        case "3": Day03.Visibility = Visibility.Visible;
                            break;
                        case "4": Day04.Visibility = Visibility.Visible;
                            break;
                        case "5": Day05.Visibility = Visibility.Visible;
                            break;
                        case "6": Day06.Visibility = Visibility.Visible;
                            break;
                        case "7": Day07.Visibility = Visibility.Visible;
                            break;
                        case "8": Day08.Visibility = Visibility.Visible;
                            break;
                        case "9": Day09.Visibility = Visibility.Visible;
                            break;
                        case "10": Day10.Visibility = Visibility.Visible;
                            break;
                        case "11": Day11.Visibility = Visibility.Visible;
                            break;
                        case "12": Day12.Visibility = Visibility.Visible;
                            break;
                        case "13": Day13.Visibility = Visibility.Visible;
                            break;
                        case "14": Day14.Visibility = Visibility.Visible;
                            break;
                        case "15": Day15.Visibility = Visibility.Visible;
                            break;
                        case "16": Day16.Visibility = Visibility.Visible;
                            break;
                        case "17": Day17.Visibility = Visibility.Visible;
                            break;
                        case "18": Day18.Visibility = Visibility.Visible;
                            break;
                        case "19": Day19.Visibility = Visibility.Visible;
                            break;
                        case "20": Day20.Visibility = Visibility.Visible;
                            break;
                        case "21": Day21.Visibility = Visibility.Visible;
                            break;
                        case "22": Day22.Visibility = Visibility.Visible;
                            break;
                        case "23": Day23.Visibility = Visibility.Visible;
                            break;
                        case "24": Day24.Visibility = Visibility.Visible;
                            break;
                        case "25": Day25.Visibility = Visibility.Visible;
                            break;
                        case "26": Day26.Visibility = Visibility.Visible;
                            break;
                        case "27": Day27.Visibility = Visibility.Visible;
                            break;
                        case "28": Day28.Visibility = Visibility.Visible;
                            break;
                        case "29": Day29.Visibility = Visibility.Visible;
                            break;
                        case "30": Day30.Visibility = Visibility.Visible;
                            break;
                        case "31": Day31.Visibility = Visibility.Visible;
                            break;
             
                        default:
                            break;
                    }
                }
            }
        }

        private void btn_la_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_ra_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
