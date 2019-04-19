using System;

namespace MercedesBenz.Models
{
    public class agvInfo
    {
        /// <summary>
        /// AGV编号
        /// </summary>
        public int agvNumber { get; set; }

        /// <summary>
        /// 是否运行
        /// </summary>
        public bool Run { get; set; }

        /// <summary>
        /// 报警码
        /// </summary>
        public string ErrorCore { get; set; }

        /// <summary>
        /// 报警
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// 是否自动
        /// </summary>
        public bool Selfmotion { get; set; }

        /// <summary>
        /// 方向   是否为前进
        /// </summary>
        public bool Advance { get; set; }

        /// <summary>
        /// 是否可以接受任务
        /// </summary>
        public bool TaskIn { get; set; }

        /// <summary>
        /// X轴数据
        /// </summary>
        public double location_X { get; set; }

        /// <summary>
        /// Y轴数据
        /// </summary>
        public double location_Y { get; set; }

        /// <summary>
        /// 电压
        /// </summary>
        public double voltage { get; set; }

        /// <summary>
        /// 速度
        /// </summary>
        public double speed { get; set; }

        /// <summary>
        /// 扫描区域
        /// </summary>
        public int PBS { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public int OrderStatic { get; set; }

        /// <summary>
        /// 任务出发地
        /// </summary>
        public double Start_location { get; set; }

        /// <summary>
        /// 任务目的地
        /// </summary>
        public double purpose_location { get; set; }

        /// <summary>
        /// 路径代码
        /// </summary>
        public int RounteCore { get; set; }

        /// <summary>
        /// 电机状态1  0.停止 1.正转 2.反转
        /// </summary>
        public int Machinery1Static { get; set; }

        /// <summary>
        /// 电机状态2 0.停止 1.正转 2.反转
        /// </summary>
        public int Machinery2Static { get; set; }

        /// <summary>
        /// 电机状态3 0.停止 1.正转 2.反转
        /// </summary>
        public int Machinery3Static { get; set; }

        /// <summary>
        /// 电机状态4 0.停止 1.正转 2.反转
        /// </summary>
        public int Machinery4Static { get; set; }
    }
}
