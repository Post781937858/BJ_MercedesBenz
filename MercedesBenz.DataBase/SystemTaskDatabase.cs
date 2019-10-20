using MercedesBenz.DataBase.DapperExecute.DataBase;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using System;
using System.Collections.Generic;

namespace MercedesBenz.DataBase
{
    public class SystemTaskDatabase
    {
        public static SystemTaskDatabase Instance { get; } = new SystemTaskDatabase();

        /// <summary>
        /// 添加任务订单
        /// </summary>
        public bool IssueOrderTask(WCSTask _issueTaskmsg, OrderType OrderType, OrderTaskType TaskType)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
             {
                 string _order_getLocation = "";
                 string _order_putLocation = "";
                 string _order_getSite = "";
                 string _order_putSite = "";
                 if (OrderType == OrderType.NDC && TaskType == OrderTaskType.Out)
                 {
                     _order_getLocation = (_issueTaskmsg as StockOutTaskmsg).location;
                     _order_putLocation = (_issueTaskmsg as StockOutTaskmsg).d_station;
                 }
                 else if (OrderType == OrderType.NDC && TaskType == OrderTaskType.In)
                 {
                     _order_getLocation = (_issueTaskmsg as boxAnnounceTaskmsg).s_station;
                     _order_putLocation = (_issueTaskmsg as boxAnnounceTaskmsg).location;
                 }
                 else if (OrderType == OrderType.NDC && TaskType == OrderTaskType.Interior)
                 {
                     _order_getLocation = (_issueTaskmsg as movePositionTaskmsg).s_location;
                     _order_putLocation = (_issueTaskmsg as movePositionTaskmsg).d_location;
                 }
                 else if (OrderType == OrderType.AGV && TaskType == OrderTaskType.Platform)
                 {
                     _order_getLocation = (_issueTaskmsg as carryPlatfromTaskmsg).s_station;
                     _order_putLocation = (_issueTaskmsg as carryPlatfromTaskmsg).d_station;
                     _order_getSite = _order_getLocation;
                     _order_putSite = _order_putLocation;
                 }
                 else if (OrderType == OrderType.AGV && TaskType == OrderTaskType.In)
                 {
                     _order_getLocation = (_issueTaskmsg as boxAnnounceTaskmsg).s_station;
                     _order_putLocation = (_issueTaskmsg as boxAnnounceTaskmsg).location;
                     _order_getSite = (_issueTaskmsg as boxAnnounceTaskmsg).s_station;
                     _order_putSite = (_issueTaskmsg as boxAnnounceTaskmsg).d_station.ToString();
                 }
                 else if (OrderType == OrderType.AGV && TaskType == OrderTaskType.Out)
                 {
                     _order_getSite = (_issueTaskmsg as carryPlatfromTaskmsg).s_station;
                     _order_putSite = (_issueTaskmsg as carryPlatfromTaskmsg).d_station;
                 }
                 var orderTask = new
                 {
                     order_guid = Guid.NewGuid().ToString(),
                     order_arecore = _issueTaskmsg.areaCode,
                     order_sourceCode = _issueTaskmsg.sourceCode,
                     order_ordernumber = $"BM{DateTime.Now.ToString("yyyyMMddHHmmssss")}{new Random().Next(0, 999999)}",
                     order_priority = _issueTaskmsg.priority,
                     order_status = -1,
                     order_utctime = UTC.ConvertDateTimeLong(DateTime.Now),
                     order_type = TaskType.ToString(),
                     order_carno = -1,
                     order_ikey = -1,
                     order_index = -1,
                     order_magic = -1,
                     order_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                     order_genre = OrderType.ToString(),
                     order_boxId = _issueTaskmsg.boxId,
                     order_wmsId = TaskType == OrderTaskType.Platform ? _issueTaskmsg.id.ToString() : _issueTaskmsg.wmsId,
                     order_getLocation = _order_getLocation,
                     order_putLocation = _order_putLocation,
                     order_getSite = _order_getSite,
                     order_putSite = _order_putSite,
                     order_cancel = 0
                 };
                 return connection.Executes("INSERT INTO mercedesbenzdb.`tb_order`(order_guid,order_ordernumber,order_getLocation,order_putLocation,order_priority,order_status,order_magic,order_ikey,order_index,order_carno,order_utctime,order_time,order_arecore,order_type,order_genre,order_boxId,order_wmsId,order_sourceCode,order_getSite,order_putSite,order_cancel) values (@order_guid,@order_ordernumber,@order_getLocation,@order_putLocation,@order_priority,@order_status,@order_magic,@order_ikey,@order_index,@order_carno,@order_utctime,@order_time,@order_arecore,@order_type,@order_genre,@order_boxId,@order_wmsId,@order_sourceCode,@order_getSite,@order_putSite,@order_cancel)", orderTask) > 0;
             });
        }

        public bool InsertOriginTask(OrderTask order)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                var orderTask = new
                {
                    order_guid = Guid.NewGuid().ToString(),
                    order_arecore = "",
                    order_sourceCode = "",
                    order_ordernumber = $"BM{DateTime.Now.ToString("yyyyMMddHHmmssss")}{new Random().Next(0, 999999)}",
                    order_priority = 0,
                    order_status = -1,
                    order_utctime = UTC.ConvertDateTimeLong(DateTime.Now),
                    order_type = "OriginTask",
                    order_carno = order.order_carno,
                    order_ikey = -1,
                    order_index = -1,
                    order_magic = 9999,
                    order_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    order_genre = "AGV",
                    order_boxId = "",
                    order_wmsId = "9999",
                    order_getLocation = "",
                    order_putLocation = "",
                    order_getSite = order.order_getSite,
                    order_putSite = "",
                    order_cancel=0
                };
                return connection.Executes("INSERT INTO mercedesbenzdb.`tb_order`(order_guid,order_ordernumber,order_getLocation,order_putLocation,order_priority,order_status,order_magic,order_ikey,order_index,order_carno,order_utctime,order_time,order_arecore,order_type,order_genre,order_boxId,order_wmsId,order_sourceCode,order_getSite,order_putSite,order_cancel) values (@order_guid,@order_ordernumber,@order_getLocation,@order_putLocation,@order_priority,@order_status,@order_magic,@order_ikey,@order_index,@order_carno,@order_utctime,@order_time,@order_arecore,@order_type,@order_genre,@order_boxId,@order_wmsId,@order_sourceCode,@order_getSite,@order_putSite,@order_cancel)", orderTask) > 0;
            });
        }


        /// <summary>
        /// 根据订单编号更新ikey
        /// </summary>
        /// <param name="order_ikey"></param>
        /// <param name="order_ordernumber"></param>
        public void UpdateOrder_ikey(int order_ikey, string _order_guid)
        {
            DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_ikey=@order_ikey,order_utctime=@order_utctime WHERE order_guid=@order_guid",
                    new { order_ikey = order_ikey, order_guid = _order_guid, order_utctime = UTC.ConvertDateTimeLong(DateTime.Now) });
            });
        }

        /// <summary>
        /// 根据ikey更新index
        /// </summary>
        /// <param name="order_index"></param>
        /// <param name="order_ikey"></param>
        public void UpdateOrder_Index(int order_index, int order_ikey)
        {
            DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_index=@order_index,order_status=@order_status,order_utctime=@order_utctime WHERE order_ikey=@order_ikey",
                    new { order_ikey = order_ikey, order_status = 1, order_index = order_index, order_utctime = UTC.ConvertDateTimeLong(DateTime.Now) });
            });
        }

        /// <summary>
        /// 根据index 更新magic
        /// </summary>
        /// <param name="order_magic"></param>
        /// <param name="order_index"></param>
        public void UpdateOrder_order_magic(int order_magic, int order_index, int car_no)
        {
            DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_magic=@order_magic,order_utctime=@order_utctime,order_carno=@order_carno WHERE order_index=@order_index",
                    new { order_magic = order_magic, order_index = order_index, order_carno = car_no, order_utctime = UTC.ConvertDateTimeLong(DateTime.Now) });
            });
        }

        /// <summary>
        /// 根据index 更新status
        /// </summary>
        /// <param name="order_status"></param>
        /// <param name="order_index"></param>
        public void UpdateOrder_status(int order_status, int order_index)
        {
            DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_status=@order_status,order_utctime=@order_utctime WHERE order_index=@order_index",
                    new { order_status = order_status, order_index = order_index, order_utctime = UTC.ConvertDateTimeLong(DateTime.Now) });
            });
        }

        /// <summary>
        /// 将已完成订单移动到历史订单
        /// </summary>
        /// <param name="_orderTask"></param>
        public int DeleteOrder_history(OrderTask _orderTask)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, (connection, transaction) =>
            {
                connection.Executes("DELETE FROM mercedesbenzdb.`tb_order` WHERE order_ordernumber=@order_ordernumber", _orderTask, transaction);
                var orderTask = new
                {
                    order_guid = _orderTask.order_guid,
                    order_arecore = _orderTask.order_arecore,
                    order_ordernumber = _orderTask.order_ordernumber,
                    order_priority = _orderTask.order_priority,
                    order_status = _orderTask.order_status,
                    order_utctime = _orderTask.order_utctime,
                    order_type = _orderTask.order_type.ToString(),
                    order_carno = _orderTask.order_carno,
                    order_ikey = _orderTask.order_ikey,
                    order_index = _orderTask.order_index,
                    order_magic = _orderTask.order_magic,
                    order_time = _orderTask.order_time,
                    order_genre = _orderTask.order_genre.ToString(),
                    order_boxId = _orderTask.order_boxId,
                    order_wmsId = _orderTask.order_wmsId,
                    order_getLocation = _orderTask.order_getLocation,
                    order_putLocation = _orderTask.order_putLocation,
                    order_sourceCode = _orderTask.order_sourceCode,
                    order_getSite = _orderTask.order_getSite,
                    order_putSite = _orderTask.order_putSite,
                    order_cancel= _orderTask.order_cancel
                };
                return connection.Executes("INSERT INTO mercedesbenzdb.`tb_order_history`(order_guid,order_ordernumber,order_getLocation,order_putLocation,order_priority,order_status,order_magic,order_ikey,order_index,order_carno,order_utctime,order_time,order_arecore,order_type,order_genre,order_boxId,order_wmsId,order_sourceCode,order_getSite,order_putSite,order_cancel) values (@order_guid,@order_ordernumber,@order_getLocation,@order_putLocation,@order_priority,@order_status,@order_magic,@order_ikey,@order_index,@order_carno,@order_utctime,@order_time,@order_arecore,@order_type,@order_genre,@order_boxId,@order_wmsId,@order_sourceCode,@order_getSite,@order_putSite,@order_cancel)", orderTask, transaction);
            });
        }


        /// <summary>
        /// AGV放货完成产生入库订单
        /// </summary>
        /// <param name="_orderTask"></param>
        public void EstablishNDCTask(OrderTask _orderTask)
        {
            DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, (connection, transaction) =>
            {
                if (connection.Executes("DELETE FROM mercedesbenzdb.`tb_order_create` WHERE order_ordernumber=@order_ordernumber", _orderTask, transaction) > 0)
                {
                    var orderTask = new
                    {
                        order_guid = _orderTask.order_guid,
                        order_arecore = _orderTask.order_arecore,
                        order_ordernumber = _orderTask.order_ordernumber,
                        order_priority = _orderTask.order_priority,
                        order_status = _orderTask.order_status,
                        order_utctime = _orderTask.order_utctime,
                        order_type = _orderTask.order_type.ToString(),
                        order_carno = _orderTask.order_carno,
                        order_ikey = _orderTask.order_ikey,
                        order_index = _orderTask.order_index,
                        order_magic = _orderTask.order_magic,
                        order_time = _orderTask.order_time,
                        order_genre = _orderTask.order_genre.ToString(),
                        order_boxId = _orderTask.order_boxId,
                        order_wmsId = _orderTask.order_wmsId,
                        order_getLocation = _orderTask.order_getLocation,
                        order_putLocation = _orderTask.order_putLocation,
                        order_sourceCode = _orderTask.order_sourceCode,
                        order_getSite = _orderTask.order_getSite,
                        order_putSite = _orderTask.order_putSite,
                        order_cancel= _orderTask.order_cancel
                    };
                    connection.Executes("INSERT INTO mercedesbenzdb.`tb_order`(order_guid,order_ordernumber,order_getLocation,order_putLocation,order_priority,order_status,order_magic,order_ikey,order_index,order_carno,order_utctime,order_time,order_arecore,order_type,order_genre,order_boxId,order_wmsId,order_sourceCode,order_getSite,order_putSite,order_cancel) values (@order_guid,@order_ordernumber,@order_getLocation,@order_putLocation,@order_priority,@order_status,@order_magic,@order_ikey,@order_index,@order_carno,@order_utctime,@order_time,@order_arecore,@order_type,@order_genre,@order_boxId,@order_wmsId,@order_sourceCode,@order_getSite,@order_putSite,@order_cancel)", orderTask, transaction);
                }
            });
        }

        public List<OrderTask> QueryBufferOrderDNC()
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<OrderTask>("SELECT * FROM  mercedesbenzdb.`tb_order_create` WHERE order_genre='NDC'");
            });
        }

        public bool AutomationInsertNDC(WCSTask _issueTaskmsg, OrderType OrderType, OrderTaskType TaskType)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                string _order_getLocation = "";
                string _order_putLocation = "";
                string _order_getSite = "";
                string _order_putSite = "";
                if (OrderType == OrderType.NDC && TaskType == OrderTaskType.In)
                {
                    _order_getLocation = (_issueTaskmsg as boxAnnounceTaskmsg).s_station;
                    _order_putLocation = (_issueTaskmsg as boxAnnounceTaskmsg).location;
                }
                var orderTask = new
                {
                    order_guid = Guid.NewGuid().ToString(),
                    order_arecore = _issueTaskmsg.areaCode,
                    order_sourceCode = _issueTaskmsg.sourceCode,
                    order_ordernumber = $"BM{DateTime.Now.ToString("yyyyMMddHHmmssss")}{new Random().Next(0, 999999)}",
                    order_priority = _issueTaskmsg.priority,
                    order_status = -1,
                    order_utctime = UTC.ConvertDateTimeLong(DateTime.Now),
                    order_type = TaskType.ToString(),
                    order_carno = -1,
                    order_ikey = -1,
                    order_index = -1,
                    order_magic = -1,
                    order_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    order_genre = OrderType.ToString(),
                    order_boxId = _issueTaskmsg.boxId,
                    order_wmsId = TaskType == OrderTaskType.Platform ? _issueTaskmsg.id.ToString() : _issueTaskmsg.wmsId,
                    order_getLocation = _order_getLocation,
                    order_putLocation = _order_putLocation,
                    order_getSite = _order_getSite,
                    order_putSite = _order_putSite,
                    order_cancel=0
                };
                return connection.Executes("INSERT INTO mercedesbenzdb.`tb_order_create`(order_guid,order_ordernumber,order_getLocation,order_putLocation,order_priority,order_status,order_magic,order_ikey,order_index,order_carno,order_utctime,order_time,order_arecore,order_type,order_genre,order_boxId,order_wmsId,order_sourceCode,order_getSite,order_putSite,order_cancel) values (@order_guid,@order_ordernumber,@order_getLocation,@order_putLocation,@order_priority,@order_status,@order_magic,@order_ikey,@order_index,@order_carno,@order_utctime,@order_time,@order_arecore,@order_type,@order_genre,@order_boxId,@order_wmsId,@order_sourceCode,@order_getSite,@order_putSite,@order_cancel)", orderTask) > 0;
            });
        }

        /// <summary>
        ///根据index查询务订单
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <returns></returns>
        public OrderTask QueryOrderDNC(int order_index)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Query_FirstOrDefault<OrderTask>("SELECT * FROM  mercedesbenzdb.`tb_order` WHERE order_index=@order_index",
                    new { order_index = order_index });
            });
        }

        /// <summary>
        /// 查询NDC任务订单
        /// </summary>
        /// <param name="_orderTask"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<OrderTask> QueryOrderDNC()
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<OrderTask>("SELECT * FROM  mercedesbenzdb.`tb_order` WHERE order_genre='NDC'");
            });
        }


        /// <summary>
        /// 查询NDC任务订单 根据订单编号
        /// </summary>
        /// <param name="_orderTask"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public OrderTask QueryOrder(string order_ordernumber)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Query_FirstOrDefault<OrderTask>("SELECT * FROM  mercedesbenzdb.`tb_order` WHERE order_ordernumber=@order_ordernumber",
                    new { order_ordernumber = order_ordernumber });
            });
        }

        /// <summary>
        /// 根据index 更新magic
        /// </summary>
        /// <param name="order_magic"></param>
        /// <param name="order_index"></param>
        public void UpdateOrder_orderNumber_magic(int order_magic, string order_ordernumber)
        {
            DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_magic=@order_magic,order_utctime=@order_utctime WHERE order_ordernumber=@order_ordernumber",
                    new { order_magic = order_magic, order_ordernumber = order_ordernumber, order_utctime = UTC.ConvertDateTimeLong(DateTime.Now) });
            });
        }

        /// <summary>
        /// 根据WMSID查询任务订单
        /// </summary>
        /// <param name="order_carno"></param>
        /// <returns></returns>
        public OrderTask QueryOrderWMSID(string wmsId)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Query_FirstOrDefault<OrderTask>("SELECT * FROM  mercedesbenzdb.`tb_order` WHERE order_wmsId=@order_wmsId", new { order_wmsId = wmsId });
            });
        }

        /// <summary>
        /// 查询NDC任务订单 根据车号
        /// </summary>
        /// <param name="_orderTask"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<OrderTask> QueryOrderDNCCarno(int order_carno)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<OrderTask>("SELECT * FROM  mercedesbenzdb.`tb_order` WHERE order_genre='NDC'AND order_carno=@order_carno",
                    new { order_carno = order_carno });
            });
        }

        /// <summary>
        /// 根据订单编号删除任务订单
        /// </summary>
        /// <param name="_orderTask"></param>
        /// <returns></returns>
        public int DeleteOrderTask(string order_ordernumber)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("DELETE FROM mercedesbenzdb.`tb_order` WHERE order_ordernumber=@order_ordernumber", new { order_ordernumber = order_ordernumber });
            });
        }

        /// <summary>
        /// 根据订单编号更新车号 AGV任务订单
        /// </summary>
        /// <param name="order_ordernumber"></param>
        /// <param name="order_status"></param>
        /// <returns></returns>
        public int UpdateOrder_CarNo(string order_ordernumber, int order_carno)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_utctime=@order_utctime,order_carno=@order_carno WHERE order_ordernumber=@order_ordernumber",
                    new { order_ordernumber = order_ordernumber, order_carno = order_carno, order_utctime = UTC.ConvertDateTimeLong(DateTime.Now) });
            });
        }


        /// <summary>
        /// 查询AGV任务订单
        /// </summary>
        /// <param name="_orderTask"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<OrderTask> QueryOrderAGV()
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<OrderTask>("SELECT * FROM  mercedesbenzdb.`tb_order` WHERE order_genre='AGV'");
            });
        }

        /// <summary>
        /// 根据车号查询任务订单
        /// </summary>
        /// <param name="order_carno"></param>
        /// <returns></returns>
        public List<OrderTask> QueryOrderCarno(int order_carno)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<OrderTask>("SELECT * FROM  mercedesbenzdb.`tb_order` WHERE order_genre='AGV' AND order_carno=@order_carno", new { order_carno = order_carno });
            });
        }

        /// <summary>
        /// 根据车号查询任务订单
        /// </summary>
        /// <param name="order_carno"></param>
        /// <returns></returns>
        public OrderTask QueryOrderCarnoinfo(int order_carno)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Query_FirstOrDefault<OrderTask>("SELECT * FROM  mercedesbenzdb.`tb_order` WHERE order_genre='AGV' AND order_carno=@order_carno", new { order_carno = order_carno });
            });
        }

        /// <summary>
        /// 根据订单编号更新status
        /// </summary>
        /// <param name="order_ordernumber"></param>
        /// <param name="order_status"></param>
        /// <returns></returns>
        public int UpdateOrder_Status(string order_ordernumber, int order_status)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_utctime=@order_utctime,order_status=@order_status WHERE order_ordernumber=@order_ordernumber",
                    new { order_ordernumber = order_ordernumber, order_status = order_status, order_utctime = UTC.ConvertDateTimeLong(DateTime.Now) });
            });
        }

        /// <summary>
        ///插入状态更新消息
        /// </summary>
        /// <param name="orderTask"></param>
        /// <returns></returns>
        public int InsertMessage(UpdateOrderTask orderTask)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                var _orderTask = new
                {
                    mes_guid = Guid.NewGuid().ToString(),
                    mes_messageName = orderTask.messageName,
                    mes_areaCode = orderTask.areaCode,
                    mes_boxId = orderTask.boxId,
                    mes_sourceCode = orderTask.sourceCode,
                    mes_state = orderTask.state,
                    mes_wmsId = orderTask.wmsId,
                    mes_utctime = UTC.ConvertDateTimeLong(DateTime.Now),
                    mes_dateTime = DateTime.Now,
                    mes_Data = orderTask.ToJson(),
                    mes_Status = -1,
                    mes_ID = orderTask.id
                };
                return connection.Executes("INSERT INTO mercedesbenzdb.`tb_message` (mes_guid,mes_messageName,mes_areaCode,mes_sourceCode,mes_boxId,mes_wmsId,mes_utctime,mes_dateTime,mes_Data,mes_ID,mes_Status,mes_state) VALUES(@mes_guid,@mes_messageName,@mes_areaCode,@mes_sourceCode,@mes_boxId,@mes_wmsId,@mes_utctime,@mes_dateTime,@mes_Data,@mes_ID,@mes_Status,@mes_state)", _orderTask);
            });
        }

        public List<UpdateOrderTaskStatus> MessageList()
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<UpdateOrderTaskStatus>("SELECT * FROM mercedesbenzdb.`tb_message`");
            });
        }

        public int UpdateMessageTime(UpdateOrderTaskStatus orderTask)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_message` SET mes_utctime=@mes_utctime WHERE mes_guid=@mes_guid", new { mes_utctime = UTC.ConvertDateTimeLong(DateTime.Now), mes_guid = orderTask.mes_guid });
            });
        }

        public int UpdateMessage(ResponsemesUpdate responsemes)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_message` SET mes_utctime=@mes_utctime,mes_Status=@mes_Status WHERE mes_ID=@mes_ID", new { mes_utctime = UTC.ConvertDateTimeLong(DateTime.Now), mes_Status = 2, mes_ID = responsemes.id });
            });
        }

        public int CopyMessage(UpdateOrderTaskStatus orderTask)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, (connection, transaction) =>
            {
                connection.Executes("DELETE FROM mercedesbenzdb.`tb_message` WHERE mes_guid=@mes_guid", new { mes_guid = orderTask.mes_guid }, transaction);
                var _orderTask = new
                {
                    mes_guid = orderTask.mes_guid,
                    mes_messageName = orderTask.mes_messageName,
                    mes_areaCode = orderTask.mes_areaCode,
                    mes_boxId = orderTask.mes_boxId,
                    mes_sourceCode = orderTask.mes_sourceCode,
                    mes_state = orderTask.mes_state,
                    mes_wmsId = orderTask.mes_wmsId,
                    mes_utctime = orderTask.mes_utctime,
                    mes_dateTime = orderTask.mes_dateTime,
                    mes_Data = orderTask.mes_Data,
                    mes_Status = orderTask.mes_Status,
                    mes_ID = orderTask.mes_ID
                };
                return connection.Executes("INSERT INTO mercedesbenzdb.`tb_message_history` (mes_guid,mes_messageName,mes_areaCode,mes_sourceCode,mes_boxId,mes_wmsId,mes_utctime,mes_dateTime,mes_Data,mes_ID,mes_Status,mes_state) VALUES(@mes_guid,@mes_messageName,@mes_areaCode,@mes_sourceCode,@mes_boxId,@mes_wmsId,@mes_utctime,@mes_dateTime,@mes_Data,@mes_ID,@mes_Status,@mes_state)", _orderTask, transaction);
            });
        }

        /// <summary>
        /// 查询缓存区状态
        /// </summary>
        /// <returns></returns>
        public List<BufferInfo> QueryBufffer()
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<BufferInfo>("SELECT * FROM mercedesbenzdb.`tb_buffer`");
            });
        }


        public bool UpdateputSite(OrderTask _orderTask)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_putSite=@order_putSite WHERE  order_ordernumber=@order_ordernumber", _orderTask) > 0;
            });
        }

        public bool Updateorder_putLocation(OrderTask _orderTask)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_putLocation=@order_putLocation WHERE  order_ordernumber=@order_ordernumber", _orderTask) > 0;
            });
        }


        public bool UpdategetSite(OrderTask _orderTask)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_getSite=@order_getSite WHERE  order_ordernumber=@order_ordernumber", _orderTask) > 0;
            });
        }

        public bool UpdatemagicCarNo(int _order_magic, string orderNumber, int CarNumber)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_magic=@order_magic,order_carno=@order_carno WHERE  order_ordernumber=@order_ordernumber", new { order_magic = _order_magic, order_ordernumber = orderNumber, order_carno = CarNumber }) > 0;
            });
        }

        public bool UpdateBufferStatus(int _Buff_number, int _Buff_status)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_buffer` SET Buff_status=@Buff_status WHERE  Buff_number=@Buff_number", new { Buff_status = _Buff_status, Buff_number = _Buff_number }) > 0;
            });
        }


        public List<Stationconfiguration> QuerySiteConfiguration()
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<Stationconfiguration>("SELECT * FROM mercedesbenzdb.`tb_stationconfiguration`");
            });

        }



        public tb_agvsite_model Queryagvsite(int agvNumber)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Query_FirstOrDefault<tb_agvsite_model>("SELECT * FROM mercedesbenzdb.`tb_agvsite` WHERE site_agvnumber=@site_agvnumber", new { site_agvnumber = agvNumber });
            });
        }

        public bool cancelOrderTask(OrderTask _orderTask)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_order` SET order_cancel=@order_cancel WHERE  order_ordernumber=@order_ordernumber", _orderTask) > 0;
            });
        }


        #region 空开



        public bool UpdateSwitch(int Key, int _Switch)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringWebDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`switchtb` SET Switch=@Switch WHERE ID=@ID", new { Switch = _Switch, ID = Key }) > 0;
            });
        }

        public bool UpdateStatus(int Key, int _Status)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringWebDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`switchtb` SET Status=@Status WHERE ID=@ID", new { Status = _Status, ID = Key }) > 0;
            });
        }

        public List<switchInfo> QuerySwitchConfiguration()
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<switchInfo>("SELECT * FROM mercedesbenzdb.`switchtb`");
            });
        }


        #endregion

        public bool UpdateEnvironment(double temperature, double humidity)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Executes("UPDATE mercedesbenzdb.`tb_infoenvironment` SET info_temperature=@info_temperature,info_humidity=@info_humidity WHERE  info_id=@info_id", new { info_temperature = temperature, info_humidity = humidity, info_id = 1 }) > 0;
            });
        }

        public EnvironmentInfo QueryenvironmentInfos()
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.Query_FirstOrDefault<EnvironmentInfo>("SELECT * FROM mercedesbenzdb.`tb_infoenvironment` WHERE info_id=@info_id", new { info_id = 1 });
            });
        }


        public List<tb_carinfo_model> QueryCarInfos()
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<tb_carinfo_model>("SELECT * FROM mercedesbenzdb.`tb_carinfo`");
            });
        }


        public List<tb_warehousesstatus_model> QueryWarehouses()
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringDB, connection =>
            {
                return connection.FindList<tb_warehousesstatus_model>("SELECT * FROM mercedesbenzdb.`tb_warehousesstatus`");
            });
        }


        //更新AGV信息
        public bool tb_carinfo_Update(tb_carinfo_model parameter)
        {
            return DataBaseExecute.ExcuteSql(SystemConfiguration.ConfigStringWebDB, connection =>
            {
                int result = connection.Executes("update mercedesbenzdb.`tb_carinfo` set Info_voltage=@Info_voltage,Info_errormes=@Info_errormes,Info_status=@Info_status,Info_Ischarge=@Info_Ischarge,Info_Iserror=@Info_Iserror,Info_ThisSite=@Info_ThisSite where Info_number=@Info_number and Info_cartype=@Info_cartype", parameter);
                return result > 0;
            });
        }
    }
}
