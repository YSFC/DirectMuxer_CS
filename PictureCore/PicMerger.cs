using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using ImageMagick;

namespace DM_CS.PictureCore
{
    public static class PicMergerCore
    {
        public static ImageOpen Alpha(ImageOpen Base, ImageOpen Diff)
        {
            return Alpha(Base, Diff, 0, 0);
        }

        /// <summary>
        /// alpha合成图像，返回的图像继承Base的坐标，并将文件名以连接符连接。
        /// </summary>
        /// <param name="Base">基本图片</param>
        /// <param name="Diff">差分图片</param>
        /// <param name="offset_x">X轴偏移</param>
        /// <param name="offset_y">Y轴偏移</param>
        /// <returns>输出的ImageOpen类图片</returns>
        public static ImageOpen Alpha(ImageOpen Base, ImageOpen Diff,int offset_x,int offset_y)
        {
            var out_pic = Base.CopyPic();
            out_pic.Composite(Diff.Pic, offset_x, offset_y, CompositeOperator.Over);
            var out_name = Path.Combine(Scheme.OutputDir, Path.GetFileNameWithoutExtension(Base.FileName));
            out_name += Scheme.JoinScheme + Path.GetFileName(Diff.FileName);
            return new ImageOpen(out_pic,Base.OffsetXY, out_name);
        }
        
        public static ImageOpen Color(ImageOpen Base, ImageOpen Diff)
        {
            return Color(Base, Diff, 0, 0);
        }

        /// <summary>
        /// Transparent Blitting
        /// </summary>
        /// <param name="Base"></param>
        /// <param name="Diff"></param>
        /// <returns></returns>
        public static ImageOpen Color(ImageOpen Base, ImageOpen Diff, int offset_x, int offset_y)
        {
            var ColorKey = Scheme.ColorKey;
            var out_pic = Base.CopyPic();
            MagickColor CK = new MagickColor(ColorKey);
            //因为会影响Diff原图，所以复制
            var Diff_byAlpha = Diff.CopyPic();
            //还有个ColorAlpha不知道怎么用，无效的感觉。
            Diff_byAlpha.Transparent(CK);

            out_pic.Composite(Diff_byAlpha, offset_x, offset_y, CompositeOperator.Over);
            Diff_byAlpha.Dispose();
            var out_name = Path.Combine(Scheme.OutputDir, Path.GetFileNameWithoutExtension(Base.FileName));
            out_name += Scheme.JoinScheme + Path.GetFileName(Diff.FileName);
            return new ImageOpen(out_pic, Base.OffsetXY, out_name);
        }

        public static ImageOpen Override(ImageOpen Base, ImageOpen Diff)
        {
            return Override(Base, Diff, 0, 0);
        }


        /// <summary>
        /// Opaque Override(但不含坐标预测)
        /// </summary>
        /// <param name="Base"></param>
        /// <param name="Diff"></param>
        /// <param name="offset_x"></param>
        /// <param name="offset_y"></param>
        /// <returns></returns>
        public static ImageOpen Override(ImageOpen Base, ImageOpen Diff, int offset_x, int offset_y)
        {
            var out_pic = Base.CopyPic();
            var Diff_byOpaque = Diff.CopyPic();
            Diff_byOpaque.HasAlpha = false;
            out_pic.Composite(Diff_byOpaque, offset_x, offset_y, CompositeOperator.Over);
            var out_name = Path.Combine(Scheme.OutputDir, Path.GetFileNameWithoutExtension(Base.FileName));
            out_name += Scheme.JoinScheme + Path.GetFileName(Diff.FileName);
            return new ImageOpen(out_pic, Base.OffsetXY, out_name);
        }
    }

    public static class PicMerger
    {
        public static ImageOpen Merger(ImageOpen basePic, ImageOpen diffPic,int merger_scheme, int offset_x, int offset_y)
        {
            ImageOpen outPic;

            switch (merger_scheme)
            {
                case 1:
                    outPic = PicMergerCore.Alpha(basePic, diffPic, offset_x, offset_y);
                    break;
                case 2:
                    outPic = PicMergerCore.Alpha(basePic, diffPic);
                    break;
                case 3:
                    outPic = PicMergerCore.Color(basePic, diffPic, offset_x, offset_y);
                    break;
                case 4:
                    outPic = PicMergerCore.Color(basePic, diffPic);
                    break;
                case 5:
                    outPic = PicMergerCore.Override(basePic, diffPic, offset_x, offset_y);
                    break;
                case 6:
                    outPic = PicMergerCore.Override(basePic, diffPic);
                    break;
                default:
                    outPic = null;
                    break;
            }

            return outPic;
        }

        public static ImageOpen MergerOfStrings(string[] merge_list)
        {
            var basePic = new ImageOpen(merge_list[0]);
            bool FirstSW = true;
            foreach(var i in merge_list)
            {
                //跳过第一张
                if (FirstSW)
                {
                    FirstSW = false;
                    continue;
                }
                var diffPic = new ImageOpen(i);
                basePic = PicMerger.Merger(basePic, diffPic, MergerStyle.Alpha,
                    diffPic.OffsetXY[0] - basePic.OffsetXY[0], diffPic.OffsetXY[1] - basePic.OffsetXY[1]);
                diffPic.Close();
            }

            return basePic;
        }
    }
}
