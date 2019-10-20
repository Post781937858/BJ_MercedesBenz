using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.Models
{
    public class NetworkInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// IP地址
        /// </summary>
        private string iPaddress;

        public string IPaddress
        {
            get { return iPaddress; }
            set { iPaddress = value; GetChanged("IPaddress"); }
        }


        /// <summary>
        /// 端口
        /// </summary>
        private string port;

        public string Port
        {
            get { return port; }
            set { port = value; GetChanged("Port"); }
        }


        /// <summary>
        /// 网络类型
        /// </summary>
        private string networkType;

        public string NetworkType
        {
            get { return networkType; }
            set { networkType = value; GetChanged("NetworkType"); }
        }

        /// <summary>
        /// 网络状态
        /// </summary>
        private string networkStatus;

        public string NetworkStatus
        {
            get { return networkStatus; }
            set { networkStatus = value; GetChanged("NetworkStatus"); }
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
