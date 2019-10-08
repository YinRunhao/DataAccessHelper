using System;
using DataAccessHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Demo.Models
{
    public partial class BloggingContext : DbContext
    {
        public BloggingContext()
        {
        }

        public BloggingContext(DbContextOptions<BloggingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Blog> Blog { get; set; }
        public virtual DbSet<Post> Post { get; set; }

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
                    .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

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
