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
    public interface IDataAccessor
    {
        bool IsClose();

        void Close();

        IQueryable<T> GetAll<T>() where T : class;

        IQueryable<T> GetMany<T>(Expression<Func<T, bool>> expression) where T : class;

        T GetByID<T>(params object[] values) where T : class;

        Task<T> GetByIDAsync<T>(params object[] values) where T : class;

        T Add<T>(T model) where T : class;

        Task<T> AddAsync<T>(T model) where T : class;

        bool AddRecord<T>(T model) where T : class;

        bool Delete<T>(T model) where T : class;

        bool Update<T>(T model) where T : class;

        IEnumerable<T> Order<T, TKey>(Func<T, TKey> orderExpression, bool isASC = false) where T : class;

        List<T> GetPageList<T, Tkey>(int pageSize, int pageIdx, Expression<Func<T, bool>> expression, Func<T, Tkey> orderExpression) where T : class;

        Task<List<T>> GetPageListAsync<T, Tkey>(int pageSize, int pageIdx, Expression<Func<T, bool>> expression, Func<T, Tkey> orderExpression) where T : class;

        int GetCount<T>(Expression<Func<T, bool>> expression) where T : class;

        Task<int> GetCountAsync<T>(Expression<Func<T, bool>> expression) where T : class;

        DataTable CallProcedure(string procName, params DbParameter[] parameters);

        Task<DataTable> CallProcedureAsync(string procName, params DbParameter[] parameters);

        int Save();

        Task<int> SaveAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();

        IDbContextTransaction BeginTransaction();
    }
}
