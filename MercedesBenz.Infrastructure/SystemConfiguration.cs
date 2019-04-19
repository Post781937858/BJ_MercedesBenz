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

        /// <summary>
        /// 获取Service配置
        /// </summary>
        /// <returns></returns>
        private static List<ServiceModel> Servicecfig()
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
        /// 获取所有远程主机配置
        /// </summary>
        /// <returns></returns>
        public static List<ServiceModel> Distance_serve()
        {
            List<ServiceModel> services = Servicecfig();
            if (services != null && services.Count() > 0)
            {
                return services.Where(p => p.type != IPType.server).ToList();
            }
            else
            {
                return new List<ServiceModel>();
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

        /// <summary>
        /// 是否开启server
        /// </summary>
        public static bool ONserver
        {
            get
            {
                return Servicecfig().FirstOrDefault(p=>p.type==IPType.server).ON;
            }
        }

        /// <summary>
        /// 是否开启agv
        /// </summary>
        public static bool OnAgv
        {
            get
            {
                List<ServiceModel> services = Servicecfig();
                return services != null && services.Count() > 0 ? services.FirstOrDefault(p => p.type == IPType.agv).ON : false;
            }
        }


        /// <summary>
        /// 是否开启Ndc
        /// </summary>
        public static bool OnNdc
        {
            get
            {
                List<ServiceModel> services = Servicecfig();
                return services != null && services.Count() > 0 ? services.FirstOrDefault(p => p.type == IPType.ndc).ON : false;
            }
        }
        /// <summary>
        /// 是否开启wcs
        /// </summary>
        public static bool OnWcs
        {
            get
            {
                List<ServiceModel> services = Servicecfig();
                return services != null && services.Count() > 0 ? services.FirstOrDefault(p => p.type == IPType.wcs).ON : false;
            }
        }








    }
}
