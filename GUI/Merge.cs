using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DM_CS.PictureCore;

namespace DM_CS.GUI
{
    public partial class MainWindow : Window
    {
        public int Merger(List<string[]> merger_lists)
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

        public List<List<string>> ListXList(List<List<string>> allMergeList, string[] addList, bool MustNeed)
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
    }
}
