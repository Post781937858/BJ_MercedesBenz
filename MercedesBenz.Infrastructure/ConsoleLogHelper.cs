using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.Infrastructure
{
    public class ConsoleLogHelper
    {
        /// <summary>
        /// 写入SucceedLog
        /// </summary>
        /// <param name="message"></param>
        public static void WriteSucceedLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssss")}：{message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// 写入ErrorLog
        /// </summary>
        /// <param name="message"></param>
        public static void WriteErrorLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssss")}：{message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// 写入InfoLog
        /// </summary>
        /// <param name="message"></param>
        public static void WriteInfoLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssss")}：{message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
