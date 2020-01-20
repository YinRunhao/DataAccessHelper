using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccessHelper
{
    /// <summary>
    /// 数据接入层接口
    /// </summary>
    public interface IDataAccessor
    {
        /// <summary>
        /// 是否已关闭
        /// </summary>
        /// <returns></returns>
        bool IsClose();

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();

        /// <summary>
        /// 获取某个表的所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryable<T> GetAll<T>() where T : class;

        /// <summary>
        /// 根据表达式进行查询返回结果
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns></returns>
        IQueryable<T> GetMany<T>(Expression<Func<T, bool>> expression) where T : class;

        /// <summary>
        /// 根据主键值来查询某条记录，单主键的表可直接输入主键，多主键的表注意主键次序
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="values">主键值</param>
        /// <returns>返回主键值为传入值的实体</returns>
        T GetByID<T>(params object[] values) where T : class;

        /// <summary>
        /// 根据主键值来查询某条记录，单主键的表可直接输入主键，多主键的表注意主键次序
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="values">主键值</param>
        /// <returns>返回主键值为传入值的实体</returns>
        Task<T> GetByIDAsync<T>(params object[] values) where T : class;

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        T Add<T>(T model) where T : class;

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<T> AddAsync<T>(T model) where T : class;

        /// <summary>
        /// 向上下文增加记录，但不保存，需要手动调用Commit
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        bool AddRecord<T>(T model) where T : class;

        /// <summary>
        /// 删除操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Delete<T>(T model) where T : class;

        /// <summary>
        /// 更新操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Update<T>(T model) where T : class;

        /// <summary>
        /// 根据某个字段的值进行排序
        /// </summary>
        /// <typeparam name="T">排序后获得的集合的类型</typeparam>
        /// <typeparam name="TKey">排序字段的类型</typeparam>
        /// <param name="orderExpression">字段表达式 如：basicInfo下根据caseID排序（s=>s.caseID）</param>
        /// <param name="isASC">是否升序</param>
        /// <returns></returns>
        IEnumerable<T> Order<T, TKey>(Func<T, TKey> orderExpression, bool isASC = false) where T : class;

        /// <summary>
        /// 在数据库中进行分页查询
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="pageSize">每页条数</param>
        /// <param name="pageIdx">当前页</param>
        /// <param name="expression">查询表达式</param>
        /// <returns></returns>
        List<T> GetPageList<T, Tkey>(int pageSize, int pageIdx, Expression<Func<T, bool>> expression, Func<T, Tkey> orderExpression) where T : class;

        /// <summary>
        /// 在数据库中进行分页查询
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="pageSize">页大小</param>
        /// <param name="pageIdx">页下标</param>
        /// <param name="expression">查询表达式</param>
        /// <param name="orderExpression">排序表达式</param>
        /// <returns>分页查询结果</returns>
        Task<List<T>> GetPageListAsync<T, Tkey>(int pageSize, int pageIdx, Expression<Func<T, bool>> expression, Func<T, Tkey> orderExpression) where T : class;

        /// <summary>
        /// 获取条目数
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns>条目数</returns>
        int GetCount<T>(Expression<Func<T, bool>> expression) where T : class;

        /// <summary>
        /// 获取条目数
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns>条目数</returns>
        Task<int> GetCountAsync<T>(Expression<Func<T, bool>> expression) where T : class;

        /// <summary>
        /// 执行存储过程，返回存储过程中返回的数据表
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据表</returns>
        DataTable CallProcedure(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程，返回存储过程中返回的数据表
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据表</returns>
        Task<DataTable> CallProcedureAsync(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 提交对数据进行的处理，如无处理返回-1
        /// </summary>
        /// <returns></returns>
        int Save();

        /// <summary>
        /// 提交对数据进行的处理，如无处理返回-1
        /// </summary>
        /// <returns></returns>
        Task<int> SaveAsync();

        /// <summary>
        /// 异步获取一个事务对象
        /// </summary>
        /// <returns></returns>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// 获取一个事务对象
        /// </summary>
        /// <returns></returns>
        IDbContextTransaction BeginTransaction();
    }
}
