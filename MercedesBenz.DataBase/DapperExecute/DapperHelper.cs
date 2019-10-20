using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MercedesBenz.DataBase.DapperExecute.DataBase
{
    /// <summary>
    /// 日 期：2019.5.02
    /// 描 述：Dapper数据访问层封装
    /// </summary>
    public static class DapperHelper
    {
        #region 同步方法

        /// <summary>
        /// 查询(带参数)
        /// </summary>
        /// <param name="sql">查询的sql</param>
        /// <param name="param">替换参数</param>
        /// <returns></returns>
        public static List<T> FindList<T>(this IDbConnection connection, string sql, object param) where T : class
        {
            return connection.Query<T>(sql, param).ToList();
        }


        /// <summary>
        /// 查询 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static List<T> FindList<T>(this IDbConnection connection, string sql) where T : class
        {
            var result = connection.Query<T>(sql);
            return result.ToList();
        }


        /// <summary>
        /// 查询第一个数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T Query_First<T>(this IDbConnection connection, string sql, object param) where T : class
        {
            return connection.QueryFirst<T>(sql, param);
        }

        /// <summary>
        /// 查询第一个数据没有返回默认值
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T Query_FirstOrDefault<T>(this IDbConnection connection, string sql, object param) where T : class
        {
            return connection.QueryFirstOrDefault<T>(sql, param);
        }


        /// <summary>
        /// 查询单条数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T Query_Single<T>(this IDbConnection connection, string sql, object param) where T : class
        {
            return connection.QuerySingle<T>(sql, param);
        }

        /// <summary>
        /// 查询单条数据没有返回默认值
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T Query_SingleOrDefault<T>(this IDbConnection connection, string sql, object param) where T : class
        {
            return connection.QuerySingleOrDefault<T>(sql, param);
        }

        /// <summary>
        /// 增删改
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int Executes(this IDbConnection connection, string sql, object param, IDbTransaction transaction = null)
        {
            return connection.Execute(sql, param, transaction);
        }

        /// <summary>
        /// 批量 增删改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int Batch_Executes<T>(this IDbConnection connection, string sql, List<T> param, IDbTransaction transaction = null) where T : class
        {
            return connection.Execute(sql, param, transaction);
        }


        /// <summary>
        /// Reader获取数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IDataReader Execute_Reader(this IDbConnection connection, string sql, object param)
        {
            return connection.ExecuteReader(sql, param);
        }

        /// <summary>
        /// Scalar获取数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static object Execute_Scalar(this IDbConnection connection, string sql, object param)
        {
            return connection.ExecuteScalar(sql, param);
        }

        /// <summary>
        /// Scalar获取数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T Execute_ScalarForT<T>(this IDbConnection connection, string sql, object param) where T : class
        {
            return connection.ExecuteScalar<T>(sql, param);
        }

        /// <summary>
        /// 带参数的存储过程
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<T> ExecutePro<T>(this IDbConnection connection, string proc, object param) where T : class
        {

            List<T> list = connection.Query<T>(proc,
                param,
                null,
                true,
                null,
                CommandType.StoredProcedure).ToList();
            return list;
        }


        /// <summary>
        /// 获取事务对象
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static IDbTransaction GetDbTransaction(this IDbConnection connection)
        {
            return connection.BeginTransaction();
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        /// <param name="transaction"></param>
        public static void RollbackDB(this IDbTransaction transaction)
        {
            transaction.Rollback();
            transaction.Dispose();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="transaction"></param>
        public static void CommitDB(this IDbTransaction transaction)
        {
            transaction.Commit();
        }
    }

    #endregion
}
