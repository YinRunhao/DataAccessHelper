using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessHelper
{
    /// <summary>
    /// 数据库可变映射
    /// </summary>
    public interface IMappingMutable
    {
        /// <summary>
        /// 更换数据库
        /// </summary>
        /// <param name="connectString">新的数据库连接字符串</param>
        void ChangeDataBase(string connString);

        /// <summary>
        /// 更换数据库, 数据表映射使用传入的映射规则
        /// </summary>
        /// <param name="connStr">新的数据库连接字符串</param>
        /// <param name="rules">映射规则</param>
        /// <exception cref="ArgumentException">type类型不支持</exception>
        void ChangeDataBase(string connStr, List<TableMappingRule> rules);

        /// <summary>
        /// 根据条件改变多个传入类型的映射数据表(此操作会导致当前操作的context释放掉，调用前需确保context的内容已保存)
        /// </summary>
        /// <param name="rules">映射规则</param>
        /// <exception cref="ArgumentException">type类型不支持</exception>
        /// <returns>改变后的数据表映射</returns>
        List<TableAccessMapping> ChangeMappingTables(List<TableMappingRule> rules);

        /// <summary>
        /// 根据条件改变传入类型的映射数据表(此操作会导致当前操作的context释放掉，调用前需确保context的内容已保存)
        /// </summary>
        /// <typeparam name="T">条件类型</typeparam>
        /// <param name="type">要改变映射的实体类型</param>
        /// <param name="condition">改变条件</param>
        /// <exception cref="ArgumentException">type类型不支持</exception>
        /// <returns>改变后的数据表映射</returns>
        TableAccessMapping ChangeMappingTable(Type type, ITableMappable mapper, object condition);

        /// <summary>
        /// 获取所有实体类的数据表映射结构体
        /// </summary>
        /// <returns>映射关系集合</returns>
        List<TableAccessMapping> GetTableNames();

        /// <summary>
        /// 获取指定实体类的数据表映射结构体
        /// </summary>
        /// <param name="mappingType">实体类性</param>
        /// <returns>传入实体类的映射关系</returns>
        TableAccessMapping GetTableName(Type mappingType);
    }
}
