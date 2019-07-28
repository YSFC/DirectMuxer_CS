using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;
using ImageMagick;
using System.Configuration;
using System.Text.RegularExpressions;
using DM_CS.PictureCore;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //BitmapImage x = new BitmapImage();
            //var F = new Uri("c1.bmp", UriKind.Relative);

            //var der = BitmapDecoder.Create(F,BitmapCreateOptions.PreservePixelFormat|BitmapCreateOptions.DelayCreation,BitmapCacheOption.Default);

            //Console.WriteLine(der.Frames[0].PixelHeight);

            //BmpBitmapEncoder SB = new BmpBitmapEncoder();
            //SB.Frames.Add(der.Frames[0]);
            //var o5 = File.Open("5.bmp", FileMode.Create);
            //SB.Save(o5);
            //o5.Close();

            //var P1 = new ImageOpen("c1.bmp");
            //var P2 = new ImageOpen("c2.bmp");
            //ImageOpen outC = null;
            //if (P2.IsPicture)
            //{
            //    outC = PicMerger.Alpha(P1, P2);
            //}
            //var q = outC.Pic.ToBitmap();

            //var cfm = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //cfm.AppSettings.Settings["JoinScheme"].Value = "+";
            //cfm.Save();

            //P2.Dispose();
            //Console.WriteLine(MagickColors.Bisque);

            var mainRe = new Regex("(abc)(2?)(233)");
            var tempMatch = mainRe.Match(args[0]);
            Console.WriteLine(tempMatch.Success);
            var matchKey = "";
            for (var j = 1; j < tempMatch.Groups.Count; j++)
            {
                matchKey += tempMatch.Groups[j] + "|";
            }
            Console.WriteLine(matchKey);
        }
    }
}
