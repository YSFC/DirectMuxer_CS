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
using System.Threading;
using System.Threading.Tasks;
using DM_CS.PictureCore;

namespace DM_CS.GUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            dm_CreateGroup(sender, e);
            dm_CreateGroup(sender, e);
            dm_DirListView.ItemsSource = GlobalScheme.DirListView.OCKeys;
            StatusPrint("就绪");
        }

        internal void ErrorPrint(string Title, string Msg)
        {
            var EOW = new ErrorOrWarning();
            EOW.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            EOW.Owner = this;
            EOW.ChangeTitle(Title);
            EOW.ChangeText(Msg);
            EOW.ShowInTaskbar = false;
            EOW.ShowDialog();
            EOW.Close();
        }

        internal void AboutBoxOpen(object sender, RoutedEventArgs e)
        {
            var AboutBoxWin = new AboutBox();
            AboutBoxWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            AboutBoxWin.Owner = this;
            AboutBoxWin.ShowDialog();
            AboutBoxWin.Close();
        }

        private void button_ClearAll(object sender, RoutedEventArgs e)
        {
            GlobalScheme.GroupID = 0;
            GlobalScheme.GroupDictList.Clear();
            GroupDock.Children.Clear();
            RegexDock.Children.Clear();
            GlobalScheme.DirListView.Clear();
            GlobalScheme.MergerComboSelect = "0";
            RegexMatchAll.FilesOfKey.Clear();
            RegexMatchAll.MatchFiles.Clear();
            RegexMatchAll.MergeSchemeList.Clear();
            regex_TextBox.Clear();
            dir_TextBox.Clear();
            this.Width = 800;
            this.Height = 450;
            ChangeGroupSize();
            Load(sender, e);
        }

        public void StatusPrint(string text)
        {
            Dispatcher.Invoke(() => { StatusBarTextBlock.Text = text.Trim(); });
        }

        /// <summary>
        /// 合成按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Merge(object sender, RoutedEventArgs e)
        {
            if (workingCount != 0)
            {
                StatusPrint("在处理中。。。");
                return;
            }
            GlobalScheme.MergedCount = 0;
            GlobalScheme.MergedErrorCount = 0;

            workingCount++;
            semaphore = new Semaphore(Scheme.MaxThread, Scheme.MaxThread);
            if (GlobalScheme.IsRegexMode)
            {
                foreach (MyGourp item in GroupDock.Children)
                {
                    ClickR(item.GroupRegexButton, e);
                }
                foreach (var itemKey in RegexMatchAll.MergeSchemeList.Keys.OrderBy(x => x))
                {
                    var merger_lists = new List<string[]>();
                    var mustNeedInfoList = new List<bool?>();
                    foreach (var groupID in RegexMatchAll.MergeSchemeList[itemKey].Keys.OrderBy(x => x))
                    {
                        var tempStrings = RegexMatchAll.MergeSchemeList[itemKey][groupID];
                        merger_lists.Add(tempStrings);
                        mustNeedInfoList.Add(GlobalScheme.GroupDictList[groupID].MustNeedChecBox.IsChecked);
                    }
                    //上面和下面的注释情况差不多的，具体实现功能已经完善，除去后续追加的图片可有无状态不需要更改
                    Task.Run(new Action(() => { ListXListAndMerge(merger_lists, mustNeedInfoList, merger_lists.Count); workingCount--; }));
                }
            }
            else
            {
                var merger_lists = new List<string[]>();
                var mustNeedInfoList = new List<bool?>();
                foreach (var group in GlobalScheme.GroupDictList)
                {
                    //提取每个组的文件，转换成string[]类型，添加进string[]列表
                    var tempStrings = group.Value.Dict.Values.ToArray();
                    merger_lists.Add(tempStrings);
                    mustNeedInfoList.Add(group.Value.MustNeedChecBox.IsChecked);
                }
                Task.Run(new Action(() => { ListXListAndMerge(merger_lists, mustNeedInfoList, merger_lists.Count); workingCount--; }));
            }

            Task.Run(new Action(() =>
            {
                while (true)
                {
                    Thread.Sleep(500);
                    if (workingCount == 0)
                    {                     
                        break;
                    }
                }
                if(GlobalScheme.MergedCount == 0 && GlobalScheme.MergedErrorCount == 0)
                {
                    //不用做什么，有提示的
                }
                else if (GlobalScheme.MergedErrorCount == 0)
                {
                    StatusPrint(string.Format("成功合成{0}张", GlobalScheme.MergedCount.ToString()));
                }
                else
                {
                    StatusPrint(string.Format("成功合成{0}张，合成失败{1}张", GlobalScheme.MergedCount.ToString(), GlobalScheme.MergedErrorCount.ToString()));
                }
                EvClearAllGroup(sender, e);
            }));
        }


		/// <summary>
		/// 改变了合成方式
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void box_MergerStyleChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem getCB = mergerComboBox.SelectedItem as ComboBoxItem;
            GlobalScheme.MergerComboSelect = getCB.Tag.ToString();
        }

        private void DropOfDirLV(object sender, DragEventArgs e)
        {
            StatusPrint("不允许拖入。");
        }

        private void dm_FileDialog(object sender, RoutedEventArgs e)
        {
            SWF.FolderBrowserDialog dialog = new SWF.FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == SWF.DialogResult.OK)
            {
                GlobalScheme.DirListView.Clear();
                string foldPath = dialog.SelectedPath;
                var files = Directory.GetFiles(foldPath);
                foreach (var file in files) {
                    GlobalScheme.DirListView.Add(Path.GetFileName(file),file);
                }
                dir_TextBox.Text = foldPath;
                //SWF.MessageBox.Show("已选择文件夹:" + foldPath, "选择文件夹提示", SWF.MessageBoxButtons.OK, SWF.MessageBoxIcon.Information);
            }
        }

		/// <summary>
		/// 按下左下角的R按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void button_regexMain(object sender, RoutedEventArgs e)
        {
            if (!GlobalScheme.IsRegexMode)
            {
                StatusPrint("未启用正则模式。");
                return;
            }
            try
            {
                var mainRe = new Regex(regex_TextBox.Text,RegexOptions.IgnoreCase);

                //清空全部匹配
                RegexMatchAll.MatchFiles.Clear();
                RegexMatchAll.FilesOfKey.Clear();
                RegexMatchAll.MergeSchemeList.Clear();

                foreach (var filename in GlobalScheme.DirListView.Keys)
                {                    
                    var tempMatch = mainRe.Match(filename);
                    if (!tempMatch.Success)
                        continue;
                    //成功匹配都塞捕获文件列表里面
                    RegexMatchAll.MatchFiles.Add(filename, GlobalScheme.DirListView[filename]);

                    //不知道用Groups去做key会发生什么事情，还是转换成string吧
                    var matchKey = "";
                    for(var j = 1;j < tempMatch.Groups.Count;j++)
                    {
                        matchKey += tempMatch.Groups[j] + "|";
                    }
                   
                    //不存在先建立
                    if (!RegexMatchAll.FilesOfKey.ContainsKey(matchKey))
                    {
                        RegexMatchAll.FilesOfKey.Add(matchKey, new List<string>());
                    }
                    if (!RegexMatchAll.MergeSchemeList.ContainsKey(matchKey))
                    {
                        RegexMatchAll.MergeSchemeList.Add(matchKey, new Dictionary<int, string[]>());
                    }

                    //不用于任何显示，直接用raw path
                    RegexMatchAll.FilesOfKey[matchKey].Add(GlobalScheme.DirListView[filename]);                    
                }

                foreach (var buttonR in RegexDock.Children)
                {
                    if (buttonR is Button)
                        (buttonR as Button).RaiseEvent(e);
                }

                StatusPrint(string.Format("成功匹配{0}个文件", RegexMatchAll.MatchFiles.Count()));
            }
            catch(ArgumentException A)
            {
                StatusPrint("错误的正则表达式：" + A.Message);
            }
        }

        private void toNormalMode(object sender, RoutedEventArgs e)
        {
            GlobalScheme.IsRegexMode = false;
            foreach(ListView i in GroupDock.Children)
            {
                i.AllowDrop = true;
            }
            foreach (var i in GlobalScheme.GroupDictList.Values)
            {
                i.Dict.Clear();
            }
            RegexMatchAll.MatchFiles.Clear();
            RegexMatchAll.FilesOfKey.Clear();
            RegexMatchAll.MergeSchemeList.Clear();
        }

        private void toRegexMode(object sender, RoutedEventArgs e)
        {
            GlobalScheme.IsRegexMode = true;
            foreach (ListView i in GroupDock.Children)
            {
                i.AllowDrop = false;
            }
            foreach (var i in GlobalScheme.GroupDictList.Values)
            {
                i.Dict.Clear();
            }

        }

        private void EvDirTBKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {                
                string foldPath = dir_TextBox.Text;                
                if (Directory.Exists(foldPath))
                {
                    GlobalScheme.DirListView.Clear();
                    var files = Directory.GetFiles(foldPath);
                    foreach (var file in files)
                    {
                        GlobalScheme.DirListView.Add(Path.GetFileName(file), file);
                    }
                }
                else
                {
                    StatusPrint("错误的路径。");
                }
            }
        }

        /// <summary>
        /// 关闭主窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 清空全部Group内元素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EvClearAllGroup(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var i in GlobalScheme.GroupDictList)
                {
                    i.Value.Dict.Clear();
                }
            }));
           
        }

		/// <summary>
		/// 清空焦点Group内元素
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EvClearGroup(object sender, RoutedEventArgs e)
		{
			if (GlobalScheme.FoucsGourpID != -1)
			{
				GlobalScheme.GroupDictList[GlobalScheme.FoucsGourpID].Dict.Clear();
			}
		}

		/// <summary>
		/// 保存设置
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveSettings(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GlobalScheme.PicturePreviewWindow != null && GlobalScheme.PicturePreviewWindow.IsVisible)
            {
                GlobalScheme.PicturePreviewWindow.Close();
            }

            PictureCore.Scheme.Save();
        }

        private void EvSetMI(object sender, RoutedEventArgs e)
        {
            var MSetWin = new SetWin();
            MSetWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            MSetWin.Owner = this;
            MSetWin.ShowInTaskbar = false;
            MSetWin.ShowDialog();
            MSetWin.Close();
        }


        public void PicturePreviewWinClosed(object sender, EventArgs e)
        {
            //关闭窗口时刷新父窗口状态
            openPicturePreview_MeanItem.IsChecked = false;
        }

        private void OpenPicturePreview(object sender, RoutedEventArgs e)
        {
            GlobalScheme.PicturePreviewWindow = new PicturePreview();
            GlobalScheme.PicturePreviewWindow.Closed += PicturePreviewWinClosed;
            GlobalScheme.PicturePreviewWindow.Show();   
        }

        private void ClosePicturePreview(object sender, RoutedEventArgs e)
        {
            GlobalScheme.PicturePreviewWindow.Close();
            GlobalScheme.PicturePreviewWindow = null;
            //openPicturePreview_MeanItem.IsChecked = false;
        }
        /// <summary>
        /// 可能修改了PA标识状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangedPA(object sender, RoutedEventArgs e)
        {
            Scheme.PAChecked = CheckPremultipliedAlpha.IsChecked;
        }

        /*private void btnFile_Click(object sender, EventArgs e)
        {
            //OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "所有文件(*.*)|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = fileDialog.FileName;
                //MessageBox.Show("已选择文件:" + file, "选择文件提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string foldPath = dialog.SelectedPath;
                MessageBox.Show("已选择文件夹:" + foldPath, "选择文件夹提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        */
        /*public class TestEntity : INotifyPropertyChanged
        {
            private string _surname;
            private string _file;

            public string Surname
            {
                get { return _surname; }
                set { _surname = value; OnPropertyChanged("Surname"); }
            }
            public string File
            {
                get { return _file; }
                set { _file = value; OnPropertyChanged("File"); }
            }

            protected void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }*/

    }



}
