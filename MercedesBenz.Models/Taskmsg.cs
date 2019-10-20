using System;
using System.ComponentModel;

namespace MercedesBenz.Models
{
    /// <summary>
    /// 任务基类
    /// </summary>
    public class BaseTask : BaseMessage
    {

        public string sourceCode { get; set; }
        /// <summary>
        /// 库区编码
        /// </summary>
        public string areaCode { get; set; }

        /// <summary>
        /// WMSID
        /// </summary>
        public string wmsId { get; set; }

        /// <summary>
        /// 箱号Id
        /// </summary>
        public string boxId { get; set; }
    }

    /// <summary>
    /// 订单状态更新
    /// </summary>
    public class UpdateOrderTask : BaseTask
    {
        /// <summary>
        /// 订单状态
        /// </summary>
        public int state { get; set; }

        /// <summary>
        /// 位置信息
        /// </summary>
        public UpdateTaskPosition location { get; set; }
    }

    public class UpdateOrderTaskStatus : UpdateOrderTask
    {
        public string mes_messageName { get; set; }

        public string mes_areaCode { get; set; }

        public string mes_sourceCode { get; set; }

        public string mes_boxId { get; set; }

        public string mes_wmsId { get; set; }

        public long mes_utctime { get; set; }

        public int mes_Status { get; set; }

        public int mes_state { get; set; }

        public string mes_guid { get; set; }

        public string mes_dateTime { get; set; }

        public string mes_Data { get; set; }

        public int mes_ID { get; set; }
    }



    //订单状态更新位置
    public class UpdateTaskPosition
    {
        /// <summary>
        /// 取货层位
        /// </summary>
        public int s_level { get; set; }

        /// <summary>
        /// 取货位置
        /// </summary>
        public string s_location { get; set; }

        /// <summary>
        /// 放货层位
        /// </summary>
        public int e_level { get; set; }

        /// <summary>
        /// 放货位置
        /// </summary>
        public string e_location { get; set; }

        /// <summary>
        /// 占用重新分配层位
        /// </summary>
        public int r_level { get; set; }

        /// <summary>
        /// 占用重新分配位置
        /// </summary>
        public string r_location { get; set; }
    }


    public class WCSTask : BaseTask
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int priority { get; set; }
    }

    /// <summary>
    ///叉车出库任务消息
    /// </summary>
    public class StockOutTaskmsg : WCSTask
    {

        /// <summary>
        /// 出库拣选台
        /// </summary>
        public string d_station { get; set; }

        /// <summary>
        /// 出库库位
        /// </summary>
        public string location { get; set; }
    }

    /// <summary>
    /// 叉车入库任务
    /// </summary>
    public class boxAnnounceTaskmsg : WCSTask
    {
        /// <summary>
        /// 取货站台编号
        /// </summary>
        public string s_station { get; set; }

        /// <summary>
        /// 放货站台编号
        /// </summary>
        public int d_station { get; set; }

        /// <summary>
        /// 放货位置
        /// </summary>
        public string location { get; set; }

    }

    /// <summary>
    /// 叉车库内搬运
    /// </summary>
    public class movePositionTaskmsg : WCSTask
    {
        /// <summary>
        /// 源库位层位
        /// </summary>
        public int s_level { get; set; }

        /// <summary>
        /// 源库位编码
        /// </summary>
        public string s_location { get; set; }

        /// <summary>
        /// 目标库位层
        /// </summary>
        public int d_level { get; set; }

        /// <summary>
        /// 目标库位编码
        /// </summary>
        public string d_location { get; set; }
    }


    /// <summary>
    /// 站台搬运
    /// </summary>
    public class carryPlatfromTaskmsg : WCSTask
    {
        /// <summary>
        /// 当前站台层位
        /// </summary>
        public int s_level { get; set; }

        /// <summary>
        /// 当前站台
        /// </summary>
        public string s_location { get; set; }

        /// <summary>
        /// 当前站台位置
        /// </summary>
        public string s_station { get; set; }

        /// <summary>
        /// 目标站台层位
        /// </summary>
        public int d_level { get; set; }

        /// <summary>
        /// 目标站台
        /// </summary>
        public string d_location { get; set; }

        /// <summary>
        /// 目标站台位置
        /// </summary>
        public string d_station { get; set; }

    }




    /// <summary>
    /// 任务订单
    /// </summary>
    public class OrderTask : INotifyPropertyChanged
    {
        /// <summary>
        /// 是否选中
        /// </summary>
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                GetChanged("IsSelected");
            }
        }

        /// <summary>
        /// Guid
        /// </summary>
        public string order_guid { get; set; }

        /// <summary>
        /// WMSID
        /// </summary>
        public string order_wmsId { get; set; }


        /// <summary>
        /// 订单编号
        /// </summary>
        public string order_ordernumber { get; set; }

        /// <summary>
        /// 箱号ID
        /// </summary>
        public string order_boxId { get; set; }

        /// <summary>
        /// 库区
        /// </summary>
        public string order_arecore { get; set; }


        public string order_sourceCode { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        private int Order_status;
        public int order_status
        {
            get { return Order_status; }
            set
            {
                Order_status = value;
                GetChanged("order_status");
            }
        }

        /// <summary>
        /// 订单类型
        /// </summary>
        public OrderTaskType order_type { get; set; }

        /// <summary>
        /// 取货点
        /// </summary>
        private string Order_getLocation;
        public string order_getLocation
        {
            get { return Order_getLocation; }
            set
            {
                Order_getLocation = value;
                GetChanged("order_getLocation");
            }
        }

        /// <summary>
        /// 放货点
        /// </summary>
        private string Order_putLocation;
        public string order_putLocation
        {
            get { return Order_putLocation; }
            set
            {
                Order_putLocation = value;
                GetChanged("order_putLocation");
            }
        }

        /// <summary>
        /// 优先级
        /// </summary>
        public int order_priority { get; set; }

        /// <summary>
        /// 时间UTC
        /// </summary>
        private long Order_utctime;
        public long order_utctime
        {
            get { return Order_utctime; }
            set
            {
                Order_utctime = value;
                GetChanged("order_utctime");
            }
        }

        /// <summary>
        /// 时间
        /// </summary>
        private string Order_time;
        public string order_time
        {
            get { return Order_time; }
            set
            {
                Order_time = value;
                GetChanged("order_time");
            }
        }

        /// <summary>
        /// 车号
        /// </summary>
        private int Order_carno;
        public int order_carno
        {
            get { return Order_carno; }
            set
            {
                Order_carno = value;
                GetChanged("order_carno");
            }
        }

        /// <summary>
        /// 阶段
        /// </summary>
        private int Order_magic;
        public int order_magic
        {
            get { return Order_magic; }
            set
            {
                Order_magic = value;
                GetChanged("order_magic");
            }
        }

        /// <summary>
        ///下发订单号（上位机订单号）
        /// </summary>
        private int Order_ikey;
        public int order_ikey
        {
            get { return Order_ikey; }
            set
            {
                Order_ikey = value;
                GetChanged("order_ikey");
            }
        }

        /// <summary>
        /// NDC订单号 
        /// </summary>
        private int Order_index;
        public int order_index
        {
            get { return Order_index; }
            set
            {
                Order_index = value;
                GetChanged("order_index");
            }
        }

        /// <summary>
        /// 类型
        /// </summary>
        public OrderType order_genre { get; set; }

        /// <summary>
        /// 取货站台
        /// </summary>
        private string Order_getSite;
        public string order_getSite
        {
            get { return Order_getSite; }
            set
            {
                Order_getSite = value;
                GetChanged("order_getSite");
            }
        }

        /// <summary>
        /// 放货站台
        /// </summary>
        private string Order_putSite;
        public string order_putSite
        {
            get { return Order_putSite; }
            set
            {
                Order_putSite = value;
                GetChanged("order_putSite");
            }
        }

        /// <summary>
        /// 任务状态
        /// </summary>
        public int order_cancel { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 属性更改通知客户端事件
        /// </summary>
        private void GetChanged(string Name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }
    }


    public enum OrderType
    {
        NDC,

        AGV
    }

    public enum OrderTaskType
    {
        In, //入库任务
        Out, //出口任务
        Interior, //库内搬运
        Platform, //站台搬运

        OriginTask //原点任务
    }
}
