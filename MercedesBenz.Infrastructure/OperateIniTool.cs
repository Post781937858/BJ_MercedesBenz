using OperateIni;
using System;
using System.IO;

namespace MercedesBenz.Infrastructure
{
    /// <summary>
    /// Ini帮助类
    /// </summary>
    public static class OperateIniTool
    {
        private static string path { get; set; } //路径
        public static bool Exist { get; private set; } //配置文件是否存在

        static OperateIniTool()
        {
            path = AppDomain.CurrentDomain.BaseDirectory + "\\setting.ini";
            Exist = File.Exists(path);
        }

        /// <summary>
        /// 读
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string OperateIniRead(string Section, string Key)
        {
            var result = IniFile.ReadIniData(Section, Key, "", path);
            return string.IsNullOrWhiteSpace(result) ? string.Empty : result;
        }

        /// <summary>
        /// 写
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        public static bool  OperateIniWrite(this object obj, string Section, string Key)
        {
            return IniFile.WriteIniData(Section, Key, obj.ToString(), path);
        }


    }
}