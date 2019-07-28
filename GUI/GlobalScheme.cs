using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace DM_CS.GUI
{
    internal static class GlobalScheme
    {
        //public static List<ListView> dm_ListView_List = new List<ListView>();

        //只能加不能减
        public static int GroupID = 0;

        public static int FoucsGourpID = 0;

        public static string MergerComboSelect = "0";

        public static Dictionary<int, MyGourp> GroupDictList = new Dictionary<int, MyGourp>();

        public static string[] AllowExt = new string[] { ".bmp", ".jpg", ".png" };

        public static MyGourpDict DirListView = new MyGourpDict();

        public static bool IsRegexMode = false;

    }


    internal static class RegexMatchAll
    {
        public static int MatchFileCount { get { return MatchFiles.Count; } }

        //这个用来做合成时的匹配字典
        public static Dictionary<string, string> MatchFiles = new Dictionary<string, string>();

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