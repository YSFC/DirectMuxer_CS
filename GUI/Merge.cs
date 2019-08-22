using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using DM_CS.PictureCore;

namespace DM_CS.GUI
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 用合成列表来合成图片
        /// </summary>
        /// <param name="merger_lists"></param>
        /// <returns></returns>
        internal int Merger(List<string[]> merger_lists)
        {            
            var merger_pics_list = new List<List<string>>();
            foreach(var basePic in merger_lists[0])
            {
                merger_pics_list.Add(new List<string>{ basePic });
            }
            
            bool fristSW = true;
            foreach(var i in merger_lists)
            {
                if (fristSW)
                {
                    fristSW = false;
                    continue;
                }
                merger_pics_list = ListXList(merger_pics_list, i, true);
            }

            var mergedCount = 0;
            foreach (var i in merger_pics_list)
            {
                mergedCount += 1;
                //StatusPrint(string.Format("处理中：{0}/{1}", mergedCount, merger_pics_list.Count));
                
                var outPic = PicMerger.MergerOfStrings(i.ToArray());
                outPic.Save();
                outPic.Close();                
            }
            return 1;
        }

        /// <summary>
        /// 列表交叉，达到直积的效果
        /// </summary>
        /// <param name="allMergeList"></param>
        /// <param name="addList"></param>
        /// <param name="MustNeed">是否必须使用</param>
        /// <returns></returns>
        internal List<List<string>> ListXList(List<List<string>> allMergeList, string[] addList, bool MustNeed)
        {
            var ReturnList = new List<List<string>>();
            foreach (var aList in allMergeList)
            {
                foreach (var addFile in addList)
                {
                    var tempList =  aList.ToList();
                    tempList.Add(addFile);
                    ReturnList.Add(tempList);
                }

                if (!MustNeed)
                {
                    ReturnList.AddRange(allMergeList);
                }
            }
            return (ReturnList);
        }

        /// <summary>
        /// 预览图合成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void PreviewMerger(object sender, SelectionChangedEventArgs e)
        {
            //如果预览图窗口没打开，不需要处理。
            if (GlobalScheme.PicturePreviewWindow == null)
                return;

            var previewMergerList = new List<string>();
            foreach(var i in GlobalScheme.GroupDictList.Values.OrderBy(x => x.ID))
            {
                var tempImage = i.MyControl.SelectedItems;
                if (tempImage == null || tempImage.Count == 0)
                    continue;
                previewMergerList.Add(i.Dict[tempImage[0].ToString()]);                
            }

            if (previewMergerList.Count != 0)
            {
                var outPic = PicMerger.MergerOfStrings(previewMergerList.ToArray());
                GlobalScheme.PreviewBS = outPic.GetBitmap();
                GlobalScheme.PicturePreviewWindow.RefreshImage();
            }
        }

    }
}
