using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.Models
{
    public class Stationconfiguration
    {
        /// <summary>
        /// 站点GUID
        /// </summary>
        public string station_guid { get; set; }

        /// <summary>
        /// 站点编号
        /// </summary>
        public int station_number { get; set; }

        /// <summary>
        /// 对应agv站点
        /// </summary>
        public int station_agvSite { get; set; }

        /// <summary>
        /// 对应WCS站点
        /// </summary>
        public int station_wcsSite { get; set; }
    }
}
