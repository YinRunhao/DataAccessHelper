using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataAccessHelper
{
    public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
    {
        private static int m_Marker = 0;

        public static void ChangeTableMapping()
        {
            Interlocked.Increment(ref m_Marker);
        }

        public object Create(DbContext context)
        {
            return (context.GetType(), m_Marker);
        }
    }
}
