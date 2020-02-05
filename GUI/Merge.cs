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
		#region 废弃
		///// <summary>
		///// 用合成列表来合成图片
		///// </summary>
		///// <param name="merger_lists"></param>
		///// <returns></returns>
		//internal int Merger(List<string[]> merger_lists)
		//{            
		//    var merger_pics_list = new List<List<string>>();
		//    foreach(var basePic in merger_lists[0])
		//    {
		//        merger_pics_list.Add(new List<string>{ basePic });
		//    }

		//    bool fristSW = true;
		//    foreach(var i in merger_lists)
		//    {
		//        if (fristSW)
		//        {
		//            fristSW = false;
		//            continue;
		//        }
		//        merger_pics_list = ListXList(merger_pics_list, i, true);
		//    }

		//    var mergedCount = 0;
		//    foreach (var i in merger_pics_list)
		//    {
		//        mergedCount += 1;
		//        //StatusPrint(string.Format("处理中：{0}/{1}", mergedCount, merger_pics_list.Count));

		//        var outPic = PicMerger.MergerOfStrings(i.ToArray());
		//        outPic.Save();
		//        outPic.Close();                
		//    }
		//    return 1;
		//}
		#endregion

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

		/// <summary>
		/// 列表交叉并合成，不出意外应该是最好的方法
		/// </summary>
		/// <param name="merger_lists">需要处理的文件组的列表</param>
		/// <param name="needListLen">需要处理的列表数量，主要是内部迭代用。</param>
		internal void ListXListAndMerge(List<string[]> merger_lists, List<bool?> mustNeedInfoList, int needListLen, ImageOpen inputImage = null)
		{
			if (needListLen < 1)
			{
				//needListLen必定大于等于1，不然就是index超出范围
				throw new IndexOutOfRangeException("卧槽怎么做到0个group合成的？");
			}

			//如果中间某个列表为空，会造成无法处理后续列表的情况
			if (merger_lists[merger_lists.Count - needListLen].Count() == 0)
			{
				if (needListLen > 1)
				{
					ListXListAndMerge(merger_lists, mustNeedInfoList, needListLen - 1, inputImage);
				}
				else if (inputImage != null)
				{
					inputImage.Save();
				}
				else
				{
					StatusPrint("请输入图片！");
				}
			}

			foreach (var file in merger_lists[merger_lists.Count - needListLen])
			{
				ImageOpen baseImage;
				if (inputImage == null)
				{
					//一般第一个组就会变成这种情况，会作为base图片处理
					baseImage = new ImageOpen(file);
				}
				else
				{
					var diffImage = new ImageOpen(file);
					//baseImage = inputImage.Clone();
					baseImage = PicMerger.Merger(inputImage, diffImage, int.Parse(GlobalScheme.MergerComboSelect));
				}

				if (needListLen > 1)
				{
					//如果这个Grou非必选，要分开原图和合成后图片处理
					if (mustNeedInfoList[merger_lists.Count - needListLen] != true && inputImage != null)
					{
						ListXListAndMerge(merger_lists, mustNeedInfoList, needListLen - 1, inputImage);
					}
					ListXListAndMerge(merger_lists, mustNeedInfoList, needListLen - 1, baseImage);
				}
				else
				{
					
					if (baseImage.MergedCount > 0)
					{
						baseImage.Save();
					}
					if (mustNeedInfoList[merger_lists.Count - needListLen] != true && inputImage.MergedCount > 0)
					{
						inputImage.Save();
					}
				}
				baseImage.Close();
			}
		}
	}
}
