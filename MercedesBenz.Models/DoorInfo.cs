using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.Models
{
    public class DoorInfo : INotifyPropertyChanged
    {
        public DoorInfo()
        {
            this.updateDateInfoTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private int doorNumber;
        public int DoorNumber
        {
            get { return doorNumber; }
            set
            {
                doorNumber = value;
                GetChanged("DoorNumber");
            }
        }

        public DoorType doorType { get; set; }

        private string doorTypeTexe;
        public string DoorTypeTexe
        {
            get { return doorTypeTexe; }
            set
            {
                doorTypeTexe = value;
                GetChanged("DoorTypeTexe");
            }
        }

        private DoorStatus doorStatus;
        public DoorStatus DoorStatus
        {
            get { return doorStatus; }
            set
            {
                doorStatus = value;
                GetChanged("DoorStatus");
            }
        }


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

    public enum DoorStatus
    {
        /// <summary>
        /// 开
        /// </summary>
        Open,

        /// <summary>
        /// 关
        /// </summary>
        Close
    }

    public enum DoorType
    {
        /// <summary>
        /// 入库门
        /// </summary>
        In,

        /// <summary>
        /// 出库门
        /// </summary>
        Out
    }
}
