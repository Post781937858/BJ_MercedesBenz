using System;
using System.ComponentModel;

namespace MercedesBenz.Models
{
    public class agvInfo : INotifyPropertyChanged
    {
        public agvInfo()
        {
            this.UpdateDateTime = 0;
            this.UpdateStopDateTime = 0;
            this.updateDateInfoTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public IPType CarIptype { get; set; }

        /// <summary>
        /// AGV编号
        /// </summary>
        private int AgvNumber;
        public int agvNumber
        {
            get { return AgvNumber; }
            set
            {
                AgvNumber = value;
                GetChanged("agvNumber");
            }
        }

        /// <summary>
        /// 电压
        /// </summary>
        private double Voltage;
        public double voltage
        {
            get { return Voltage; }
            set
            {
                Voltage = value;
                GetChanged("voltage");
            }
        }

        /// <summary>
        /// 导航状态
        /// </summary>
        public navigation NavigationStatus { get; set; }


        /// <summary>
        /// 导航状态
        /// </summary>
        private string NavigationText;
        public string navigationText
        {
            get { return NavigationText; }
            set
            {
                NavigationText = value;
                GetChanged("navigationText");
            }
        }

        /// <summary>
        /// 当前站点
        /// </summary>
        private int thisStation;
        public int ThisStation
        {
            get { return thisStation; }
            set
            {
                thisStation = value;
                GetChanged("ThisStation");
            }
        }

        /// <summary>
        /// 目标站点
        /// </summary>
        private int targetStation;
        public int TargetStation
        {
            get { return targetStation; }
            set
            {
                targetStation = value;
                GetChanged("TargetStation");
            }
        }

        /// <summary>
        /// 上/下料完成状态
        /// </summary>
        public TaskStatus taskStatus { get; set; }

        /// <summary>
        /// 上下料完成状态
        /// </summary>
        private string TaskStatusText;
        public string taskStatusText
        {
            get { return TaskStatusText; }
            set
            {
                TaskStatusText = value;
                GetChanged("taskStatusText");
            }
        }

        public string UpdownStatus()
        {
            string str = "";
            switch (taskStatus)
            {
                case TaskStatus.UPmaterial:
                    str = "上料完成";
                    break;
                case TaskStatus.Downmaterial:
                    str = "下料完成";
                    break;
                default:
                    str = "未知";
                    break;
            }
            return str;
        }

        /// <summary>
        /// 滚筒状态
        /// </summary>
        private int RollStatus;
        public int rollStatus
        {
            get { return RollStatus; }
            set
            {
                RollStatus = value;
                GetChanged("rollStatus");
            }
        }

        /// <summary>
        /// 错误码
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 当前X轴
        /// </summary>
        private float Location_X;
        public float location_X
        {
            get { return Location_X; }
            set
            {
                Location_X = value;
                GetChanged("location_X");
            }
        }

        /// <summary>
        /// 当前X轴
        /// </summary>
        private float Location_Y;
        public float location_Y
        {
            get { return Location_Y; }
            set
            {
                Location_Y = value;
                GetChanged("location_Y");
            }
        }

        /// <summary>
        /// 状态更新时间UTC
        /// </summary>
        public long UpdateDateTime { get; set; }

        /// <summary>
        /// 状态更新时间
        /// </summary>
        private string UpdateDateInfoTime;
        public string updateDateInfoTime
        {
            get { return UpdateDateInfoTime; }
            set
            {
                UpdateDateInfoTime = value;
                GetChanged("updateDateInfoTime");
            }
        }

        public string navigationStr()
        {
            string str = "";
            switch (NavigationStatus)
            {
                case navigation.not:
                    str = "无状态";
                    break;
                case navigation.wait:
                    str = "等待执行导航";
                    break;
                case navigation.This:
                    str = "正在执行导航";
                    break;
                case navigation.suspend:
                    str = "暂停导航";
                    break;
                case navigation.arrive:
                    str = "到达";
                    break;
                case navigation.Error:
                    str = "导航失败";
                    break;
                case navigation.Cancel:
                    str = "取消";
                    break;
                case navigation.Timeout:
                    str = "超时";
                    break;
                default:
                    str = "未知";
                    break;
            }
            return str;
        }

        /// <summary>
        /// 站点停止时间
        /// </summary>
        public long UpdateStopDateTime { get; set; }

        /// <summary>
        /// 停止状态
        /// </summary>
        public int StopRunStatus { get; set; }

        public int agvRunStop { get; set; }

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

    /// <summary>
    /// 导航状态
    /// </summary>
    public enum navigation {

        /// <summary>
        /// 无状态
        /// </summary>
        not,

        /// <summary>
        /// 等待执行导航
        /// </summary>
        wait,

        /// <summary>
        /// 正在执行导航
        /// </summary>
        This,

        /// <summary>
        /// 暂停导航
        /// </summary>
        suspend,

        /// <summary>
        /// 到达
        /// </summary>
        arrive,

        /// <summary>
        /// 导航失败
        /// </summary>
        Error,

        /// <summary>
        /// 取消
        /// </summary>
        Cancel,

        /// <summary>
        /// 超时
        /// </summary>
        Timeout
    }


    public enum TaskStatus
    {

        /// <summary>
        /// 上料完成
        /// </summary>
        UPmaterial = 1,

        /// <summary>
        /// 下料完成
        /// </summary>
        Downmaterial = 2
    }
}
