using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

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
        public static string GetTableCreateScript(this DataAccessor accessor)
        {
            var context = accessor.GetDbContext();
            var dbCreator = context.GetService<IRelationalDatabaseCreator>();
            return dbCreator.GenerateCreateScript();
        }
    }
}
