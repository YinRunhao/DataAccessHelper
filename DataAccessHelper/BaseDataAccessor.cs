using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccessHelper
{
    /// <summary>
    /// .NET EF Core框架帮助基础类
    /// </summary>
    internal class BaseDataAccessor : IEFAvailable
    {
        private ExtendDbContext context;

        public static BaseDataAccessor Create(Type tp, ICollection<TableMappingRule> rules)
        {
            ExtendDbContext ctx = Activator.CreateInstance(tp, rules) as ExtendDbContext;
            BaseDataAccessor ret = new BaseDataAccessor(ctx);
            return ret;
        }

        public static BaseDataAccessor Create(Type tp)
        {
            ExtendDbContext ctx = Activator.CreateInstance(tp) as ExtendDbContext;
            BaseDataAccessor ret = new BaseDataAccessor(ctx);
            return ret;
        }

        /// <summary>
        /// 创建Helper对象
        /// </summary>
        /// <param name="ctx">EFCore DbContext</param>
        private BaseDataAccessor(ExtendDbContext ctx)
        {
            context = ctx;
        }

        /// <summary>
        /// 是否已关闭
        /// </summary>
        /// <returns></returns>
        public bool IsClose()
        {
            return context == null;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (context != null)
            {
                context.Dispose();
                context = null;
            }
        }

        /// <summary>
        /// 获取某个表的Queryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> GetQueryable<T>() where T : class
        {
            var query = context.Set<T>().AsQueryable();
            return query;
        }

        /// <summary>
        /// 获取某个表的所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> GetAll<T>() where T : class
        {
            var result = context.Set<T>().Where(s => true);
            return result;
        }

        /// <summary>
        /// 根据表达式进行查询返回结果
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns></returns>
        public IQueryable<T> GetMany<T>(Expression<Func<T, bool>> expression) where T : class
        {
            // var result = context.Set<T>().AsExpandable().Where(expression);
            var result = context.Set<T>().Where(expression);
            return result;
        }

        /// <summary>
        /// 根据主键值来查询某条记录，单主键的表可直接输入主键，多主键的表注意主键次序
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="values">主键值</param>
        /// <returns>返回主键值为传入值的实体</returns>
        public T GetByID<T>(params object[] values) where T : class
        {
            return context.Set<T>().Find(values);
        }

        /// <summary>
        /// 根据主键值来查询某条记录，单主键的表可直接输入主键，多主键的表注意主键次序
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="values">主键值</param>
        /// <returns>返回主键值为传入值的实体</returns>
        public async Task<T> GetByIDAsync<T>(params object[] values) where T : class
        {
            return await context.Set<T>().FindAsync(values);
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public T Add<T>(T model) where T : class
        {
            T retModel;
            if (model != null)
            {
                context.Set<T>().Add(model);
                context.SaveChanges();
                retModel = model;
            }
            else
            {
                retModel = null;
            }                

            return retModel;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<T> AddAsync<T>(T model) where T : class
        {
            T retModel;

            if (model != null)
            {
                context.Set<T>().Add(model);
                await context.SaveChangesAsync();
                retModel = model;
            }
            else
            {
                retModel = null;
            }

            return retModel;
        }

        /// <summary>
        /// 向上下文增加记录，但不保存，需要手动调用Commit
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public void AddRecord<T>(T model) where T : class
        {
            if (model != null)
            {
                context.Set<T>().Add(model);
            }
        }

        /// <summary>
        /// 按主键标记实体删除(即使传入的实体不是被追踪的实体，同主键的追踪实体依然会标记删除删除)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public void Delete<T>(T model) where T : class
        {
            try
            {
                var entity = context.Remove(model);
            }
            catch (InvalidOperationException e)
            {
                // 若在追踪列表中已经存在另一个实体和该实体有相同主键(但不是同一个)
                // 则找到追踪列表里的实体把它标记为删除
                Type modelType = typeof(T);
                var key = context.Model.FindEntityType(modelType).FindPrimaryKey();
                if (key == null)
                {
                    throw e;
                }
                else
                {
                    // 找主键
                    var props = key.Properties;
                    object[] param = new object[props.Count];
                    int idx = 0;
                    foreach (var p in props)
                    {
                        var clrProp = modelType.GetProperty(p.Name);
                        var val = clrProp.GetValue(model);
                        param[idx] = val;
                        idx++;
                    }
                    // 用主键找实体，标记为删除
                    var cacheModel = context.Set<T>().Find(param);
                    if (cacheModel != null)
                    {
                        context.Remove(cacheModel);
                    }
                }
            }
        }

        /// <summary>
        /// 更新操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public void Update<T>(T model) where T : class
        {
            var entity = context.Entry<T>(model);
            entity.State = EntityState.Modified;
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
            var entity = context.Entry<T>(model);
            entity.Property(property).IsModified = true;
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
            if (isASC)
                return context.Set<T>().OrderBy(orderExpression);
            else
                return context.Set<T>().OrderByDescending(orderExpression);
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
            var datas = this.GetMany<T>(expression);
            var result = datas.OrderBy(orderExpression).Skip(pageSize * (pageIdx - 1)).Take(pageSize);
            return result.ToList();
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
        public async Task<List<T>> GetPageListAsync<T, Tkey>(int pageSize, int pageIdx, Expression<Func<T, bool>> expression, Func<T, Tkey> orderExpression) where T : class
        {
            var datas = this.GetMany<T>(expression);
            var result = datas.OrderBy(orderExpression).Skip(pageSize * (pageIdx - 1)).Take(pageSize).AsQueryable();
            var ret = await result.ToListAsync();
            return ret;
        }

        /// <summary>
        /// 获取条目数
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns>条目数</returns>
        public int GetCount<T>(Expression<Func<T, bool>> expression) where T : class
        {
            int ret = 0;
            var query = this.GetMany<T>(expression);
            ret = query.Count();
            return ret;
        }

        /// <summary>
        /// 获取条目数
        /// </summary>
        /// <typeparam name="T">查询的类</typeparam>
        /// <param name="expression">查询表达式</param>
        /// <returns>条目数</returns>
        public async Task<int> GetCountAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            var query = this.GetMany<T>(expression);
            int ret = await query.CountAsync();
            return ret;
        }

        /// <summary>
        /// 执行存储过程，返回存储过程中返回的数据表
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据表</returns>
        public List<DataTable> CallProcedure(string procName, params DbParameter[] parameters)
        {
            List<DataTable> ret = new List<DataTable>();
            var connection = context.Database.GetDbConnection();
            DbDataReader reader = null;
            DataTable table = null;
            using DbCommand cmd = connection.CreateCommand();
            IDbContextTransaction tran = context.Database.CurrentTransaction;
            if (tran != null)
            {
                cmd.Transaction = tran.GetDbTransaction();
            }

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = procName;
            cmd.Parameters.AddRange(parameters);
            context.Database.OpenConnection();
            reader = cmd.ExecuteReader();
            table = ReaderToDataTable(reader);
            ret.Add(table);
            while (reader.NextResult())
            {
                table = ReaderToDataTable(reader);
                ret.Add(table);
            }
            reader.Close();
            //context.Database.CloseConnection();

            return ret;
        }

        /// <summary>
        /// 执行存储过程，返回存储过程中返回的数据表
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据表</returns>
        public async Task<List<DataTable>> CallProcedureAsync(string procName, params DbParameter[] parameters)
        {
            List<DataTable> ret = new List<DataTable>();
            var connection = context.Database.GetDbConnection();
            DbDataReader reader = null;
            DataTable table = null;
            using DbCommand cmd = connection.CreateCommand();
            IDbContextTransaction tran = context.Database.CurrentTransaction;
            if (tran != null)
            {
                cmd.Transaction = tran.GetDbTransaction();
            }

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = procName;
            cmd.Parameters.AddRange(parameters);
            await context.Database.OpenConnectionAsync();
            reader = await cmd.ExecuteReaderAsync();
            table = ReaderToDataTable(reader);
            ret.Add(table);
            while (reader.NextResult())
            {
                table = ReaderToDataTable(reader);
                ret.Add(table);
            }
            reader.Close();
            //context.Database.CloseConnection();

            return ret;
        }

        /// <summary>
        /// 提交对数据进行的处理，如无处理返回-1
        /// </summary>
        /// <returns></returns>
        public int Save()
        {
            if (context != null)
                return context.SaveChanges();
            else
                return -1;
        }

        /// <summary>
        /// 提交对数据进行的处理，如无处理返回-1
        /// </summary>
        /// <returns></returns>
        public async Task<int> SaveAsync()
        {
            if (context != null)
                return await context.SaveChangesAsync();
            else
                return -1;
        }

        /// <summary>
        /// 异步获取一个事务对象
        /// </summary>
        /// <returns></returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            var ret = await context.Database.BeginTransactionAsync();
            return ret;
        }

        /// <summary>
        /// 获取一个事务对象
        /// </summary>
        /// <returns></returns>
        public IDbContextTransaction BeginTransaction()
        {
            return context.Database.BeginTransaction();
        }

        public DbContext GetDbContext()
        {
            return context;
        }

        private DataTable ReaderToDataTable(DbDataReader reader)
        {
            DataTable table = new DataTable();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                DataColumn column = new DataColumn();
                column.DataType = reader.GetFieldType(i);
                column.ColumnName = reader.GetName(i);
                table.Columns.Add(column);
            }

            while (reader.Read())
            {
                DataRow row = table.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader[i];
                }
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
