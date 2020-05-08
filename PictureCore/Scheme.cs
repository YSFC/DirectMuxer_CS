using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace DM_CS.PictureCore
{
    public static class Scheme
    {
        public static string JoinScheme = "_";
        public static int OffsetMode = 0;
        public static string OutputDir = ".";
        public static string ColorKey = "#000000";
        public static int OutFormat = 0;
        public static int MaxThread = 0;
        public static bool PAChecked = true;

        public static string[] FormatList = new string[] { ".bmp", ".png", ".jpg" };
        static Scheme()
        {
            Refresh();
        }

        public static void Refresh()
        {
            var config_manager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            JoinScheme = GetKey("JoinScheme", config_manager, "_");
            OffsetMode = int.Parse(GetKey("OffsetMode", config_manager, "0"));
            OutputDir = GetKey("OutputDir", config_manager, ".");
            ColorKey = GetKey("ColorKey", config_manager, "#000000");
            OutFormat = int.Parse(GetKey("OutFormat", config_manager, "0"));
            MaxThread = int.Parse(GetKey("MaxThread", config_manager, "1"));
        }

        public static void Save()
        {
            var config_manager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            AddKey("JoinScheme", ref config_manager, JoinScheme);
            AddKey("OffsetMode", ref config_manager, OffsetMode.ToString());
            AddKey("OutputDir", ref config_manager, OutputDir);
            AddKey("ColorKey", ref config_manager, ColorKey);
            AddKey("OutFormat", ref config_manager, OutFormat.ToString());
            AddKey("MaxThread", ref config_manager, MaxThread.ToString());
            config_manager.Save();
        }

        /// <summary>
        /// 得到设置
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="config_manager"></param>
        /// <param name="def">默认值</param>
        /// <returns></returns>
        private static string GetKey(string key, Configuration config_manager,string def)
        {
            try
            {
                if (config_manager.AppSettings.Settings.AllKeys.Contains(key))
                {
                    return config_manager.AppSettings.Settings[key].Value;
                }
                else
                {
                    return def;
                }
            }
            catch(Exception X)
            {
                return null;
            }
        }

        private static void AddKey(string key, ref Configuration config_manager, string def)
        {
            try
            {
                if (config_manager.AppSettings.Settings.AllKeys.Contains(key))
                {
                    config_manager.AppSettings.Settings[key].Value = def;
                }
                else
                {
                    config_manager.AppSettings.Settings.Add(key,def);
                }
            }
            catch (Exception X)
            {
                return;
            }
        }

    }

    public static class MergerStyle
    {
        /// <summary>
        /// Alpha
        /// </summary>
        public static int Alpha { get { return 1; } }
        public static int Color { get { return 2; } }
        public static int Override { get { return 3; } }
    }
}
