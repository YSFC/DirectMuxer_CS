using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DM_CS.GUI
{
    internal static class GlobalScheme
    {
        //public static List<ListView> dm_ListView_List = new List<ListView>();

        //只能加不能减
        public static int GroupID = 0;

        //Group的焦点，用来记录最后活动的Group
        public static int FoucsGourpID = 0;

        //合成模式选项
        public static string MergerComboSelect = "0";

        //Group的控件都装在这个字典内，有几个Group这字典内就有几个元素
        public static Dictionary<int, MyGourp> GroupDictList = new Dictionary<int, MyGourp>();

        //允许的图片后缀，在导入IM库后可以增加许多支持，比如PSD、webp什么的。
        public static string[] AllowExt = new string[] { ".bmp", ".jpg", ".png" };

        //文件夹下全部文件，用于最左边的ListView的显示和合成时的快速获取文件名。
        public static MyGourpDict DirListView = new MyGourpDict();

        //正则合成模式的状态
        public static bool IsRegexMode = false;

        //图片预览窗口，平时应该为null，活动时才是一个PicturePreview类
        public static PicturePreview PicturePreviewWindow = null;

        //图片预览窗口所使用的BitmapSource，初始应与图片控件绑定
        public static BitmapSource PreviewBS;

    }

	/// <summary>
	/// 正则匹配的相关数据
	/// </summary>
    internal static class RegexMatchAll
    {
        public static int MatchFileCount { get { return MatchFiles.Count; } }

		/// <summary>
		/// 这个用来做合成时的匹配字典
		/// </summary>
		public static Dictionary<string, string> MatchFiles = new Dictionary<string, string>();

		/// <summary>
		/// 文件列表（一个key对应的）
		/// </summary>
        public static Dictionary<string, List<string>> FilesOfKey = new Dictionary<string, List<string>>();

        //一个主key对应Group数量的string[]，依靠字典筛选
        public static Dictionary<string, Dictionary<int, string[]>> MergeSchemeList = new Dictionary<string, Dictionary<int, string[]>>();

        /// <summary>
        /// 这个函数有问题，暂时别用
        /// </summary>
        /// <returns></returns>
        private static int calcSumCount()
        {
            var getCount = 0;
            foreach(var fileListOfKey in FilesOfKey.Values)
            {
                var tempV = 1;
                foreach (var fileList in fileListOfKey)
                {
                    tempV *= fileList.Count();
                }

                getCount += tempV;
            }
            return getCount;
        }
    }
}