using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Threading.Tasks;

namespace DataAccessHelper
{
    public static class MyExtensions
    {
        /// <summary>
        /// 根据传入规则改变数据表的映射
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="mapping">映射规则</param>
        public static void ChangeTableMapping(this ModelBuilder builder, ICollection<TableMappingRule> mapping)
        {
            if (mapping != null)
            {
                string tableNm;
                foreach (var rule in mapping)
                {
                    tableNm = rule.Mapper.GetMappingTableName(rule.MappingType, rule.Condition);
                    builder.Entity(rule.MappingType).ToTable(tableNm);
                }
            }
        }

        /// <summary>
        /// 根据当前设置的映射规则获取数据库的建表语句
        /// </summary>
        /// <param name="accessor">DataAccessor</param>
        /// <returns>建表语句</returns>
        public static string GetTableCreateScript(this IDbAccessor accessor)
        {
            var context = accessor.GetDbContext();
            var dbCreator = context.GetService<IRelationalDatabaseCreator>();
            return dbCreator.GenerateCreateScript();
        }

        /// <summary>
        /// 执行SQL语句，参数用法可参考ExecuteSqlRawAsync方法
        /// </summary>
        /// <remarks>例:"update tbUser set Name='HaHa' where Id={0}"</remarks>
        /// <param name="accessor"></param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>受影响行数</returns>
        public static Task<int> ExecuteSqlRawAsync(this IDbAccessor accessor, string sql, params object[] parameters)
        {
            var context = accessor.GetDbContext();
            return context.Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}
