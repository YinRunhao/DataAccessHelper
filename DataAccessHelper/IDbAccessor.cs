using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessHelper
{
    /// <summary>
    /// SQL数据库接入层接口
    /// </summary>
    public interface IDbAccessor: IDataAccessor, IMappingMutable, IDisposable
    {
    }
}
