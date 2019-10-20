using MercedesBenz.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Configuration;

namespace MercedesBenz.Infrastructure
{
    /// <summary>
    /// 系统配置
    /// </summary>
    public class SystemConfiguration
    {
        private static List<ServiceModel> Services { get; set; }

        public static string Description { get { return ConfigurationManager.AppSettings["Description"]; } }

        public static string DisplayName { get { return ConfigurationManager.AppSettings["DisplayName"]; } }

        public static string ServiceName { get { return ConfigurationManager.AppSettings["ServiceName"]; } }

        public static string sourceCode { get { return ConfigurationManager.AppSettings["SourceCode"]; } }

        public static string arecore { get { return ConfigurationManager.AppSettings["WCSarecore"]; } }

        public static int DoorON { get { return ConfigurationManager.AppSettings["DoorON"].ToInt(); } }

        public static bool IsRunDoor { get { return ConfigurationManager.AppSettings["IsRunDoor"].ToBool(); } }

        public static int InDoorON { get { return OperateIniTool.OperateIniRead("Service", "InDoorNO").ToInt(); } }

        public static int OutDoorON { get { return OperateIniTool.OperateIniRead("Service", "OutDoorNO").ToInt(); } }


        /// <summary>
        /// SQLDB连接字符
        /// </summary>
        public static string ConfigStringDB = ConfigurationManager.ConnectionStrings["DateBaseText"].ConnectionString;


        /// <summary>
        /// SQLDB连接字符
        /// </summary>
        public static string ConfigStringWebDB = ConfigurationManager.ConnectionStrings["DateBaseWebText"].ConnectionString;


        /// <summary>
        /// agv总数
        /// </summary>
        public static List<int> agvList
        {
            get
            {
                var agvCount = ConfigurationManager.AppSettings["agvCount"];
                return !string.IsNullOrWhiteSpace(agvCount) ? agvCount.Split(',').Select(p => p.ToInt()).ToList() : new List<int>();
            }
        }

        /// <summary>
        /// 获取Service配置
        /// </summary>
        /// <returns></returns>
        public static List<ServiceModel> Servicecfig()
        {
            if (Services == null)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "ServiceConfig.json";
                if (File.Exists(path))
                {
                    string JsonText = File.ReadAllText(path);
                    if (JsonText != "")
                        Services = JsonConvert.DeserializeObject<List<ServiceModel>>(JsonText);
                }
            }
            return Services;
        }

        /// <summary>
        /// 获取远程主机配置
        /// </summary>
        /// <returns></returns>
        public static ServiceModel Distance_serve(IPType type)
        {
            List<ServiceModel> services = Servicecfig();
            if (services != null && services.Count() > 0)
            {
                return services.FirstOrDefault(p => p.type == type);
            }
            else
            {
                throw new Exception("配置读取失败！");
            }
        }

        /// <summary>
        /// 获取Service 配置参数
        /// </summary>
        /// <returns></returns>
        public static ServiceModel This_serve()
        {
            List<ServiceModel> services = Servicecfig();
            if (services != null && services.Count() > 0)
            {
                return Servicecfig().FirstOrDefault(p => p.type == IPType.server);
            }
            else
            {
                return null;
            }
        }

        public static List<ServiceModel> CarService()
        {
            List<ServiceModel> services = Servicecfig();
            if (services != null && services.Count() > 0)
            {
                return Servicecfig().Where(p => p.CarType == "agv").ToList();
            }
            else
            {
                return null;
            }
        }


        #region 参数配置

        //public static string OrderTypeNDC { get { return "NDC"; } }

        //public static string OrderTypeAGV { get { return "AGV"; } }

        //public static string OrderTaskIn { get { return "In"; } }

        //public static string OrderTaskOut { get { return "Out"; } }

        #endregion

    }
}
