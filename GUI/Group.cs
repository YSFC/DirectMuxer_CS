using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.IO;

namespace DM_CS.GUI
{
    public partial class MainWindow : Window
    {
		/// <summary>
		/// CreateGroup按钮的具体实现。
		/// 这个实现是在容器里面加入。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void dm_CreateGroup(object sender, RoutedEventArgs e)
        {
            //Width先加个多少？  
            if (GroupDock.Children.Count >= 2)
            {
                this.Width += 200;
            }
            this.Width = Math.Min(this.Width, SystemParameters.PrimaryScreenWidth);

            var tempGroup = new MyGourp(GlobalScheme.GroupID);
            tempGroup.MyControl.Drop += GroupDrop;
            tempGroup.MyControl.SelectionChanged += PreviewMerger;
            tempGroup.MyTB_Button.Click += ClickR;
            //listview
            Thickness tempMargin = tempGroup.MyControl.Margin;
            if (GroupDock.Children.Count != 0)
            {
                tempMargin.Left = 10;
            }
            else
            {
                tempMargin.Left = 0;
            }
            tempGroup.MyControl.Margin = tempMargin;
            tempGroup.MyTB.Margin = tempMargin;

            //计算的单group框宽度
            int groupWidth = Convert.ToInt32((Width - 332) / (GroupDock.Children.Count + 1)) - 10;      
            tempGroup.Colum.Width = groupWidth - 10;

            GlobalScheme.GroupID += 1;
            GlobalScheme.GroupDictList.Add(tempGroup.ID, tempGroup);

            StatusPrint(string.Format("Group{0}已添加", GlobalScheme.GroupID));
            
            GroupDock.Children.Add(tempGroup.MyControl);
            RegexDock.Children.Add(tempGroup.MyTB);
            RegexDock.Children.Add(tempGroup.MyTB_Button);

            ChangeGroupSize();
        }

        private void ChangeGroupSize()
        {
            //计算的单group框宽度
            int groupWidth = Convert.ToInt32((Width - 332) / Math.Max(GroupDock.Children.Count, 1)) - 10;

            foreach (ListView i in GroupDock.Children)
            {
                i.Width = groupWidth;
                var b = (GridView)i.View;
                b.Columns[0].Width = groupWidth - 10;
            }

            foreach (Control i in RegexDock.Children)
            {
                if (i.GetType() == typeof(TextBox))
                {
                    i.Width = groupWidth - 24;
                }
            }
        }

        /// <summary>
        /// 写得太快，回头一看有点多余的东西，就是ChangeGroupSize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EvGourpSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeGroupSize();
        }

        /// <summary>
        /// 拖放文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GroupDrop(object sender, DragEventArgs e)
        {
            var FocusListView = (ListView)sender;
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                string[] fileList = e.Data.GetData(DataFormats.FileDrop, false) as String[];
                //var drapLV = (ListView)sender;
                foreach (var file in fileList)
                {
                    if (GlobalScheme.GroupDictList[(int)FocusListView.Tag].Dict.ContainsKey(Path.GetFileName(file)))
                    {
                        //ErrorPrint("警告", "图片已经存在");
                        StatusPrint(string.Format("图片已经存在：{0}", file));
                        throw new ExistsInGroupException(string.Format("图片已经存在：{0}", file));
                        //continue;
                    }

                    if (!GlobalScheme.AllowExt.Contains(Path.GetExtension(file).ToLower()))
                    {
                        ErrorPrint("警告", "不支持的图片格式");
                        continue;
                    }

                    GlobalScheme.GroupDictList[(int)FocusListView.Tag].Dict.Add(Path.GetFileName(file), file);
                }
            }
            else
            {
                StatusPrint("无效文件");
            }
        }


		/// <summary>
		/// 正则合成的预处理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        public void ClickR(object sender, RoutedEventArgs e)
        {
            //StatusPrint(e.ToString());
            int ID = (int)(sender as Button).Tag;
            if (!GlobalScheme.IsRegexMode)
            {
                StatusPrint("未启用正则模式。");
                return;
            }
            var reText = GlobalScheme.GroupDictList[ID].MyTB.Text;
            if(reText == "")
            {
                return;
            }
            try
            {
                
                var gourpRe = new Regex(reText, RegexOptions.IgnoreCase);                
                foreach (var key in RegexMatchAll.FilesOfKey.Keys)
                {
                    var tempList = new List<string>();
                    foreach (var filename in RegexMatchAll.FilesOfKey[key])
                    {
                        //小组只确认是否匹配，不需要别的
                        if (!gourpRe.IsMatch(filename))
                            continue;                        
                        tempList.Add(filename);
                    }

                    //如果点击过MainRe按钮，会确保RegexMatchAll.MergeSchemeList[key]必定存在
                    if (RegexMatchAll.MergeSchemeList[key].ContainsKey(ID))
                    {
                        RegexMatchAll.MergeSchemeList[key][ID] = tempList.ToArray();
                    }
                    else
                    {
                        RegexMatchAll.MergeSchemeList[key].Add(ID,tempList.ToArray());
                    }                    
                }

                //原本的拖动增减图片功能禁止，功能改为文件预览用
                GlobalScheme.GroupDictList[ID].Dict.Clear();
                foreach( var i in RegexMatchAll.MergeSchemeList.First().Value[ID])
                    GlobalScheme.GroupDictList[ID].Dict.Add(Path.GetFileName(i),i);

            }
            catch (ArgumentException A)
            {
                StatusPrint("错误的正则表达式：" + A.Message);
            }
        }

    }

    internal class MyGourp
    {

        public int ID { get; }

		/// <summary>
		/// 自定义的组字典
		/// </summary>
        public MyGourpDict Dict { get; set; }
        public ListView MyControl { get; }
        public TextBox MyTB { get; }
        public Button MyTB_Button { get; }
        public GridViewColumn Colum { get; }

        public MyGourp(int ID)
        {
            this.ID = ID;
            this.Dict = new MyGourpDict();
            this.MyControl = new ListView();
            if (GlobalScheme.IsRegexMode)
            {
                this.MyControl.AllowDrop = false;
            }
            else
            {
                this.MyControl.AllowDrop = true;
            }
            this.MyControl.Tag = ID;
            this.MyControl.ItemsSource = this.Dict.OCKeys;
            this.MyControl.DragEnter += dm_GroupEnter;
            this.MyControl.MouseLeftButtonDown += listview_mouseup;            
            this.MyTB = new TextBox();
            this.MyTB.Tag = ID;
            this.MyTB_Button = new Button();
            this.MyTB_Button.Tag = ID;
            this.MyTB_Button.Content = "R";

            //this.MyControl.Drop += dm_GroupDrop;

            //Column生成，风格调整为左对齐
            Colum = new GridViewColumn();
            Style groupStyle = new Style(typeof(GridViewColumnHeader));
            groupStyle.Setters.Add(new Setter(GridViewColumnHeader.HorizontalContentAlignmentProperty,
                 HorizontalAlignment.Left));
            this.Colum.HeaderContainerStyle = groupStyle;
            this.Colum.Header = string.Format("Group{0}", GlobalScheme.GroupID + 1);

            //button
            this.MyTB_Button.Width = 21;
            this.MyTB_Button.Height = 21;
            Thickness tempMargin = this.MyTB_Button.Margin;
            tempMargin.Left = 3;
            this.MyTB_Button.Margin = tempMargin;

            //ContextMenu
            ContextMenu tempCM = new ContextMenu();
            var tempMI1 = new MenuItem();
            tempMI1.Header = "删除";
            tempCM.Items.Add(tempMI1);
			tempMI1.Click += mi_Remove;
			var tempMI2 = new MenuItem();
            tempMI2.Header = "清空";
            tempMI2.Click += mi_Clear;
            tempCM.Items.Add(tempMI2);
            this.MyControl.ContextMenu = tempCM;

            //GridView
            var tempView = new GridView();
            tempView.Columns.Add(Colum);
            this.MyControl.View = tempView;
        }

        private void dm_GroupEnter(object sender, DragEventArgs e)
        {
            GlobalScheme.FoucsGourpID = this.ID;
        }

        //清空 选项
        private void mi_Clear(object sender, RoutedEventArgs e)
        {
            this.Dict.Clear();
        }

		//删除 选项
		private void mi_Remove(object sender, RoutedEventArgs e)
		{
			foreach(var key in this.MyControl.SelectedItems.Cast<string>().ToList())
			{
				this.Dict.Remove(key);
			}
		}

		private void listview_mouseup(object sender, MouseButtonEventArgs e)
        {
            this.MyControl.SelectedIndex = -1;
        }
    }
    /// <summary>
    /// Group专用
    /// </summary>
    public class MyGourpDict : Dictionary<string, string>
    {
        public ObservableCollection<string> _OCKeys = new ObservableCollection<string>();

        public ObservableCollection<string> OCKeys { get { return _OCKeys; } }
        public new void Add(string key, string value)
        {
            base.Add(key, value);
            OCKeys.Add(key);
        }

        public new void Clear()
        {
            base.Clear();
            OCKeys.Clear();
        }

		public new void Remove(string key)
		{
			base.Remove(key);
			OCKeys.Remove(key);
		}
	}

    [Serializable]
    public class ExistsInGroupException : Exception
    {
        public ExistsInGroupException()
        {
        }
        public ExistsInGroupException(string message) : base(message)
        {
        }
        public ExistsInGroupException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
