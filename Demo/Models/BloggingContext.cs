using System;
using DataAccessHelper;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Demo.Models
{
    public partial class BloggingContext : DbContext
    {
        public virtual DbSet<Blog> Blog { get; set; }
        public virtual DbSet<Post> Post { get; set; }

        private ICollection<TableMappingRule> m_TableMappingRule;

        public BloggingContext()
        {
        }

        public BloggingContext(ICollection<TableMappingRule> rules)
        {
            this.m_TableMappingRule = rules;
        }

        private static string ConnectString
        {
            get; set;
        }

        public static void SetConnectString(string conStr)
        {
            ConnectString = conStr;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // setp 0 : Replace Service
                optionsBuilder.UseSqlite(ConnectString)
                    .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(e => e.BlogId);

                entity.ToTable("Blog");

                entity.Property(e => e.BlogId).ValueGeneratedNever();

                entity.Property(e => e.Url).HasColumnType("VARCHAR (1024)");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.PostId);

                entity.ToTable("Post");

                entity.Property(e => e.PostId).ValueGeneratedNever();

                entity.Property(e => e.Content).HasColumnType("VARCHAR (1024)");

                entity.Property(e => e.PostDate)
                    .IsRequired()
                    .HasColumnType("DATETIME");

                entity.Property(e => e.Title).HasColumnType("VARCHAR (512)");

                entity.HasOne(e => e.Blog)
                    .WithMany(b => b.Posts)
                    .HasForeignKey(e => e.BlogId);
            });
            // step 3 Call Extension method
            modelBuilder.ChangeTableMapping(m_TableMappingRule);
        }
    }
}
