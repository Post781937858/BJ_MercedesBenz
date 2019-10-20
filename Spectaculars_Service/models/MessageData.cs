using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BJ_MercedesBenz_Spectaculars
{
    /// <summary>
    /// 看板数据实体
    /// </summary>
    public class MessageUpdateData
    {
        /// <summary>
        /// AGV状态信息
        /// </summary>
        public List<agvinfo> agvInfoList { get; set; }

        /// <summary>
        /// 叉车状态信息
        /// </summary>
        public List<ForkCar> forkCarinfoList { get; set; }

        /// <summary>
        /// RGV状态信息
        /// </summary>
        public List<RGVCar> RGVCarinfoList { get; set; }

        /// <summary>
        /// 穿梭车状态信息
        /// </summary>
        public List<ShuttleCar> shuttleCarinfoList { get; set; }

        /// <summary>
        /// 输送线状态信息
        /// </summary>
        public DeliveryLine deliveryLineinfo { get; set; }

        /// <summary>
        /// 仓库集合
        /// </summary>
        public List<warehouseStatus> WarehouseStatusesList { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// 湿度
        /// </summary>
        public double Humidity { get; set; }
    }
}
