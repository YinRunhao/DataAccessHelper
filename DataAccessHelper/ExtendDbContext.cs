using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessHelper
{
    /// <summary>
    /// 对EFCore中DbContext的一个小扩展用于实现表类型和表名的动态映射，请重写Configuring方法而不是OnConfiguring；重写ModelCreating方法而不是OnModelCreating方法
    /// </summary>
    public abstract class ExtendDbContext : DbContext
    {
        protected ICollection<TableMappingRule> m_TableMappingRule;

        public ExtendDbContext() 
        {
        }

        public ExtendDbContext(ICollection<TableMappingRule> rules)
        {
            this.m_TableMappingRule = rules;
        }

        protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                Configuring(optionsBuilder);
                optionsBuilder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();
            }
        }

        protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ModelCreating(modelBuilder);
            modelBuilder.ChangeTableMapping(m_TableMappingRule);
        }

        protected abstract void Configuring(DbContextOptionsBuilder optionsBuilder);

        protected abstract void ModelCreating(ModelBuilder modelBuilder);
    }
}
