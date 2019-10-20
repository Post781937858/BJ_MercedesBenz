using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MercedesBenz.DataBase;
using MercedesBenz.Infrastructure;

namespace SwitchClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<int, int> SwitchList = new Dictionary<int, int>();
        private CancellationTokenSource ClientCancel = new CancellationTokenSource();
        private Task UpdateTask;

        public MainWindow()
        {
            InitializeComponent();
            for (int i = 1; i < 7; i++)
            {
                SwitchList.Add(i, 0);
            }
            UpdateTask = Task.Run(() =>
             {
                 while (!ClientCancel.IsCancellationRequested)
                 {
                     UpdateButtonStatus();
                     Thread.Sleep(500);
                 }

             }, ClientCancel.Token);
        }

        private void UpdateButtonStatus()
        {
            var SwitchInfoList = SystemTaskDatabase.Instance.QuerySwitchConfiguration();
            foreach (var Switchitem in SwitchInfoList)
            {
                this.Dispatcher.Invoke(() =>
                {
                    switch (Switchitem.ID)
                    {
                        case 1:
                            if (Switchitem.Status == 0)
                            {
                                SwitchList[Switchitem.ID] = 2;
                                switchImg4.Source = new BitmapImage(new Uri("pack://application:,,,/img/light.png", UriKind.Absolute));
                                switch4.Content = "开启";
                            }
                            else
                            {
                                switchImg4.Source = new BitmapImage(new Uri("pack://application:,,,/img/lighton.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 1;
                                switch4.Content = "关闭";
                            }
                            break;
                        case 2:
                            if (Switchitem.Status == 0)
                            {
                                switchImg2.Source = new BitmapImage(new Uri("pack://application:,,,/img/light.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 2;
                                switch2.Content = "开启";
                            }
                            else
                            {
                                switchImg2.Source = new BitmapImage(new Uri("pack://application:,,,/img/lighton.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 1;
                                switch2.Content = "关闭";
                            }
                            break;
                        case 3:
                            if (Switchitem.Status == 0)
                            {
                                switchImg6.Source = new BitmapImage(new Uri("pack://application:,,,/img/light.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 2;
                                switch6.Content = "开启";
                            }
                            else
                            {
                                switchImg6.Source = new BitmapImage(new Uri("pack://application:,,,/img/lighton.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 1;
                                switch6.Content = "关闭";
                            }
                            break;
                        case 4:
                            if (Switchitem.Status == 0)
                            {
                                switchImg5.Source = new BitmapImage(new Uri("pack://application:,,,/img/light.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 2;
                                switch5.Content = "开启";
                            }
                            else
                            {
                                switchImg5.Source = new BitmapImage(new Uri("pack://application:,,,/img/lighton.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 1;
                                switch5.Content = "关闭";
                            }
                            break;
                        case 5:
                            if (Switchitem.Status == 0)
                            {
                                switchImg3.Source = new BitmapImage(new Uri("pack://application:,,,/img/light.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 2;
                                switch3.Content = "开启";
                            }
                            else
                            {
                                switchImg3.Source = new BitmapImage(new Uri("pack://application:,,,/img/lighton.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 1;
                                switch3.Content = "关闭";
                            }
                            break;
                        case 6:
                            if (Switchitem.Status == 0)
                            {
                                switchImg1.Source = new BitmapImage(new Uri("pack://application:,,,/img/light.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 2;
                                switch1.Content = "开启";
                            }
                            else
                            {
                                switchImg1.Source = new BitmapImage(new Uri("pack://application:,,,/img/lighton.png", UriKind.Absolute));
                                SwitchList[Switchitem.ID] = 1;
                                switch1.Content = "关闭";
                            }
                            break;
                    }
                });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ButtonInfo = sender as Button;
            var Key = ButtonInfo.Tag.ToInt();
            if (SwitchList.ContainsKey(Key))
            {
                var SwitchStatus = SwitchList[Key];
                if (SwitchStatus == 1)
                {
                    SystemTaskDatabase.Instance.UpdateSwitch(Key, 2);
                    SystemTaskDatabase.Instance.UpdateStatus(Key, 0);
                }
                else
                {
                    SystemTaskDatabase.Instance.UpdateStatus(Key, 1);
                    SystemTaskDatabase.Instance.UpdateSwitch(Key, 1);
                }
            }
        }

        private void CountSwitchON_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 7; i++)
            {
                SystemTaskDatabase.Instance.UpdateStatus(i, 1);
                SystemTaskDatabase.Instance.UpdateSwitch(i, 1);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Task.Run(() =>
            {
                ClientCancel.Cancel();
                Task.WaitAll(new Task[] { UpdateTask });
            });
        }

        private void CountSwitchOFF_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 7; i++)
            {
                SystemTaskDatabase.Instance.UpdateStatus(i, 0);
                SystemTaskDatabase.Instance.UpdateSwitch(i, 2);
            }
        }
    }
}
