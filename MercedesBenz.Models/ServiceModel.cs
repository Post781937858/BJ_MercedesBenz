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
    }


    public enum IPType
    {
        agv = 0,

        ndc = 1,

        wcs = 2,

        server=3
    }
}
