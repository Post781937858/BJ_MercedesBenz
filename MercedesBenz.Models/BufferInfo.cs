using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MercedesBenz.Models
{
    public class BufferInfo : INotifyPropertyChanged
    {
        public BufferInfo()
        {
            this.updateDateInfoTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 缓存区ID
        /// </summary>
        public string Buffer_guid { get; set; }

        /// <summary>
        /// 缓存区编号
        /// </summary>
        private int buff_number { get; set; }
        public int Buff_number
        {
            get { return buff_number; }
            set
            {
                buff_number = value;
                GetChanged("Buff_number");
            }
        }

        /// <summary>
        /// 缓存区类型
        /// </summary>
        private int buff_type;
        public int Buff_type 
        {
            get { return buff_type; }
            set
            {
                buff_type = value;
                GetChanged("Buff_type");
            }
        }

        private string buff_typeText;
        public string Buff_typeText
        {
            get { return buff_typeText; }
            set
            {
                buff_typeText = value;
                GetChanged("Buff_typeText");
            }
        }

        /// <summary>
        /// 缓存位状态
        /// </summary>
        private int buff_status;
        public int Buff_status
        {
            get { return buff_status; }
            set
            {
                buff_status = value;
                GetChanged("Buff_status");
            }
        }

        private string buff_statusText;
        public string Buff_statusText
        {
            get { return buff_statusText; }
            set
            {
                buff_statusText = value;
                GetChanged("Buff_statusText");
            }
        }


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

        /// <summary>
        /// agv站台号
        /// </summary>
        public int Buff_Site { get; set; }

        /// <summary>
        /// NDC站台号
        /// </summary>
        public int Buff_ndcSite { get; set; }


        /// <summary>
        /// 颜色
        /// </summary>
        private Brush turncolor;
        public Brush Turncolor
        {
            get { return turncolor; }
            set
            {
                turncolor = value;
                GetChanged("Turncolor");
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
}
