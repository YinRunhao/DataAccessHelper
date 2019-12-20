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
    public class BaseDataAccessor : IDataAccessor
    {
        private DbContext context;
        private static Type contextType = null;

        /// <summary>
        /// 使用该类前必须调用此方法指明DbContext类型，若传入的类型不是继承于DbContext，将冒ArgumentException
        /// </summary>
        /// <param name="t">Context类型</param>
        /// <exception cref="ArgumentException">context 类型设置错误</exception>
        public static void SetContextType(Type t)
        {
            if (t.BaseType != typeof(DbContext))
            {
                throw new ArgumentException("ContextType must inherits from DbContext");
            }
            else
            {
                contextType = t;
            }
        }

        /// <summary>
        /// 创建Helper对象，调用前请确保已设定Context类型，若未设定DbContext类型则会冒ArgumentNullException
        /// </summary>
        /// <exception cref="ArgumentNullException">context 未设置</exception>
        public BaseDataAccessor()
        {
            if (contextType == null)
            {
                throw new ArgumentNullException("You have not set context type");
            }
            context = (DbContext)Activator.CreateInstance(contextType);
        }

        public BaseDataAccessor(ICollection<TableMappingRule> rules)
        {
            if (contextType == null)
            {
                throw new ArgumentNullException("You have not set context type");
            }
            context = (DbContext)Activator.CreateInstance(contextType, rules);
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
        /// 获取某个表的所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> GetAll<T>() where T : class
        {
            var result = context.Set<T>().Where(s => 1 == 1);
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

            try
            {
                if (model != null)
                {
                    context.Set<T>().Add(model);
                    context.SaveChanges();
                    retModel = model;
                }
                else
                    retModel = null;
            }
            catch (Exception)
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

            try
            {
                if (model != null)
                {
                    context.Set<T>().Add(model);
                    await context.SaveChangesAsync();
                    retModel = model;
                }
                else
                    retModel = null;
            }
            catch (Exception)
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
        public bool AddRecord<T>(T model) where T : class
        {
            bool ret = false;
            try
            {
                if (model != null)
                {
                    context.Set<T>().Add(model);
                    ret = true;
                }
                else
                    ret = false;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// 删除操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete<T>(T model) where T : class
        {
            try
            {
                context.Set<T>().Attach(model);
                context.Set<T>().Remove(model);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 更新操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update<T>(T model) where T : class
        {
            var entity = context.Entry<T>(model);
            entity.State = EntityState.Modified; //System.Data.Entity.EntityState.Modified;
            return true;
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
        public DataTable CallProcedure(string procName, params DbParameter[] parameters)
        {
            var connection = context.Database.GetDbConnection();
            DbDataReader reader = null;
            DataTable table = null;
            DbCommand cmd = connection.CreateCommand();

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = procName;
            cmd.Parameters.AddRange(parameters);
            context.Database.OpenConnection();
            reader = cmd.ExecuteReader();
            table = ReaderToDataTable(reader);
            reader.Close();
            context.Database.CloseConnection();

            return table;
        }

        /// <summary>
        /// 执行存储过程，返回存储过程中返回的数据表
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据表</returns>
        public async Task<DataTable> CallProcedureAsync(string procName, params DbParameter[] parameters)
        {
            var connection = context.Database.GetDbConnection();
            DbDataReader reader = null;
            DataTable table = null;
            DbCommand cmd = connection.CreateCommand();

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = procName;
            cmd.Parameters.AddRange(parameters);
            await context.Database.OpenConnectionAsync();
            reader = await cmd.ExecuteReaderAsync();
            table = ReaderToDataTable(reader);
            reader.Close();
            context.Database.CloseConnection();

            return table;
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
