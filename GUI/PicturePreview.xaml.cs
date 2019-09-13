using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DM_CS.GUI
{
	/// <summary>
	/// PicturePreview.xaml 的交互逻辑
	/// </summary>
	public partial class PicturePreview : Window
	{
		public PicturePreview()
		{
			InitializeComponent();
			imageControl.Source = GlobalScheme.PreviewBS;
		}

		private void Image_Load(object sender, RoutedEventArgs e)
		{
			imageControl.Width = preview.Width - 20;
			imageControl.Height = preview.Height;
		}

		private void ClosePicturePreview(object sender, EventArgs e)
		{

		}

		/// <summary>
		/// 根据输入的百分比改变预览图大小。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ChangePreviewSize(object sender, RoutedEventArgs e)
		{
			if (imageControl.Source == null)
				return;
			else
			{
				var tempImageSource = GlobalScheme.PreviewBS;
				ScaleTransform st = new ScaleTransform();
				st.ScaleX = float.Parse(scale.Text) / 100;
				st.ScaleY = st.ScaleX;
				var thumbPic = new TransformedBitmap(tempImageSource, st);
				//var newImageSource = new FormatConvertedBitmap(thumbPic, PixelFormats.Bgra32, null, 0);

				imageControl.Source = thumbPic;
			}
		}

		/// <summary>
		/// 预览图实质是从GlobalScheme那边调取的。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PreviewLoad(object sender, RoutedEventArgs e)
		{
			imageControl.Source = GlobalScheme.PreviewBS;
		}

		/// <summary>
		/// 刷新图片，主要是选择图片改变后或窗口大小变动时使用。
		/// </summary>
		public void RefreshImage()
		{
			var tempImageSource = GlobalScheme.PreviewBS;
			ScaleTransform st = new ScaleTransform();
			st.ScaleX = (preview.ActualWidth - 20) / (double)tempImageSource.PixelWidth;
			if (st.ScaleX < 0.1)
			{
				st.ScaleX = 0.1;
			}
			else if (st.ScaleX > 6)
			{
				st.ScaleX = 6;
			}
			st.ScaleY = st.ScaleX;
			scale.Text = ((int)(st.ScaleX * 100)).ToString();
			var thumbPic = new TransformedBitmap(tempImageSource, st);
			//var newImageSource = new FormatConvertedBitmap(thumbPic, PixelFormats.Bgra32, null, 0);

			imageControl.Source = thumbPic;
		}

		/// <summary>
		/// 窗口大小改变时调用。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ChangeWinSize(object sender, SizeChangedEventArgs e)
		{
			if (imageControl.Source != null)
				RefreshImage();
		}


		/// <summary>
		/// 输入字符限制为数字
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void inputScale(object sender, KeyEventArgs e)
		{
			bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;//判断shifu键是否按下
			if (shiftKey == true) //当按下shift
			{
				e.Handled = true;//不可输入
			}
			else //未按shift
			{
				if (!((e.Key >= Key.D0 && e.Key <= Key.D9)
					|| (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
					|| e.Key == Key.Delete
					|| e.Key == Key.Back
					|| e.Key == Key.Tab
					|| e.Key == Key.Enter
					|| e.Key == Key.Decimal))
				{
					e.Handled = true;//不可输入
				}
				if (scale.Text.Contains(".") && e.Key == Key.Decimal)
				{
					e.Handled = true;//不可输入第二个.
				}
			}
		}

		/// <summary>
		/// 输入完成（失去键盘焦点）
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void inputScaleComplete(object sender, KeyboardFocusChangedEventArgs e)
		{
			try
			{
				var scaleGet = float.Parse(scale.Text);
				if (scaleGet < 10)
				{
					scale.Text = "10";
				}
				else if (scaleGet > 600)
				{
					scale.Text = "600";
				}
			}
			catch (Exception ex)
			{
				scale.Text = "100";
			}
		}
	}
}
