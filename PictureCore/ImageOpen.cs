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
	public class ImageOpen : IDisposable
	{
		public MagickImage m_pic = null;
		private int m_height = 0;
		private int m_width = 0;
		private string m_filename = null;
		private string m_format = null;
		private int[] m_offset = { 0, 0 };
		public int Height { get { return m_height; } }
		public int Width { get { return m_width; } }
		public int[] OffsetXY { get { return m_offset; } }
		public string FileName { get { return m_filename; } }
		public string Format { get { return m_format; } }
		public MagickImage Pic { get { return m_pic; } }
		public bool IsPicture { get { return m_format != null; } }
		public long PicMemorySize { get { return GetPicMemorySize(); } }
		/// <summary>
		/// 合成次数统计
		/// </summary>
		public int MergedCount { get; set; }
		/// <summary>
		/// 是否保存过标识
		/// </summary>
		public int SavedSign { get; set; }

		private void ImageInfoSet(FileStream file)
		{
			try
			{
				var m_pic_temp = new MagickImage(file);

				//有些图片，全黑（只有一种颜色），明明是32位的全透明图片，就是被识别成24位全黑不透明图片，于是有了这个。
				//Windows库读取没有问题，主要是IM库，然后再看好像不止IM，XN软件也这么干的。没办法才这么整一下。
				if (m_pic_temp.TotalColors == 1 && m_pic_temp.Format == MagickFormat.Bmp3)
				{
					try
					{
						file.Seek(10, SeekOrigin.Begin);
						var tempIntBytes = new byte[4];
						file.Read(tempIntBytes, 0, 4);
						var tempInt = BitConverter.ToUInt32(tempIntBytes, 0);
						file.Seek(tempInt, SeekOrigin.Begin);
						var dataLen = m_pic_temp.Width * m_pic_temp.Height * 4;
						var data = new byte[dataLen];
						file.Read(data, 0, dataLen);
						var prs = new PixelReadSettings(m_pic_temp.Width, m_pic_temp.Height, StorageType.Char, PixelMapping.RGBA);
						m_pic_temp = new MagickImage(data, prs);
					}
					catch(Exception ex)
					{
						//emmmmmm确实不知道怎么处理这个情况了
					}
				}
				m_pic = (MagickImage)m_pic_temp.Clone();
				m_pic_temp.Dispose();
			}
			catch (Exception X)
			{
				m_format = null;
				return;
			}
			m_height = m_pic.Height;
			m_width = m_pic.Width;
			m_format = m_pic.Format.ToString().ToUpper();
		}
		public ImageOpen(FileStream file)
		{
			m_filename = file.Name;
			ImageInfoSet(file);
			FindPosByFilename();
		}

		public ImageOpen(string filename)
		{
			var file = File.Open(filename, FileMode.Open);
			m_filename = file.Name;
			ImageInfoSet(file);
			file.Close();
			FindPosByFilename();
		}

		/// <summary>
		/// 合成图片专用
		/// </summary>
		/// <returns></returns>
		public ImageOpen(MagickImage im_pic, int[] offset)
		{
			m_height = im_pic.Height;
			m_width = im_pic.Width;
			m_format = im_pic.Format.ToString().ToUpper();
			m_filename = null;
			m_pic = im_pic;
			m_offset[0] = offset[0];
			m_offset[1] = offset[1];
			MergedCount = 0;
			SavedSign = 0;
		}

		public ImageOpen(MagickImage im_pic, int[] offset, string out_filename) : this(im_pic, offset)
		{
			m_filename = out_filename;
		}

		/// <summary>
		/// 得到复制的MagickImage图像
		/// </summary>
		/// <returns></returns>
		public MagickImage CopyPic()
		{
			return (MagickImage)m_pic.Clone();
		}

		public ImageOpen Clone()
		{
			var returnIO = new ImageOpen(this.CopyPic(), this.OffsetXY, this.m_filename);
			return (returnIO);
		}

		/// <summary>
		/// 这个主要是计算图片如果以Bitmap形式在内存中的占用空间，但并不准确
		/// </summary>
		/// <returns></returns>
		private long GetPicMemorySize()
		{
			if (!IsPicture)
				return 0;

			return Height * Width * m_pic.Depth * m_pic.ChannelCount / 8;

		}

		/// <summary>
		/// 坐标搜寻（文件名内含）,之后去除坐标（如果使用了坐标）
		/// 具体设置应在主界面设置内调整，此外还有TGA这种可内挂坐标的格式，另外考虑。
		/// </summary>
		private void FindPosByFilename()
		{
			Regex re;
			string replaceString = null;
			switch (Scheme.OffsetMode)
			{
				case 0:
					re = new Regex(@"x(-?\d+)y(-?\d+)", RegexOptions.IgnoreCase);
					replaceString = "x{0}y{1}";
					break;
				case 1:
					re = new Regex(@"_pos_(-?\d+)_(-?\d+)", RegexOptions.IgnoreCase);
					replaceString = "_pos_{0}_{1}";
					break;
				default:
					m_offset[0] = 0;
					m_offset[1] = 0;
					return;
			}

			//如果未能匹配都为0好了
			var ma = re.Match(FileName);
			if (ma.Success)
			{
				m_offset[0] = int.Parse(ma.Groups[1].Value);
				m_offset[1] = int.Parse(ma.Groups[2].Value);
				if (replaceString != null)
				{
					var ttt = string.Format(replaceString, m_offset[0], m_offset[1]);
					this.m_filename = this.m_filename.Replace(ttt, "");

				}
			}
			else
			{
				m_offset[0] = 0;
				m_offset[1] = 0;
			}
		}

		public void Save()
		{
			var outFormatExt = Scheme.FormatList[Scheme.OutFormat];
			if (outFormatExt != ".bmp")
			{
				m_pic.Write(Path.ChangeExtension(FileName, outFormatExt));
			}
			else
			{
				var outBMP = m_pic.ToBitmap();
				var outFileName = Path.ChangeExtension(FileName, outFormatExt);
				outBMP.Save(outFileName, ImageFormat.Bmp);
				outBMP.Dispose();
			}
			this.SavedSign = 1;
		}

		public BitmapSource GetBitmap()
		{
			var outBit = m_pic.ToBitmapSource();
			return outBit;
		}

		#region Dispose
		bool m_disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Close()
		{
			this.Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!m_disposed)
			{
				if (disposing && m_pic != null)
				{
					m_pic.Dispose();
					m_pic = null;
				}
				m_disposed = true;
			}
		}
		#endregion

	}

}
