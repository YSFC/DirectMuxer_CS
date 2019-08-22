using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private void ChangePreviewSize(object sender, RoutedEventArgs e)
        {
            if (imageControl.Source == null)
                return;
            else
            {
                var tempImageSource = GlobalScheme.PreviewBS;
                ScaleTransform st = new ScaleTransform();
                st.ScaleX = float.Parse(scale.Text)/100;
                st.ScaleY = st.ScaleX;
                var thumbPic = new TransformedBitmap(tempImageSource, st);
                //var newImageSource = new FormatConvertedBitmap(thumbPic, PixelFormats.Bgra32, null, 0);

                imageControl.Source = thumbPic;
            }
        }

        private void PreviewLoad(object sender, RoutedEventArgs e)
        {
            imageControl.Source = GlobalScheme.PreviewBS;
        }

        public void RefreshImage()
        {
            var tempImageSource = GlobalScheme.PreviewBS;
            ScaleTransform st = new ScaleTransform();
            st.ScaleX = (preview.ActualWidth - 20) / (double)tempImageSource.PixelWidth;
            st.ScaleY = st.ScaleX;
            scale.Text = ((int)(st.ScaleX * 100)).ToString();
            var thumbPic = new TransformedBitmap(tempImageSource, st);
            //var newImageSource = new FormatConvertedBitmap(thumbPic, PixelFormats.Bgra32, null, 0);
            
            imageControl.Source = thumbPic;
        }

        private void ChangeWinSize(object sender, SizeChangedEventArgs e)
        {
            if(imageControl.Source != null)
                RefreshImage();
        }
    }
}
