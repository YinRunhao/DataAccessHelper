using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessHelper
{
    /// <summary>
    /// 数据表映射规则接口
    /// </summary>
    public interface ITableMappable
    {
        /// <summary>
        /// 获取映射表名
        /// </summary>
        /// <typeparam name="T">条件类型</typeparam>
        /// <param name="modelType">分表的映射类型</param>
        /// <param name="condition">映射条件</param>
        /// <returns></returns>
        string GetMappingTableName(Type modelType, object condition);
    }
}
