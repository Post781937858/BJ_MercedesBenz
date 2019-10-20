using System;

namespace MercedesBenz.Models
{
    public class ServiceModel
    {
        public string IP { get; set; }

        public int Port { get; set; }

        public int ServerMax { get; set; }

        public IPType type { get; set; }

        public bool ON { get; set; }

        public string CarType { get; set; }

        public int CarNumber { get; set; }
    }


    public enum IPType
    {
        ndc = 0,

        wcs = 1,

        server = 2,

        agvserver = 3,



        OutSite = 4,

        InSite = 5,



        #region AGV

        weight = 6,

        light = 7,


        #endregion



        #region 空开

        SwitchServer = 11,

        AL1_1,

        AL1_2,

        AL2_1,

        AL2_2,

        AL3_1,

        AL3_2,
        #endregion

        #region 温湿度传感器

        Temperature1

        #endregion
    }
}
