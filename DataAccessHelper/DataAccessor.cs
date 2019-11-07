using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

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

    /// <summary>
    /// 数据表映射结构体
    /// </summary>
    public struct TableAcccessMapping
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

        public TableAcccessMapping(Type mappingType, string tableName)
        {
            this.MappingType = mappingType;
            this.TableName = tableName;
        }
    }

    /// <summary>
    /// .NET EF Core框架帮助类
    /// </summary>
    public class DataAccessor : IDataAccessor
    {
        private static object LockObj = new object();

        protected ThreadLocal<BaseDataAccessor> BaseAccessor = new ThreadLocal<BaseDataAccessor>(() => new BaseDataAccessor());

        /// <summary>
        /// 更换数据库
        /// </summary>
        /// <param name="connectString">新的数据库连接字符串</param>
        public void ChangeDataBase(string connectString)
        {
            // close
            var accessor = BaseAccessor.Value;
            if (!accessor.IsClose())
            {
                accessor.Close();
            }
            // new base accessor
            accessor = new BaseDataAccessor();
            accessor.GetDbContext().Database.GetDbConnection().ConnectionString = connectString;
            BaseAccessor.Value = accessor;
        }

        /// <summary>
        /// 根据条件改变多个传入类型的映射数据表(此操作会导致当前操作的context释放掉，调用前需确保context的内容已保存)
        /// </summary>
        /// <param name="rules">映射规则</param>
        /// <exception cref="ArgumentException">type类型不支持</exception>
        /// <returns>改变后的数据表映射</returns>
        public List<TableAcccessMapping> ChangeMappingTables(List<TableMappingRule> rules)
        {
            List<TableAcccessMapping> ret = new List<TableAcccessMapping>();
            if (rules != null)
            {
                lock (LockObj)
                {
                    // close
                    var accessor = BaseAccessor.Value;
                    if (!accessor.IsClose())
                    {
                        accessor.Close();
                    }
                    // new base accessor
                    accessor = new BaseDataAccessor();

                    // set new mapping
                    DynamicModelCacheKeyFactory.ChangeTableMapping();

                    var context = accessor.GetDbContext();

                    var types = context.Model.GetEntityTypes();
                    foreach (var t in types)
                    {
                        Type refType = t.ClrType;
                        
                    }

                    foreach (var rule in rules)
                    {
                        // check changeable
                        var mapping = GetTableName(rule.MappingType, accessor);

                        // get table name
                        string tableNm = rule.Mapper.GetMappingTableName(rule.MappingType, rule.Condition);
                        if (context.Model.FindEntityType(rule.MappingType)?.Relational() is RelationalEntityTypeAnnotations relational)
                        {
                            relational.TableName = tableNm;
                        }
                        else
                        {
                            tableNm = "";
                        }
                        ret.Add(new TableAcccessMapping(rule.MappingType, tableNm));
                    }
                    // set value
                    BaseAccessor.Value = accessor;
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
        public TableAcccessMapping ChangeMappingTable(Type type, ITableMappable mapper, object condition)
        {
            TableMappingRule rule = default;
            rule.MappingType = type;
            rule.Mapper =mapper;
            rule.Condition = condition;

            List<TableMappingRule> param = new List<TableMappingRule> { rule };
            var result = ChangeMappingTables(param);
            return result[0];
        }

        /// <summary>
        /// 获取所有实体类的数据表映射结构体
        /// </summary>
        /// <returns>映射关系集合</returns>
        public List<TableAcccessMapping> GetTableNames()
        {
            BaseDataAccessor helper = BaseAccessor.Value;
            var context = helper.GetDbContext();

            List<TableAcccessMapping> ret = new List<TableAcccessMapping>();
            var models = context.Model.GetEntityTypes();
            foreach (var model in models)
            {
                string table = model.Relational().TableName;
                Type type = model.ClrType;
                TableAcccessMapping mapping = new TableAcccessMapping(type, table);
                ret.Add(mapping);
            }

            return ret;
        }

        /// <summary>
        /// 获取指定实体类的数据表映射结构体
        /// </summary>
        /// <param name="mappingType">实体类性</param>
        /// <returns>传入实体类的映射关系</returns>
        public TableAcccessMapping GetTableName(Type mappingType)
        {
            BaseDataAccessor helper = BaseAccessor.Value;
            var context = helper.GetDbContext();
            var model = context.Model.FindEntityType(mappingType);

            if (model != null)
            {
                string table = model.Relational().TableName;
                return new TableAcccessMapping(mappingType, table);
            }
            else
            {
                throw new ArgumentException("Mapping type not found");
            }
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public T Add<T>(T model) where T : class
        {
            return BaseAccessor.Value?.Add<T>(model);
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task<T> AddAsync<T>(T model) where T : class
        {
            return BaseAccessor.Value?.AddAsync<T>(model);
        }

        /// <summary>
        /// 向上下文增加记录，但不保存，需要手动调用Commit
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool AddRecord<T>(T model) where T : class
        {
            if (BaseAccessor.Value == null)
            {
                return false;
            }

            return BaseAccessor.Value.AddRecord<T>(model);
        }

        /// <summary>
        /// 获取一个事务对象
        /// </summary>
        /// <returns></returns>
        public IDbContextTransaction BeginTransaction()
        {
            return BaseAccessor.Value?.BeginTransaction();
        }

        /// <summary>
        /// 异步获取一个事务对象
        /// </summary>
        /// <returns></returns>
        public Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return BaseAccessor.Value?.BeginTransactionAsync();
        }

        /// <summary>
        /// 执行存储过程，返回存储过程中返回的数据表
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据表</returns>
        public DataTable CallProcedure(string procName, params DbParameter[] parameters)
        {
            return BaseAccessor.Value?.CallProcedure(procName, parameters);
        }

        /// <summary>
        /// 执行存储过程，返回存储过程中返回的数据表
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据表</returns>
        public Task<DataTable> CallProcedureAsync(string procName, params DbParameter[] parameters)
        {
            return BaseAccessor.Value?.CallProcedureAsync(procName, parameters);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            BaseAccessor.Value?.Close();
        }

        /// <summary>
        /// 删除操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete<T>(T model) where T : class
        {
            if (BaseAccessor.Value == null)
            {
                return false;
            }

            return BaseAccessor.Value.Delete<T>(model);
        }

        /// <summary>
        /// 获取某个表的所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> GetAll<T>() where T : class
        {
            return BaseAccessor.Value?.GetAll<T>();
        }

        /// <summary>
        /// 根据主键值来查询某条记录，单主键的表可直接输入主键，多主键的表注意主键次序
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="values">主键值</param>
        /// <returns>返回主键值为传入值的实体</returns>
        public T GetByID<T>(params object[] values) where T : class
        {
            return BaseAccessor.Value?.GetByID<T>(values);
        }

        /// <summary>
        /// 根据主键值来查询某条记录，单主键的表可直接输入主键，多主键的表注意主键次序
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="values">主键值</param>
        /// <returns>返回主键值为传入值的实体</returns>
        public Task<T> GetByIDAsync<T>(params object[] values) where T : class
        {
            return BaseAccessor.Value?.GetByIDAsync<T>(values);
        }

        /// <summary>
        /// 获取条目数
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns>条目数</returns>
        public int GetCount<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return BaseAccessor.Value.GetCount<T>(expression);
        }

        /// <summary>
        /// 获取条目数
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns>条目数</returns>
        public Task<int> GetCountAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return BaseAccessor.Value.GetCountAsync(expression);
        }

        /// <summary>
        /// 根据表达式进行查询返回结果
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns></returns>
        public IQueryable<T> GetMany<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return BaseAccessor.Value.GetMany<T>(expression);
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
            return BaseAccessor.Value?.GetPageList(pageSize, pageIdx, expression, orderExpression);
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
            return BaseAccessor.Value?.GetPageListAsync(pageSize, pageIdx, expression, orderExpression);
        }

        /// <summary>
        /// 是否已关闭
        /// </summary>
        /// <returns></returns>
        public bool IsClose()
        {
            return BaseAccessor.Value.IsClose();
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
            return BaseAccessor.Value?.Order(orderExpression, isASC);
        }

        /// <summary>
        /// 提交对数据进行的处理，如无处理返回-1
        /// </summary>
        /// <returns></returns>
        public int Save()
        {
            return BaseAccessor.Value.Save();
        }

        /// <summary>
        /// 提交对数据进行的处理，如无处理返回-1
        /// </summary>
        /// <returns></returns>
        public Task<int> SaveAsync()
        {
            return BaseAccessor.Value.SaveAsync();
        }

        /// <summary>
        /// 更新操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update<T>(T model) where T : class
        {
            return BaseAccessor.Value.Update<T>(model);
        }

        private TableAcccessMapping GetTableName(Type mappingType, BaseDataAccessor helper)
        {
            var context = helper.GetDbContext();
            var model = context.Model.FindEntityType(mappingType);

            if (model != null)
            {
                string table = model.Relational().TableName;
                return new TableAcccessMapping(mappingType, table);
            }
            else
            {
                throw new ArgumentException("Mapping type not found");
            }
        }
    }
}
