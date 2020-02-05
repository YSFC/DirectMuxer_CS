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
		/// <summary>
		/// alpha合成图像，返回的图像继承Base的坐标，并将文件名以连接符连接。
		/// </summary>
		/// <param name="Base">基本图片</param>
		/// <param name="Diff">差分图片</param>
		/// <param name="offset_x">X轴偏移</param>
		/// <param name="offset_y">Y轴偏移</param>
		/// <returns>输出的ImageOpen类图片</returns>
		public static ImageOpen Alpha(ImageOpen Base, ImageOpen Diff, int offset_x, int offset_y)
		{
			var out_pic = Base.CopyPic();
			out_pic.Composite(Diff.Pic, offset_x, offset_y, CompositeOperator.Over);
			var out_name = Path.Combine(Scheme.OutputDir, Path.GetFileNameWithoutExtension(Base.FileName));
			out_name += Scheme.JoinScheme + Path.GetFileName(Diff.FileName);
			return new ImageOpen(out_pic, Base.OffsetXY, out_name);
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

		/// <summary>
		/// 边缘扫描，类似原本DM的O模式，但并非相似度而是完全一致匹配。
		/// </summary>
		/// <param name="Base"></param>
		/// <param name="Diff"></param>
		/// <returns></returns>
		public static ImageOpen EgdeDetect(ImageOpen Base, ImageOpen Diff)
		{
			//首先要扫描diff图片的最外圈，24位图就边缘，32位图就得非透明的边缘。
			//半透明如何处理我得想想。
			var getEgdeList = RGBA_G2L(Diff.Pic);
			var getXY = BaseXY(Base.Pic, Diff.Pic, getEgdeList);
			var outputPic = Alpha(Base, Diff, getXY[0], getXY[1]);
			return (outputPic);
		}

		/// <summary>
		/// 从python移植过来的，边缘像素列表化，24或者32内部判断化。
		/// </summary>
		/// <param name=""></param>
		/// <returns></returns>
		public static List<int[]> RGBA_G2L(MagickImage PicOpen)
		{

			var picWidth = PicOpen.Width;
			var picHeight = PicOpen.Height;

			var eList = new List<int[]>();

			//根据条件设置像素跨度（y轴的）
			var RH = 0;
			if (picHeight > 1080)
			{
				RH = 4;
			}
			else if (picHeight > 720)
			{
				RH = 3;
			}
			else if (picHeight > 480)
			{
				RH = 2;
			}
			else
			{
				RH = 3;
			}

			var picPixelC = PicOpen.GetPixels();
			if (PicOpen.Format == MagickFormat.Rgba)
			{
				//具体处理大致是判断图像透明度，250以上算不透明区域
				//然后根据从透明区域扫描进去还是出来判断边缘坐标，并将符合条件的加进去。
				//主要是IM库的pixel不熟，可能还得测试一阵。
				for (var pixelY = 0; pixelY < picHeight; pixelY += RH)
				{
					var SW_e = 1;
					for (var pixelX = 0; pixelX < picWidth; pixelX++)
					{
						var pixel = picPixelC.GetPixel(pixelX, pixelY);

						if (SW_e == 1 && pixel[3] > 250)
						{
							SW_e = 0;
							eList.Add(new int[] { pixelX, pixelY });
							continue;
						}
						else if (SW_e == 0 && pixel[3] < 250)
						{
							eList.Add(new int[] { pixelX - 1, pixelY });
							break;
						}
					}
				}
			}
			else
			{
				//具体处理大致是边缘的坐标都加进去，恩。
				for (var pixelY = 0; pixelY < picHeight; pixelY += RH)
				{
					eList.Add(new int[] { 0, pixelY });
					eList.Add(new int[] { picWidth - 1, pixelY });
				}
			}
			return (eList);
		}


		/// <summary>
		/// python 移植加修改而成，考虑到这边性能，打算换种方式，但怎么处理还没确定。
		/// </summary>
		/// <param name="baseOpen"></param>
		/// <param name="faceOpen"></param>
		/// <param name="faceEList"></param>
		/// <returns></returns>
		public static int[] BaseXY(MagickImage baseOpen, MagickImage faceOpen, List<int[]> faceEList) {
			//根据RGB和坐标数据进行图片扫描对比，并传出符合要求的数据
			var baseWidth = baseOpen.Width;
			var baseHeight = baseOpen.Height;

			var faceWidth = faceOpen.Width;
			var faceHeight = faceOpen.Height;

			var basePixels = baseOpen.GetPixels();
			var facePixels = faceOpen.GetPixels();


			//原本是有个容错限制，这边怎么处理还没想好，先设置为0好了
			//主要是，python那边是可以稍微偏一点的，但这边，至少这个是按照完全一致设计的。
			var limitDiff = faceEList.Count / 1;
			limitDiff = 1;
			if (limitDiff == 0)
			{
				limitDiff = 1;
			}

			var MAX = new int[] { 0, 0 };
			var A_EC = 99999;


			for (var baseY = 0; baseY < baseHeight - faceHeight; baseY++) {

				for (var baseX = 0; baseX < baseWidth - faceWidth; baseX++)
				{
					var sameCount = 0;
					foreach (var info in faceEList)
					{
						var B_XY = new int[] { baseX + info[0], baseY + info[1] };
						
						//这边本来是python的RC，也就是容差进行一个容错，不过这边直接按完全一致考虑，所以修改了。
						//由于以后不知道怎么设计，姑且保留目前情况是冗余的部分。
						var bP = basePixels.GetPixel(B_XY[0], B_XY[1]);
						var fP = facePixels.GetPixel(info[0], info[1]);
						if (compareColor24(bP, fP))
						{
							sameCount += 1;
						}
						else
						{
							//如果匹配的是边缘，可能没法一致，但可以忽略这些不同
							if (B_XY.Contains(0) || B_XY[0] == baseX - 1 || B_XY[1] == baseY - 1)
							{
								sameCount += 1;
								continue;
							}
							break;
						}

						//if (faceEList.Count - sameCount > limitDiff)
						//{
							//break;
						//}
					}
					if (faceEList.Count != sameCount)
					{
						continue;
					}
					else
					{
						MAX[0] = baseX;
						MAX[1] = baseY;
						return (MAX);
					}
				}
			}
			return (MAX);
		}

		/// <summary>
		/// 比较A和B的RGB颜色是否相同。
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <returns></returns>
		private static bool compareColor24(Pixel A,Pixel B)
		{
			var t1 = A[0] == B[0];
			var t2 = A[1] == B[1];
			var t3 = A[2] == B[2];
			return (t1 && t2 && t3);
		}
	}

	public static class PicMerger
	{
		/// <summary>
		/// 这个可能放弃使用
		/// </summary>
		/// <param name="basePic"></param>
		/// <param name="diffPic"></param>
		/// <param name="merger_scheme"></param>
		/// <param name="offset_x"></param>
		/// <param name="offset_y"></param>
		/// <returns></returns>
		public static ImageOpen Merger(ImageOpen basePic, ImageOpen diffPic, int merger_scheme, int offset_x, int offset_y)
		{
			ImageOpen outPic;

			switch (merger_scheme)
			{
				case 1:
					outPic = PicMergerCore.Alpha(basePic, diffPic, offset_x, offset_y);
					break;
				case 2:
					outPic = PicMergerCore.Color(basePic, diffPic, offset_x, offset_y);
					break;
				case 3:
					outPic = PicMergerCore.Override(basePic, diffPic, offset_x, offset_y);
					break;
				default:
					outPic = null;
					break;
			}

			return outPic;
		}


		/// <summary>
		/// 输入两张图片，得到的是合成后的图片。
		/// 返回图片由clone生成，不影响源图。
		/// </summary>
		/// <param name="basePic"></param>
		/// <param name="diffPic"></param>
		/// <param name="merger_scheme"></param>
		/// <returns></returns>
		public static ImageOpen Merger(ImageOpen basePic, ImageOpen diffPic, int merger_scheme)
		{
			ImageOpen outPic;
			var offset_x = diffPic.OffsetXY[0] - basePic.OffsetXY[0];
			var offset_y = diffPic.OffsetXY[1] - basePic.OffsetXY[1];

			switch (merger_scheme)
			{
				case 1:
					outPic = PicMergerCore.Alpha(basePic, diffPic, offset_x, offset_y);
					break;
				case 2:
					outPic = PicMergerCore.Color(basePic, diffPic, offset_x, offset_y);
					break;
				case 3:
					outPic = PicMergerCore.Override(basePic, diffPic, offset_x, offset_y);
					break;
				case 4:
					outPic = PicMergerCore.EgdeDetect(basePic, diffPic);
					break;
				default:
					outPic = null;
					break;
			}
			outPic.MergedCount = basePic.MergedCount + 1;
			return outPic;
		}

		public static ImageOpen MergerOfStrings(string[] merge_list)
		{
			var basePic = new ImageOpen(merge_list[0]);
			bool FirstSW = true;
			foreach (var i in merge_list)
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
