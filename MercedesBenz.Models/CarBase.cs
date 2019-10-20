using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.Models
{
    /// <summary>
    /// 状态更新消息基类
    /// </summary>
    public class MessageBase
    {
        /// <summary>
        /// 是否报警
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// 报警信息
        /// </summary>
        public string ErrorMes { get; set; }

        /// <summary>
        /// 是否运行
        /// </summary>
        public bool IsRun { get; set; }
    }


    /// <summary>
    /// 车辆基类
    /// </summary>
    public class CarBase : MessageBase
    {
        /// <summary>
        /// 车辆编号
        /// </summary>
        public int CarNumber { get; set; }

        /// <summary>
        /// 电压
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// 车辆状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 是否充电
        /// </summary>
        public string IsCharge { get; set; }
    }

    /// <summary>
    /// AGV状态信息
    /// </summary>
    public class agvinfo : CarBase
    {


    }

    /// <summary>
    /// 叉车AGV状态信息
    /// </summary>
    public class ForkCar : CarBase
    {


    }
    /// <summary>
    /// RGV状态信息
    /// </summary>
    public class RGVCar : CarBase
    {

    }

    /// <summary>
    /// 穿梭车状态
    /// </summary>
    public class ShuttleCar : CarBase
    {

    }

    /// <summary>
    /// 输送线状态
    /// </summary>
    public class DeliveryLine : MessageBase
    {



    }

    /// <summary>
    /// 看板仓库实体
    /// </summary>
    public class warehouseStatus
    {
        /// <summary>
        /// 仓库名称
        /// </summary>
        public string warehouseName { get; set; }

        /// <summary>
        /// 数据集
        /// </summary>
        public List<Data> warehouseData { get; set; }
    }


    public class Data
    {
        public int value { get; set; }

        public string name { get; set; }
    }
}
