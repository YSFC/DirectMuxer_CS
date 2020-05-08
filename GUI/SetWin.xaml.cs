using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Media.Imaging;
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
            comboBox_MaxThread.SelectedIndex = PictureCore.Scheme.MaxThread - 1;
        }

        private void EvOutPathKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                PictureCore.Scheme.OutputDir = DirTextBox.Text;
            }
        }

        private void FileDialog(object sender, RoutedEventArgs e)
        {
            SWF.FolderBrowserDialog dialog = new SWF.FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.Description = "请选择输出路径";
            dialog.SelectedPath = DirTextBox.Text;
            if (dialog.ShowDialog() == SWF.DialogResult.OK)
            {                
                string foldPath = dialog.SelectedPath;
                DirTextBox.Text = foldPath;                
                //SWF.MessageBox.Show("已选择文件夹:" + foldPath, "选择文件夹提示", SWF.MessageBoxButtons.OK, SWF.MessageBoxIcon.Information);
            }
            dialog.Dispose();
        }

        private void EvColorChooseClick(object sender, RoutedEventArgs e)
        {
            var colorDialogWin = new System.Windows.Forms.ColorDialog();
            if (colorDialogWin.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ColorKey.Text = string.Format("#{0:X6}",colorDialogWin.Color.ToArgb() & 0xFFFFFF);
            }
            colorDialogWin.Dispose();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKClick(object sender, RoutedEventArgs e)
        {
            //先检查下
            if (!Directory.Exists(DirTextBox.Text))               
            {
                MessageBox.Show("输出路径不存在");
                tabControl.SelectedIndex = 1;
                return;
            }
            if (!Regex.IsMatch(ColorKey.Text, @"^#[0-9A-F]{6}\z", RegexOptions.IgnoreCase))
            {
                MessageBox.Show("透明通道颜色设置错误");
                tabControl.SelectedIndex = 0;
                return;
            }


            //设置
            PictureCore.Scheme.OffsetMode = int.Parse((OffsetModeCB.SelectedItem as ComboBoxItem).Tag.ToString());
            PictureCore.Scheme.OutFormat = int.Parse((OutFormatCB.SelectedItem as ComboBoxItem).Tag.ToString());
            PictureCore.Scheme.OutputDir = DirTextBox.Text;
            PictureCore.Scheme.ColorKey = ColorKey.Text;
            PictureCore.Scheme.MaxThread = int.Parse((string)(comboBox_MaxThread.SelectedItem as ComboBoxItem).Content);

            this.Close();
        }

    }
}
