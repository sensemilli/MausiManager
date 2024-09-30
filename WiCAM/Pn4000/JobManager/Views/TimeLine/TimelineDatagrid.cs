using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace WiCAM.Pn4000.JobManager
{
    public class TimelineDatagrid: DataGrid
    {
        public static DependencyProperty StartTimeProperty = 
            DependencyProperty.Register("StartTime", typeof(int), typeof(TimelineDatagrid));

        public int StartTime
        {
            get { return (int)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static DependencyProperty EndTimeProperty = 
            DependencyProperty.Register("EndTime", typeof(int), typeof(TimelineDatagrid));

        public int EndTime
        {
            get { return (int)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }
    }
}
