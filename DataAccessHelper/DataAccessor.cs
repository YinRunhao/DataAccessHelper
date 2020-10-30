using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccessHelper
{
    /// <summary>
    /// .NET EF Core框架帮助类
    /// </summary>
    public class DataAccessor : IDbAccessor
    {
        /// <summary>
        /// 更改映射时的互斥锁
        /// </summary>
        private static object LockObj = new object();

        private BaseDataAccessor BaseAccessor = new BaseDataAccessor();

        /// <summary>
        /// 使用该类前必须调用此方法指明DbContext类型，若传入的类型不是继承于DbContext，将冒ArgumentException
        /// </summary>
        /// <param name="t">Context类型</param>
        /// <exception cref="ArgumentException">context 类型设置错误</exception>
        public static void SetContextType(Type t)
        {
            BaseDataAccessor.SetContextType(t);
        }

        #region IMappingMutable接口实现
        /// <summary>
        /// 更换数据库, 数据表映射使用默认映射规则
        /// </summary>
        /// <param name="connStr">新的数据库连接字符串</param>
        public void ChangeDataBase(string connStr)
        {
            // close
            var accessor = BaseAccessor;
            if (!accessor.IsClose())
            {
                accessor.Close();
            }
            // new base accessor
            accessor = new BaseDataAccessor();
            accessor.GetDbContext().Database.GetDbConnection().ConnectionString = connStr;
            BaseAccessor = accessor;
        }

        /// <summary>
        /// 更换数据库, 数据表映射使用传入的映射规则
        /// </summary>
        /// <param name="connStr">新的数据库连接字符串</param>
        /// <param name="rules">映射规则</param>
        /// <exception cref="ArgumentException">type类型不支持</exception>
        public void ChangeDataBase(string connStr, List<TableMappingRule> rules)
        {
            // close
            var accessor = BaseAccessor;
            if (!accessor.IsClose())
            {
                accessor.Close();
            }
            lock (LockObj)
            {
                // new base accessor
                accessor = new BaseDataAccessor(rules);
                // notity new mapping
                DynamicModelCacheKeyFactory.ChangeTableMapping();
                accessor.GetDbContext().Database.GetDbConnection().ConnectionString = connStr;
                BaseAccessor = accessor;
            }
        }

        /// <summary>
        /// 根据条件改变多个传入类型的映射数据表(此操作会导致当前操作的context释放掉，调用前需确保context的内容已保存)
        /// </summary>
        /// <param name="rules">映射规则</param>
        /// <exception cref="ArgumentException">type类型不支持</exception>
        /// <returns>改变后的数据表映射</returns>
        public List<TableAccessMapping> ChangeMappingTables(List<TableMappingRule> rules)
        {
            List<TableAccessMapping> ret = new List<TableAccessMapping>();
            if (rules != null)
            {
                // close old accessor
                var accessor = BaseAccessor;
                if (!accessor.IsClose())
                {
                    accessor.Close();
                }
                lock (LockObj)
                {
                    // new accessor
                    accessor = new BaseDataAccessor(rules);
                    // notity new mapping
                    DynamicModelCacheKeyFactory.ChangeTableMapping();
                    this.BaseAccessor = accessor;
                }
                foreach (var rule in rules)
                {
                    var mapping = GetTableName(rule.MappingType, accessor);
                    ret.Add(mapping);
                }
            }

            return ret;
        }

        /// <summary>
        /// 根据条件改变传入类型的映射数据表(此操作会导致当前操作的context释放掉，调用前需确保context的内容已保存)
        /// </summary>
        /// <typeparam name="T">条件类型</typeparam>
        /// <param name="type">要改变映射的实体类型</param>
        /// <param name="condition">改变条件</param>
        /// <exception cref="ArgumentException">type类型不支持</exception>
        /// <returns>改变后的数据表映射</returns>
        public TableAccessMapping ChangeMappingTable(Type type, ITableMappable mapper, object condition)
        {
            TableMappingRule rule = default(TableMappingRule);
            rule.MappingType = type;
            rule.Mapper = mapper;
            rule.Condition = condition;

            List<TableMappingRule> param = new List<TableMappingRule> { rule };
            var result = ChangeMappingTables(param);
            return result[0];
        }

        /// <summary>
        /// 获取所有实体类的数据表映射结构体
        /// </summary>
        /// <returns>映射关系集合</returns>
        public List<TableAccessMapping> GetTableNames()
        {
            BaseDataAccessor helper = BaseAccessor;
            var context = helper.GetDbContext();

            List<TableAccessMapping> ret = new List<TableAccessMapping>();
            var models = context.Model.GetEntityTypes();
            foreach (var model in models)
            {
                string table = model.GetTableName();
                Type type = model.ClrType;
                TableAccessMapping mapping = new TableAccessMapping(type, table);
                ret.Add(mapping);
            }

            return ret;
        }

        /// <summary>
        /// 获取指定实体类的数据表映射结构体
        /// </summary>
        /// <param name="mappingType">实体类性</param>
        /// <returns>传入实体类的映射关系</returns>
        public TableAccessMapping GetTableName(Type mappingType)
        {
            BaseDataAccessor helper = BaseAccessor;
            var context = helper.GetDbContext();
            var model = context.Model.FindEntityType(mappingType);

            if (model != null)
            {
                string table = model.GetTableName();
                return new TableAccessMapping(mappingType, table);
            }
            else
            {
                throw new ArgumentException("Mapping type not found");
            }
        }
        #endregion

        #region IDataAccessor接口实现
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public T Add<T>(T model) where T : class
        {
            return BaseAccessor?.Add<T>(model);
        }

        /// <summary>
        /// 插入数据并保存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task<T> AddAsync<T>(T model) where T : class
        {
            return BaseAccessor?.AddAsync<T>(model);
        }

        /// <summary>
        /// 向上下文增加记录，但不保存，需要手动调用Save
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public void AddRecord<T>(T model) where T : class
        {
            BaseAccessor?.AddRecord<T>(model);
        }

        /// <summary>
        /// 获取一个事务对象
        /// </summary>
        /// <returns></returns>
        public IDbContextTransaction BeginTransaction()
        {
            return BaseAccessor?.BeginTransaction();
        }

        /// <summary>
        /// 异步获取一个事务对象
        /// </summary>
        /// <returns></returns>
        public Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return BaseAccessor?.BeginTransactionAsync();
        }

        /// <summary>
        /// 执行存储过程，返回存储过程中返回的数据表
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据表</returns>
        public List<DataTable> CallProcedure(string procName, params DbParameter[] parameters)
        {
            return BaseAccessor?.CallProcedure(procName, parameters);
        }

        /// <summary>
        /// 执行存储过程，返回存储过程中返回的数据表
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据表</returns>
        public Task<List<DataTable>> CallProcedureAsync(string procName, params DbParameter[] parameters)
        {
            return BaseAccessor?.CallProcedureAsync(procName, parameters);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            BaseAccessor?.Close();
        }

        /// <summary>
        /// 按主键标记实体删除(即使传入的实体不是被追踪的实体，同主键的追踪实体依然会标记删除删除)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public void Delete<T>(T model) where T : class
        {
            BaseAccessor?.Delete<T>(model);
        }

        /// <summary>
        /// 获取某个表的所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> GetAll<T>() where T : class
        {
            return BaseAccessor?.GetAll<T>();
        }

        /// <summary>
        /// 根据主键值来查询某条记录，单主键的表可直接输入主键，多主键的表注意主键次序
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="values">主键值</param>
        /// <returns>返回主键值为传入值的实体</returns>
        public T GetByID<T>(params object[] values) where T : class
        {
            return BaseAccessor?.GetByID<T>(values);
        }

        /// <summary>
        /// 根据主键值来查询某条记录，单主键的表可直接输入主键，多主键的表注意主键次序
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="values">主键值</param>
        /// <returns>返回主键值为传入值的实体</returns>
        public Task<T> GetByIDAsync<T>(params object[] values) where T : class
        {
            return BaseAccessor?.GetByIDAsync<T>(values);
        }

        /// <summary>
        /// 获取条目数
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns>条目数</returns>
        public int GetCount<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return BaseAccessor.GetCount<T>(expression);
        }

        /// <summary>
        /// 获取条目数
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns>条目数</returns>
        public Task<int> GetCountAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return BaseAccessor.GetCountAsync(expression);
        }

        /// <summary>
        /// 根据表达式进行查询返回结果
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns></returns>
        public IQueryable<T> GetMany<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return BaseAccessor.GetMany<T>(expression);
        }

        /// <summary>
        /// 在数据库中进行分页查询
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="pageSize">每页条数</param>
        /// <param name="pageIdx">当前页</param>
        /// <param name="expression">查询表达式</param>
        /// <returns></returns>
        public List<T> GetPageList<T, Tkey>(int pageSize, int pageIdx, Expression<Func<T, bool>> expression, Func<T, Tkey> orderExpression) where T : class
        {
            return BaseAccessor?.GetPageList(pageSize, pageIdx, expression, orderExpression);
        }

        /// <summary>
        /// 在数据库中进行分页查询
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="pageSize">页大小</param>
        /// <param name="pageIdx">页下标</param>
        /// <param name="expression">查询表达式</param>
        /// <param name="orderExpression">排序表达式</param>
        /// <returns>分页查询结果</returns>
        public Task<List<T>> GetPageListAsync<T, Tkey>(int pageSize, int pageIdx, Expression<Func<T, bool>> expression, Func<T, Tkey> orderExpression) where T : class
        {
            return BaseAccessor?.GetPageListAsync(pageSize, pageIdx, expression, orderExpression);
        }

        /// <summary>
        /// 是否已关闭
        /// </summary>
        /// <returns></returns>
        public bool IsClose()
        {
            return BaseAccessor.IsClose();
        }

        /// <summary>
        /// 根据某个字段的值进行排序
        /// </summary>
        /// <typeparam name="T">排序后获得的集合的类型</typeparam>
        /// <typeparam name="TKey">排序字段的类型</typeparam>
        /// <param name="orderExpression">字段表达式 如：basicInfo下根据caseID排序（s=>s.caseID）</param>
        /// <param name="isASC">是否升序</param>
        /// <returns></returns>
        public IEnumerable<T> Order<T, TKey>(Func<T, TKey> orderExpression, bool isASC = false) where T : class
        {
            return BaseAccessor?.Order(orderExpression, isASC);
        }

        /// <summary>
        /// 提交对数据进行的处理，如无处理返回-1
        /// </summary>
        /// <returns></returns>
        public int Save()
        {
            return BaseAccessor.Save();
        }

        /// <summary>
        /// 提交对数据进行的处理，如无处理返回-1
        /// </summary>
        /// <returns></returns>
        public Task<int> SaveAsync()
        {
            return BaseAccessor.SaveAsync();
        }

        /// <summary>
        /// 更新操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public void Update<T>(T model) where T : class
        {
            BaseAccessor?.Update<T>(model);
        }

        /// <summary>
        /// 根据主键更新指定字段，使用时需要注意EFCore的仓储机制可能会使数据库和缓存仓的数据不一致。
        /// </summary>
        /// <typeparam name="T">映射类</typeparam>
        /// <typeparam name="TProperty">字段类型</typeparam>
        /// <param name="model">更新的实体</param>
        /// <param name="property">要更新的属性, VisualStudio在这里有Bug, 不能智能显示类型属性, 但不影响使用</param>
        /// <returns></returns>
        public void Update<T>(T model, Expression<Func<T, object>> property) where T : class
        {
            BaseAccessor?.Update(model, property);
        }

        /// <summary>
        /// 获取EFCore的DbContext
        /// </summary>
        /// <returns>DbContext</returns>
        public DbContext GetDbContext()
        {
            return BaseAccessor?.GetDbContext();
        }
        #endregion

        #region IDisposable接口实现
        /// <summary>
        /// 释放EFCore的DbContext
        /// </summary>
        public void Dispose()
        {
            if (BaseAccessor != null)
            {
                if (!BaseAccessor.IsClose())
                {
                    BaseAccessor.Close();
                }
            }
        }
        #endregion

        private TableAccessMapping GetTableName(Type mappingType, BaseDataAccessor helper)
        {
            var context = helper.GetDbContext();
            var model = context.Model.FindEntityType(mappingType);

            if (model != null)
            {
                string table = model.GetTableName();
                return new TableAccessMapping(mappingType, table);
            }
            else
            {
                throw new ArgumentException("Mapping type not found");
            }
        }
    }
}
