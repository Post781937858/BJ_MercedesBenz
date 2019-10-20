using System;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SuperSocketTask;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using MercedesBenz.DataBase;

namespace MercedesBenz_AGV_Service
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsOpen = false;
        private CancellationTokenSource ClientCancel;
        private Task updateTask;
        private ObservableCollection<BufferInfo> BufferInfos = new ObservableCollection<BufferInfo>();
        private ObservableCollection<agvInfo> agvInfos = new ObservableCollection<agvInfo>();
        private ObservableCollection<DoorInfo> DoorInfos = new ObservableCollection<DoorInfo>();
        private ObservableCollection<OrderTask> OrderTaskNDCInfos = new ObservableCollection<OrderTask>();
        private ObservableCollection<OrderTask> OrderTaskAGVInfos = new ObservableCollection<OrderTask>();

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        #region 侧边Memu

        //菜单
        private void SelectMenu(Label menuItem)
        {
            MenuItem1.Background = new SolidColorBrush(Colors.Transparent);
            MenuItem2.Background = new SolidColorBrush(Colors.Transparent);
            MenuItem3.Background = new SolidColorBrush(Colors.Transparent);
            MenuItem4.Background = new SolidColorBrush(Colors.Transparent);
            MenuItem5.Background = new SolidColorBrush(Colors.Transparent);
            MenuItem6.Background = new SolidColorBrush(Colors.Transparent);
            MenuItem7.Background = new SolidColorBrush(Colors.Transparent);
            menuItem.Background = new SolidColorBrush(Color.FromRgb(73, 143, 215));
        }

        private void MenuItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectMenu(sender as Label);
            ShowPanel(int.Parse((sender as Label).Tag.ToString()));
        }

        private void ShowPanel(int PanelId)
        {
            Panel.SetZIndex(Panel1, -1);
            Panel.SetZIndex(Panel2, -1);
            Panel.SetZIndex(Panel3, -1);
            Panel.SetZIndex(Panel4, -1);
            Panel.SetZIndex(Panel5, -1);
            Panel.SetZIndex(Panel6, -1);
            switch (PanelId)
            {
                case 1:
                    Panel.SetZIndex(Panel1, 9999);
                    break;
                case 2:
                    Panel.SetZIndex(Panel2, 9999);
                    break;
                case 3:
                    Panel.SetZIndex(Panel3, 9999);
                    break;
                case 4:
                    Panel.SetZIndex(Panel4, 9999);
                    break;
                case 5:
                    Panel.SetZIndex(Panel5, 9999);
                    break;
                case 6:
                    Panel.SetZIndex(Panel6, 9999);
                    break;
            }
        }
        #endregion

        #region 头部Menu

        //菜单移入
        private void ButtonMenuIn(Border menuItem, int colorId)
        {
            switch (colorId)
            {
                case 0:
                    menuItem.Background = new SolidColorBrush(Color.FromRgb(40, 141, 246));
                    break;
                case 1:
                    menuItem.Background = new SolidColorBrush(Color.FromRgb(223, 46, 46));
                    break;
            }
        }

        //菜单移出
        private void ButtonMenuOut(Border menuItem, int colorId)
        {
            switch (colorId)
            {
                case 0:
                    menuItem.Background = new SolidColorBrush(Color.FromRgb(8, 127, 250));
                    break;
                case 1:
                    menuItem.Background = new SolidColorBrush(Color.FromRgb(242, 53, 51));
                    break;
            }

        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            ButtonMenuOut(sender as Border, int.Parse((sender as Border).Tag.ToString()));
        }

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            ButtonMenuIn(sender as Border, int.Parse((sender as Border).Tag.ToString()));
        }


        #endregion

        //初始化数据
        private void Init()
        {
            SystemTaskDispose.SystemLog += SystemLog;
            var CarServiceConfig = SystemConfiguration.CarService();
            for (int i = 0; i < CarServiceConfig.Count(); i++)
            {
                if (CarServiceConfig[i].ON)
                    agvInfos.Add(new agvInfo() { agvNumber = CarServiceConfig[i].CarNumber });
            }
            for (int i = 1; i < 6; i++)
            {
                string type = "1.4米";
                if (i == 3 || i == 5)
                {
                    type = "1.1米";
                }
                BufferInfos.Add(new BufferInfo() { Buff_number = i, Buff_typeText = type, Buff_statusText = "无托盘", Turncolor = new SolidColorBrush(Colors.Green) });
            }
            for (int i = 1; i < 3; i++)
            {
                if (i == 1)
                    DoorInfos.Add(new DoorInfo() { DoorNumber = i, DoorStatus = DoorStatus.Close, DoorTypeTexe = "入库栏杆机" });
                else
                    DoorInfos.Add(new DoorInfo() { DoorNumber = i, DoorStatus = DoorStatus.Close, DoorTypeTexe = "出库栏杆机" });
            }
            ClientCancel = new CancellationTokenSource();
            agvInfoTable.DataContext = agvInfos;
            BuffsTable.DataContext = BufferInfos;
            DoorInfoTable.DataContext = DoorInfos;
            OrderNdcTable.DataContext = OrderTaskNDCInfos;
            OrderAGVTable.DataContext = OrderTaskAGVInfos;
            updateTask = Task.Run(() =>
            {
                while (!ClientCancel.IsCancellationRequested)
                {
                    UpdateinfoTable();
                    Thread.Sleep(500);
                }
            }, ClientCancel.Token);
            Task.Run(() => { StartServer_MouseDown(null, null); });
        }

        //开启服务
        private void StartServer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!IsOpen)
                {
                    IsOpen = true;
                    SystemTaskDispose.StartService();
                    SystemLog("服务已开启", true);
                }
                else
                {
                    SystemLog("服务已开启", true);
                }
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }

        //日志输出
        public void SystemLog(string message, bool IsLog)
        {
            SystemtxtLog.Dispatcher.BeginInvoke(new Action(() =>
            {
                //大于2000行清除记录，可选
                if (SystemtxtLog.LineCount > 2000)
                {
                    SystemtxtLog.Clear();
                }
                SystemtxtLog.AppendText($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}：" + message + "\r\n");
                if (!SystemtxtLog.IsMouseOver)
                {
                    SystemtxtLog.ScrollToEnd();
                }
            }));
            if (IsLog == true)
            {
                Log4NetHelper.WriteSystemLog(message);
            }
        }

        //停止服务
        private void StopServer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (IsOpen)
                {
                    IsOpen = false;
                    SystemTaskDispose.StopService();
                    SystemLog("服务已关闭", true);
                }
                else
                {
                    SystemLog("服务未开启", true);
                }
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }

        //重启服务
        private void ServerOpen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                IsOpen = true;
                SystemTaskDispose.StopService();
                SystemTaskDispose.StartService();
                SystemLog("服务重启成功", true);
            }
            catch (Exception ex)
            {
                SystemLog($"服务重启失败，异常信息:{ex.Message}", true);
            }
        }

        //更新UI
        private void UpdateinfoTable()
        {
            var OrderArray = SystemTaskDatabase.Instance.QueryOrderDNC();
            if (OrderArray != null)
            {
                OrderArray.ForEach(p =>
                {
                    var OrderModel = OrderTaskNDCInfos.FirstOrDefault(item => item.order_ordernumber == p.order_ordernumber);
                    if (OrderModel == null)
                    {
                        if (!ClientCancel.IsCancellationRequested)
                            this.Dispatcher.Invoke(() =>
                            {
                                OrderTaskNDCInfos.Add(p);
                            });
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            var Index = OrderTaskNDCInfos.IndexOf(OrderModel);
                            p.IsSelected = OrderModel.IsSelected;
                            OrderTaskNDCInfos[Index] = p;
                        });
                    }
                });
                for (int i = 0; i < OrderTaskNDCInfos.ToArray().Count(); i++)
                {
                    var OrderItem = OrderTaskNDCInfos[i];
                    var OrderModel = OrderArray.FirstOrDefault(p => p.order_ordernumber == OrderItem.order_ordernumber);
                    if (OrderModel == null)
                    {
                        if (!ClientCancel.IsCancellationRequested)
                            this.Dispatcher.Invoke(() =>
                            {
                                OrderTaskNDCInfos.Remove(OrderItem);
                            });
                    }
                }
            }

            var OrderAGVArray = SystemTaskDatabase.Instance.QueryOrderAGV();
            if (OrderAGVArray != null)
            {
                OrderAGVArray.ForEach(p =>
                {
                    var OrderModel = OrderTaskAGVInfos.FirstOrDefault(item => item.order_ordernumber == p.order_ordernumber);
                    if (OrderModel == null)
                    {
                        if (!ClientCancel.IsCancellationRequested)
                            this.Dispatcher.Invoke(() =>
                            {
                                OrderTaskAGVInfos.Add(p);
                            });
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            var Index = OrderTaskAGVInfos.IndexOf(OrderModel);
                            p.IsSelected = OrderModel.IsSelected;
                            OrderTaskAGVInfos[Index] = p;
                        });
                    }
                });
                for (int i = 0; i < OrderTaskAGVInfos.ToArray().Count(); i++)
                {
                    var OrderItem = OrderTaskAGVInfos[i];
                    var OrderModel = OrderAGVArray.FirstOrDefault(p => p.order_ordernumber == OrderItem.order_ordernumber);
                    if (OrderModel == null)
                    {
                        if (!ClientCancel.IsCancellationRequested)
                            this.Dispatcher.Invoke(() =>
                            {
                                OrderTaskAGVInfos.Remove(OrderItem);
                            });
                    }
                }

                for (int i = 0; i < agvInfos.Count; i++)
                {
                    var agvInfo = agvInfos[i];
                    if (SystemTaskDispose.agvInfoList.ContainsKey(agvInfo.agvNumber))
                    {
                        if (!ClientCancel.IsCancellationRequested)
                            this.Dispatcher.Invoke(() =>
                            {
                                agvInfos[i] = SystemTaskDispose.agvInfoList[agvInfo.agvNumber];
                            });
                    }
                }
            }

            var BufferData = SystemTaskDatabase.Instance.QueryBufffer();
            if (BufferData != null)
            {
                for (int i = 0; i < BufferInfos.Count; i++)
                {
                    var BufferInfo = BufferData.FirstOrDefault(s => s.Buff_number == BufferInfos[i].Buff_number);
                    if (BufferInfo != null)
                    {
                        if (!ClientCancel.IsCancellationRequested)
                            this.Dispatcher.Invoke(() =>
                            {
                                BufferInfos[i].Buff_statusText = BufferInfo.Buff_status == 0 ? "无托盘" : "有托盘";
                                BufferInfos[i].updateDateInfoTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                BufferInfos[i].Turncolor = BufferInfo.Buff_status == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                            });
                    }
                }

                for (int i = 0; i < DoorInfos.Count; i++)
                {
                    var DoorInfo = DoorInfos[i];
                    if (i == 0)
                    {
                        if (SystemTaskDispose.DoorInfoArray.ContainsKey(DoorType.In))
                        {
                            if (!ClientCancel.IsCancellationRequested)
                                this.Dispatcher.Invoke(() =>
                                {
                                    var updateDoor = SystemTaskDispose.DoorInfoArray[DoorType.In];
                                    DoorInfos[i].DoorStatus = updateDoor.DoorStatus;
                                    DoorInfos[i].updateDateInfoTime = updateDoor.updateDateInfoTime;
                                });
                        }
                    }
                    else
                    {
                        if (SystemTaskDispose.DoorInfoArray.ContainsKey(DoorType.Out))
                        {
                            if (!ClientCancel.IsCancellationRequested)
                                this.Dispatcher.Invoke(() =>
                                {
                                    var updateDoor = SystemTaskDispose.DoorInfoArray[DoorType.Out];
                                    DoorInfos[i].DoorStatus = updateDoor.DoorStatus;
                                    DoorInfos[i].updateDateInfoTime = updateDoor.updateDateInfoTime;
                                });
                        }
                    }
                }
            }
        }

        //清除清除日志
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            SystemtxtLog.Clear();
        }


        //格式校验
        private bool VerifyText(string typeText, string Text, out int Type)
        {
            Type = 0;
            if (!string.IsNullOrEmpty(Text))
            {
                if (FormatVerification.IsFloat(Text))
                {
                    int type = typeText == "箱号" ? 0 : 1;
                    if ((type == 0 && Text.Length == 4) || (type == 1 && Text.Length == 8))
                    {
                        Type = type;
                        return true;
                    }
                    else
                    {
                        ShowText("货位或箱号格式输入错误！", -1);
                        return false;
                    }
                }
                else
                {
                    ShowText("货位或箱号只能为数字！", -1);
                    return false;
                }
            }
            else
            {
                ShowText("货位或箱号不可为空！", -1);
                return false;
            }
        }


        //消息提示
        private void ShowText(string message, int errorcode)
        {
            if (errorcode == 0)
            {
                messageShow.Foreground = new SolidColorBrush(Color.FromRgb(8, 127, 250));
            }
            else
            {
                messageShow.Foreground = new SolidColorBrush(Colors.Red);
            }
            messageShow.Content = "";
            messageShow.Content = message;
            Task.Delay(10000).ContinueWith(task =>
            {
                this.Dispatcher.InvokeAsync(() =>
                {
                    messageShow.Content = "";
                });
            });
        }

        //入库任务
        private void IssuedIn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int TypeInfo = 0;
            if (VerifyText(SiteType.Text, SiteText.Text, out TypeInfo))
            {
                string location = TypeInfo == 0 ? WareHouseFChangeLoc(SiteText.Text) : SiteText.Text;
                var taskmsg = new boxAnnounceTaskmsg()
                {
                    boxId = TypeInfo == 0 ? SiteText.Text : "",
                    wmsId = location,
                    s_station = InStation.Text,
                    location = location
                };
                MessageBoxResult result = MessageBox.Show($"确定发布入库站台为{taskmsg.s_station}{(TypeInfo == 0 ? "箱号为" + SiteText.Text : "")},货位为{taskmsg.location}的入库任务吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    var orderTask = SystemTaskDatabase.Instance.QueryOrderWMSID(taskmsg.wmsId);
                    if (orderTask == null)
                    {
                        SystemTaskDatabase.Instance.IssueOrderTask(taskmsg, OrderType.NDC, OrderTaskType.In);
                        SiteText.Text = "";
                        ShowText("入库任务发布成功！", 0);
                    }
                    else
                    {
                        ShowText("任务已存在！", -1);
                    }
                }
            }
        }

        //出库
        private void Out_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int SiteType = 0;
            if (VerifyText(OutType.Text, OutLocation.Text, out SiteType))
            {
                string Location = SiteType == 0 ? WareHouseFChangeLoc(OutLocation.Text) : OutLocation.Text;
                StockOutTaskmsg stockOutTaskmsg = new StockOutTaskmsg() { location = Location, wmsId = Location, boxId = SiteType == 0 ? OutLocation.Text : "", };
                MessageBoxResult result = MessageBox.Show($"确定发布货位为{Location}{(SiteType == 0 ? "箱号为" + OutLocation.Text : "")}的出库任务吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    var orderTask = SystemTaskDatabase.Instance.QueryOrderWMSID(stockOutTaskmsg.wmsId);
                    if (orderTask == null)
                    {
                        SystemTaskDatabase.Instance.IssueOrderTask(stockOutTaskmsg, OrderType.NDC, OrderTaskType.Out);
                        OutLocation.Text = "";
                        ShowText("出库任务发布成功！", 0);
                    }
                    else
                    {
                        ShowText("任务已存在！", -1);
                    }
                }
            }
        }

        //移库
        private void MoveWarehouse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int originalType = 0;
            int NewType = 0;
            if (VerifyText(moveOriginalType.Text, moveOriginalText.Text, out originalType) && VerifyText(moveNewType.Text, moveNewText.Text, out NewType))
            {
                string OriginalLocation = originalType == 0 ? WareHouseFChangeLoc(moveOriginalText.Text) : moveOriginalText.Text;

                string NewLocation = NewType == 0 ? WareHouseFChangeLoc(moveNewText.Text) : moveNewText.Text;

                movePositionTaskmsg movePosition = new movePositionTaskmsg()
                {
                    wmsId = OriginalLocation,
                    s_location = OriginalLocation,
                    d_location = NewLocation
                };

                MessageBoxResult result = MessageBox.Show($"确定发布原始货位为{movePosition.s_location},新库位{movePosition.d_location}的出库任务吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    var orderTask = SystemTaskDatabase.Instance.QueryOrderWMSID(movePosition.wmsId);
                    if (orderTask == null)
                    {
                        moveOriginalText.Text = "";
                        moveNewText.Text = "";
                        SystemTaskDatabase.Instance.IssueOrderTask(movePosition, OrderType.NDC, OrderTaskType.Interior);
                        ShowText("移库任务发布成功！", 0);
                    }
                    else
                    {
                        ShowText("任务已存在！", -1);
                    }
                }
            }
        }


        //根据箱号转换为对应库位
        public string WareHouseFChangeLoc(string contain)
        {
            var loc = "";
            var hj = contain.Substring(0, 2);
            if (int.Parse(hj) < 15)
            {
                var lie = int.Parse(contain.Substring(2, 2));
                var line = "";
                var row = "";
                if (lie % 8 == 0)
                {
                    line = ((lie - 1) / 8 + 1).ToString("00");
                }
                else
                {
                    line = (lie / 8 + 1).ToString("00");
                }
                var rowText = ((int.Parse(line) * 8 - lie) + 1);
                row = (((8 - rowText) / 2) + 2).ToString("00");
                if (lie % 2 == 0)
                {
                    loc = "F" + hj + line + row + "02";
                }
                else
                {
                    loc = "F" + hj + line + row + "01";
                }
            }
            else
            {
                var lie = int.Parse(contain.Substring(2, 2));
                var line = "";
                var row = "";
                if (lie % 12 == 0)
                {
                    line = ((lie - 1) / 12 + 1).ToString("00");
                }
                else
                {
                    line = (lie / 12 + 1).ToString("00");
                }
                var rowText = ((int.Parse(line) * 12 - lie) + 1);
                row = (((12 - rowText) / 3) + 2).ToString("00");
                var a = lie % 3;
                if (lie % 3 == 0)
                {
                    loc = "F" + hj + line + row + "03";
                }
                else if (lie % 3 == 2)
                {
                    loc = "F" + hj + line + row + "02";
                }
                else
                {
                    loc = "F" + hj + line + row + "01";
                }
            }
            return loc;
        }


        //应用程序关闭
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClientCancel.Cancel();
            Task.Run(() => { SystemTaskDispose.StopService(); });
        }

        //NDC订单全选or反选
        private void AllNDCCkek_Click(object sender, RoutedEventArgs e)
        {
            OrderTaskNDCInfos.ToList().ForEach(x => x.IsSelected = ((CheckBox)sender).IsChecked == true ? true : false); //全选or反选
        }

        //AGV订单全选or反选
        private void AllAGVCkek_Click(object sender, RoutedEventArgs e)
        {
            OrderTaskAGVInfos.ToList().ForEach(x => x.IsSelected = ((CheckBox)sender).IsChecked == true ? true : false); //全选or反选
        }

        //删除NDC订单
        private void DeleteNDCTask_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show($"确定删除任务吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                OrderTaskNDCInfos.ToList().Where(p => p.IsSelected).ToList().ForEach(item =>
                {
                    if (item.order_ikey == -1)
                    {
                        if (SystemTaskDatabase.Instance.DeleteOrder_history(item) == 0)
                        {
                            MessageBox.Show("任务删除失败", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("任务已执行，如需终止订单请点击取消订单", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }

        //取消叉车任务
        private void CancelNDCTask_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show($"确定取消任务吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                OrderTaskNDCInfos.ToList().Where(p => p.IsSelected).ToList().ForEach(item =>
                {
                    if (item.order_ikey != -1)
                    {
                        item.order_cancel = -1;
                        if (!SystemTaskDatabase.Instance.cancelOrderTask(item))
                        {
                            throw new Exception($"任务取消失败，订单信息：{item.order_ordernumber}");
                        }
                    }
                    else
                    {
                        SystemTaskDatabase.Instance.DeleteOrder_history(item);
                    }
                });
            }
        }

        //入库门开启
        private void InDoorOpen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!OperateIniTool.OperateIniWrite(1, "Service", "InDoorNO"))
            {
                throw new Exception("写入失败，请检查setting.ini文件是否存在");
            }
        }

        //入库门关闭
        private void InDoorClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!OperateIniTool.OperateIniWrite(2, "Service", "InDoorNO"))
            {
                throw new Exception("写入失败，请检查setting.ini文件是否存在");
            }
        }

        //出库门开启
        private void OutDoorOpen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!OperateIniTool.OperateIniWrite(1, "Service", "OutDoorNO"))
            {
                throw new Exception("写入失败，请检查setting.ini文件是否存在");
            }
        }

        //出库门关闭
        private void OutDoorClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!OperateIniTool.OperateIniWrite(2, "Service", "OutDoorNO"))
            {
                throw new Exception("写入失败，请检查setting.ini文件是否存在");
            }
        }

        private void LightOpen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                for (int i = 1; i < 7; i++)
                {
                    SystemTaskDatabase.Instance.UpdateStatus(i, 1);
                    SystemTaskDatabase.Instance.UpdateSwitch(i, 1);
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        private void LightClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                for (int i = 1; i < 7; i++)
                {
                    SystemTaskDatabase.Instance.UpdateStatus(i, 0);
                    SystemTaskDatabase.Instance.UpdateSwitch(i, 2);
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        //删除AGV订单
        private void DeleteAGVTask_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var IsSelect = OrderTaskAGVInfos.ToList().Where(p => p.IsSelected).ToList();
            if (IsSelect.Count() > 0)
            {
                MessageBoxResult result = MessageBox.Show($"确定删除任务吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    OrderTaskAGVInfos.ToList().Where(p => p.IsSelected).ToList().ForEach(item =>
                    {
                        if (SystemTaskDatabase.Instance.DeleteOrder_history(item) == 0)
                        {
                            MessageBox.Show("任务删除失败", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }
            }
            else
            {
                MessageBox.Show("请选中您需要操作的列表项", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //强制上料完成
        private void ForceUp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var IsSelect = OrderTaskAGVInfos.ToList().Where(p => p.IsSelected).ToList();
            if (IsSelect.Count() > 0)
            {
                MessageBoxResult result = MessageBox.Show($"确定执行上料完成操作吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    IsSelect.ForEach(item =>
                    {
                        if (!SystemTaskDatabase.Instance.UpdatemagicCarNo(2, item.order_ordernumber, item.order_carno))
                        {
                            MessageBox.Show("任务操作失败", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }
            }
            else
            {
                MessageBox.Show("请选中您需要操作的列表项", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //强制下料完成
        private void ForceDown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var IsSelect = OrderTaskAGVInfos.ToList().Where(p => p.IsSelected).ToList();
            if (IsSelect.Count() > 0)
            {
                MessageBoxResult result = MessageBox.Show($"确定执行下料完成操作吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    IsSelect.ForEach(item =>
                    {
                        if (SystemTaskDatabase.Instance.DeleteOrder_history(item) == 0)
                        {
                            MessageBox.Show("任务操作失败", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }
            }
            else
            {
                MessageBox.Show("请选中您需要操作的列表项", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //入库门指令重置
        private void ResetInDoor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!OperateIniTool.OperateIniWrite(0, "Service", "InDoorNO"))
            {
                throw new Exception("写入失败，请检查setting.ini文件是否存在");
            }
        }

        //出库门指令重置
        private void ResetOutDoor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!OperateIniTool.OperateIniWrite(0, "Service", "OutDoorNO"))
            {
                throw new Exception("写入失败，请检查setting.ini文件是否存在");
            }
        }

        //灯控指令重置
        private void LightReset_MouseDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 1; i < 7; i++)
            {
                SystemTaskDatabase.Instance.UpdateSwitch(i, 0);
            }
        }
    }
}
