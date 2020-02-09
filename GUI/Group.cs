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
            tempGroup.GroupRegexButton.Click += ClickR;
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
            tempGroup.GroupRegexTextBox.Margin = tempMargin;//ChangeGroupSize还会调整一次

			//计算的单group框宽度
			int groupWidth = Convert.ToInt32((Width - 332) / (GroupDock.Children.Count + 1)) - 10;      
            tempGroup.Colum.Width = groupWidth - 10;

            GlobalScheme.GroupID += 1;
            GlobalScheme.GroupDictList.Add(tempGroup.ID, tempGroup);

            StatusPrint(string.Format("Group{0}已添加", GlobalScheme.GroupID));
            
            GroupDock.Children.Add(tempGroup.MyControl);
            RegexDock.Children.Add(tempGroup.GroupRegexTextBox);
            RegexDock.Children.Add(tempGroup.GroupRegexButton);
			RegexDock.Children.Add(tempGroup.MustNeedChecBox);

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
                    i.Width = groupWidth - 44;
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
            var reText = GlobalScheme.GroupDictList[ID].GroupRegexTextBox.Text;
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

	/// <summary>
	/// 自定义的一个Group包含的全部元素
	/// </summary>
    internal class MyGourp
    {

        public int ID { get; }

		/// <summary>
		/// 自定义的组字典
		/// </summary>
        public MyGourpDict Dict { get; set; }
		/// <summary>
		/// ListView控件实体
		/// </summary>
        public ListView MyControl { get; }
		/// <summary>
		/// 每个Group下的正则表达式输入框
		/// </summary>
        public TextBox GroupRegexTextBox { get; }
		/// <summary>
		/// 正则输入框旁边的R按钮
		/// </summary>
        public Button GroupRegexButton { get; }
		/// <summary>
		/// 是否必须图片复选框
		/// </summary>
		public CheckBox MustNeedChecBox { get; }
        public GridViewColumn Colum { get; }

        public MyGourp(int ID)
        {
            this.ID = ID;
            this.Dict = new MyGourpDict();
			//ListView相关
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
            this.MyControl.MouseEnter += GroupEnter;
			this.MyControl.MouseLeave += GroupLevae;

			this.MyControl.MouseLeftButtonDown += listview_mouseup;

			//下方输入框和按钮
            this.GroupRegexTextBox = new TextBox();
            this.GroupRegexTextBox.Tag = ID;

			//Button
            this.GroupRegexButton = new Button();
            this.GroupRegexButton.Tag = ID;
            this.GroupRegexButton.Content = "R";
			this.GroupRegexButton.Width = 21;
			this.GroupRegexButton.Height = 21;
			Thickness tempMargin = this.GroupRegexButton.Margin;
			tempMargin.Left = 3;
			this.GroupRegexButton.Margin = tempMargin;

			//CheckBox
			this.MustNeedChecBox = new CheckBox();
			this.MustNeedChecBox.Tag = ID;
			this.MustNeedChecBox.Width = 17;
			this.MustNeedChecBox.Height = 21;
			var checkBoxST = new System.Windows.Media.ScaleTransform(1.1, 1.1);
			this.MustNeedChecBox.RenderTransform = checkBoxST;
			tempMargin = this.MustNeedChecBox.Margin;
			tempMargin.Top = 4;
			tempMargin.Left = 3;
			this.MustNeedChecBox.Margin = tempMargin;

			//this.MyControl.Drop += dm_GroupDrop;

			//Column生成，风格调整为左对齐
			Colum = new GridViewColumn();
            Style groupStyle = new Style(typeof(GridViewColumnHeader));
            groupStyle.Setters.Add(new Setter(GridViewColumnHeader.HorizontalContentAlignmentProperty,
                 HorizontalAlignment.Left));
            this.Colum.HeaderContainerStyle = groupStyle;
            this.Colum.Header = string.Format("Group{0}", GlobalScheme.GroupID + 1);



            //ContextMenu
            ContextMenu contextMenu = new ContextMenu();
            var menuItemDelete = new MenuItem();
            menuItemDelete.Header = "删除";
			menuItemDelete.Click += GroupDelete;
			contextMenu.Items.Add(menuItemDelete);
            var menuItemClear = new MenuItem();
            menuItemClear.Header = "清空";
            menuItemClear.Click += GroupClear;
            contextMenu.Items.Add(menuItemClear);
            this.MyControl.ContextMenu = contextMenu;

            //GridView
            var tempView = new GridView();
            tempView.Columns.Add(Colum);
            this.MyControl.View = tempView;
        }
		/// <summary>
		/// 本Grou成为为焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void GroupEnter(object sender, MouseEventArgs e)
        {
            GlobalScheme.FoucsGourpID = this.ID;
        }

		/// <summary>
		/// 失去焦点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GroupLevae(object sender, MouseEventArgs e)
		{
			GlobalScheme.FoucsGourpID = -1;
		}

		/// <summary>
		/// 清空 选项
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GroupClear(object sender, RoutedEventArgs e)
        {
			this.Dict.Clear();
		}

		/// <summary>
		/// 删除 选项
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GroupDelete(object sender, RoutedEventArgs e)
		{
			var tempList = this.MyControl.SelectedItems.Cast<string>().ToList();
			foreach (var key in tempList)
			{
				this.Dict.Remove((string)key);
			}
		}

		private void listview_mouseup(object sender, MouseButtonEventArgs e)
        {
            this.MyControl.SelectedIndex = -1;
        }
    }
    /// <summary>
    /// Group专用自定义字典
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
		public new void Remove(string key)
		{
			base.Remove(key);
			OCKeys.Remove(key);
		}
		public new void Clear()
        {
            base.Clear();
            OCKeys.Clear();
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
