using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using Dapper;
using System.Data.Common;
using System.Threading.Tasks;

namespace DataAccessHelper.Extensions.Dapper
{
    /// <summary>
    /// 使用Dapper实现原生SQL查询
    /// </summary>
    public static class DapperExtensions
    {
        /// <summary>
        /// 使用Dapper执行原生SQL查询，返回单个结果集
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="accessor"></param>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> SqlQueryAsync<T>(this IDbAccessor accessor, string sql, object param = null)
        {
            var context = accessor.GetDbContext();
            DbConnection conn = context.Database.GetDbConnection();
            DbTransaction tran = default;
            var efTran = context.Database.CurrentTransaction;
            if (efTran != null)
            {
                tran = efTran.GetDbTransaction();
            }

            var ret = await conn.QueryAsync<T>(sql, param, tran);
            return ret;
        }

        /// <summary>
        /// 使用Dapper执行原生SQL查询，返回多个结果集
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static async Task<SqlMapper.GridReader> SqlQueryMultipleAsync(this IDbAccessor accessor, string sql, object param = null)
        {
            var context = accessor.GetDbContext();
            DbConnection conn = context.Database.GetDbConnection();
            DbTransaction tran = default;
            var efTran = context.Database.CurrentTransaction;
            if (efTran != null)
            {
                tran = efTran.GetDbTransaction();
            }

            return await conn.QueryMultipleAsync(sql, param, tran);
        }
    }
}
