using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Configuration;
using SWF = System.Windows.Forms;

namespace DM_CS.GUI
{
    /// <summary>
    /// SetWin.xaml 的交互逻辑
    /// </summary>
    public partial class SetWin : Window
    {
        public SetWin()
        {
            InitializeComponent();
        }

        private void EvLoad(object sender, RoutedEventArgs e)
        {
            DirTextBox.Text = PictureCore.Scheme.OutputDir;
            ColorKey.Text = PictureCore.Scheme.ColorKey;
            OffsetModeCB.SelectedIndex = PictureCore.Scheme.OffsetMode;
            OutFormatCB.SelectedIndex = PictureCore.Scheme.OutFormat;
        }

        private void EvOffsetModeChanged(object sender, SelectionChangedEventArgs e)
        {
            //Console.WriteLine(PictureCore.Scheme.OffsetMode);
            PictureCore.Scheme.OffsetMode = int.Parse((OffsetModeCB.SelectedItem as ComboBoxItem).Tag.ToString());
        }

        private void EvOutPathKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                PictureCore.Scheme.OutputDir = DirTextBox.Text;
            }
        }

        private void EvFormatChanged(object sender, SelectionChangedEventArgs e)
        {
            PictureCore.Scheme.OutFormat = int.Parse((OutFormatCB.SelectedItem as ComboBoxItem).Tag.ToString());
        }

        private void FileDialog(object sender, RoutedEventArgs e)
        {
            SWF.FolderBrowserDialog dialog = new SWF.FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == SWF.DialogResult.OK)
            {                
                string foldPath = dialog.SelectedPath;
                DirTextBox.Text = foldPath;
                PictureCore.Scheme.OutputDir = DirTextBox.Text;
                //SWF.MessageBox.Show("已选择文件夹:" + foldPath, "选择文件夹提示", SWF.MessageBoxButtons.OK, SWF.MessageBoxIcon.Information);
            }            
        }
    }
}
