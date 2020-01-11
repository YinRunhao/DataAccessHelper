using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessHelper
{
    /// <summary>
    /// 数据表映射规则结构体
    /// </summary>
    public struct TableMappingRule
    {
        /// <summary>
        /// 需要重新映射的类
        /// </summary>
        public Type MappingType
        {
            get; set;
        }

        /// <summary>
        /// 映射功能提供者
        /// </summary>
        public ITableMappable Mapper
        {
            get; set;
        }

        /// <summary>
        /// 映射条件（供映射功能提供者生成表名）
        /// </summary>
        public object Condition
        {
            get; set;
        }
    }
}
