using System;

namespace MercedesBenz.Models
{

    /// <summary>
    /// 任务状态回报
    /// </summary>
    public class TaskStatic : BaseMessage 
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public string ordermark { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public int ordertype { get; set; }


        /// <summary>
        /// 执行任务agv
        /// </summary>
        public int agvnumber { get; set; }

        /// <summary>
        /// 任务阶段
        /// 1 任务开始
        /// 3 为到达取货点
        /// 4 取货完毕
        /// 5 到达放货点
        /// 6 放货完毕
        /// 7 任务完成
        /// 8 请求删除任务
        /// 9 任务删除确认
        /// </summary>
        public int state { get; set; }
    }
}
