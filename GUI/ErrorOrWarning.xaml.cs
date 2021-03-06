﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DM_CS.GUI
{
    /// <summary>
    /// ErrorOrWarning.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorOrWarning : Window
    {
        public ErrorOrWarning()
        {
            InitializeComponent();
        }

        private void CloseErrorWindow(object sender, RoutedEventArgs e)
        {            
            this.Close();
        }

        //没有采用构造函数，而是用两个普通函数处理是为了方便看
        //反正这个窗体用不了多少次？
        public void ChangeTitle(string title)
        {
            this.Title = title;
        }

        public void ChangeText(string text)
        {
            this.ErrorMsg.Text = text;
        }
    }
}
