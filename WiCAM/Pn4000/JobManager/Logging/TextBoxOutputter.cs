using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace WiCAM.Pn4000.JobManager
{
    public class TextBoxOutputter : TextWriter
    {
        public TextBox textBox = null;

        public TextBoxOutputter(TextBox output)
        {
            textBox = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value.ToString());
            }));
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

       
    }
}
