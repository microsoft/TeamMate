using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Transfer;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for DropTestWindow.xaml
    /// </summary>
    public partial class DropTestWindow : Window
    {
        public DropTestWindow()
        {
            InitializeComponent();

            this.dropTarget.Drop += dropTarget_Drop;
        }

        void dropTarget_Drop(object sender, DragEventArgs e)
        {
            var dataObject = e.Data;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Join(", ", dataObject.GetFormats().OrderBy(f => f)));

            foreach (var item in dataObject.GetFormats())
            {
                try
                {
                    object data = dataObject.GetData(item);
                    string typeName = data.GetType().FullName;
                    Debug.WriteLine("{0}: {1}", item, typeName);


                    if (item == CustomDataFormats.FileGroupDescriptorW)
                    {
                        FileGroup fileGroup = dataObject.GetFileGroup();
                        foreach (var fn in fileGroup.Items)
                        {
                            Debug.WriteLine(fn.FileName);
                        }
                    }
                    else if (data is String)
                    {
                        Debug.WriteLine((String)data);
                    }
                    else if (data is Stream)
                    {
                        Encoding enc = Encoding.Unicode;
                        if (item == "UniformResourceLocator")
                        {
                            enc = Encoding.ASCII;
                        }
                        StreamReader sr = new StreamReader((Stream)data, enc);
                        string text2 = sr.ReadToEnd();
                        Debug.WriteLine(text2);
                    }
                    Debug.WriteLine("--END");
                }
                catch (Exception)
                {
                }
            }

            text.Text = sb.ToString();
        }
    }
}
