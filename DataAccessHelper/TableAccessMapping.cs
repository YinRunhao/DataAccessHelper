using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessHelper
{
    /// <summary>
    /// 数据表映射结构体
    /// </summary>
    public struct TableAccessMapping
    {
        /// <summary>
        /// 实体类
        /// </summary>
        public Type MappingType
        {
            get; private set;
        }

        /// <summary>
        /// 数据表名
        /// </summary>
        public string TableName
        {
            get; private set;
        }

        public TableAccessMapping(Type mappingType, string tableName)
        {
            this.MappingType = mappingType;
            this.TableName = tableName;
        }
    }
}
