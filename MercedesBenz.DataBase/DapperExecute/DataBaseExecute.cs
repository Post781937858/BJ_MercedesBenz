using System.Data;
using System;
using MySql.Data.MySqlClient;
using MercedesBenz.Infrastructure;

namespace MercedesBenz.DataBase.DapperExecute.DataBase
{
    /// <summary>
    /// 日 期：2019.5.02
    /// 描 述：获取数据库连接执行SQL
    /// </summary>
    public class DataBaseExecute
    {
        /// <summary>
        /// 从连接池获取SQL连接
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IDbConnection CreateConnection(string ConfigString)
        {
            return new MySqlConnection(ConfigString);
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="action"></param>
        public static void ExcuteSql(string ConfigString, Action<IDbConnection> action)
        {
            using (IDbConnection connection = CreateConnection(ConfigString))
            {
                try
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                    action.Invoke(connection);
                }
                catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
                finally
                {
                    connection.Close();
                }
            }
        }
        //


        /// <summary>
        /// 执行SQL语句(事务)
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="action"></param>
        public static void ExcuteSql(string ConfigString, Action<IDbConnection, IDbTransaction> action)
        {
            using (IDbConnection connection = CreateConnection(ConfigString))
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    action.Invoke(connection, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    Log4NetHelper.WriteErrorLog(ex.Message, ex);
                }
                finally { connection.Close(); }
            }
        }


        /// <summary>
        /// 执行SQL语句返回受影响行数
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="action"></param>
        public static int ExcuteSql(string ConfigString, Func<IDbConnection, int> action)
        {
            using (IDbConnection connection = CreateConnection(ConfigString))
            {
                try
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                    return action.Invoke(connection);
                }
                catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); return 0; }
                finally
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行SQL语句返回受影响行数(事务)
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="action"></param>
        public static int ExcuteSql(string ConfigString, Func<IDbConnection, IDbTransaction, int> action)
        {
            using (IDbConnection connection = CreateConnection(ConfigString))
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    var count = action.Invoke(connection, transaction);
                    transaction.Commit();
                    return count;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    Log4NetHelper.WriteErrorLog(ex.Message, ex);
                    return 0;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行SQL语句返回 T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionString"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static T ExcuteSql<T>(string ConfigString, Func<IDbConnection, T> action)
        {
            T obj = default(T);
            using (IDbConnection connection = CreateConnection(ConfigString))
            {
                try
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                    obj = action.Invoke(connection);
                }
                catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
                finally
                {
                    connection.Close();
                }
            }
            return obj;
        }

    }
}
