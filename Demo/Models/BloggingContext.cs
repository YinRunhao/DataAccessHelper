using System;
using DataAccessHelper;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Demo.Models
{
    // step1:派生自ExtendDbContext
    public partial class BloggingContext : ExtendDbContext
    {
        public virtual DbSet<Blog> Blog { get; set; }
        public virtual DbSet<Post> Post { get; set; }

        // step2:实现基类的构造方法，但可以什么都不干
        public BloggingContext()
        {
        }

        // step2:实现基类的构造方法，但可以什么都不干
        public BloggingContext(ICollection<TableMappingRule> rules):base(rules)
        {
        }

        private static string ConnectString
        {
            get; set;
        }

        public static void SetConnectString(string conStr)
        {
            ConnectString = conStr;
        }

        // step3:把重写OnConfiguring的代码移到Configuring方法中进行重写
        protected override void Configuring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(ConnectString)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        }

        // step4:把重写OnModelCreating的代码移到ModelCreating方法中进行重写
        protected override void ModelCreating(ModelBuilder modelBuilder)
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
        }
    }
}
