using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessHelper
{
    /// <summary>
    /// EFCore SQL数据库接入层接口
    /// </summary>
    public interface IDbAccessor: IEFAvailable, IMappingMutable, IDisposable
    {
        /// <summary>
        /// 获取EFCore的DbContext
        /// </summary>
        /// <returns>DbContext</returns>
        DbContext GetDbContext();
    }
}
