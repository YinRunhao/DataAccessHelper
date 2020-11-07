# DataAccessHelper
一个基于EFCore的工具类，对EFCore中的context操作做了进一步的封装，且支持一个实体类映射多个数据表。
## 使用方法
如Demo中的例子所示，示例程序中一个Post实体类对应有多个Post数据表，多个数据表按年月存放不同的数据。数据表的命名规则为PostyyyyMM(如Post201910)。
### 1.新建数据表命名规则提供类，实现ITableMappable接口
```
    // 数据表命名规则提供类，用于根据不同的条件映射不同的数据表
    public class PostMapper : ITableMappable
    {
        // 根据条件返回对应的数据表名
        public string GetMappingTableName(Type modelType, object condition)
        {
            string ret = "";

            if (condition is DateTime date)
            {
                if (modelType == typeof(Post))
                {
                    // Format like   Post201906
                    ret = $"Post{date.ToString("yyyyMM")}";
                }
            }

            return ret;
        }
    }
```
### 2.你的DbContext类需要改动一些代码
请将你代码中的派生自*DbContext*类改为派生自*ExtendDbContext*，*ExtendDbContext*几乎不会改变任何DbContext的行为。

1. 将你代码中的派生自*DbContext*类改为派生自*ExtendDbContext*
2. 实现基类的构造方法，但可以什么都不干
3. 把重写OnConfiguring的代码移到Configuring方法中进行重写
4. 把重写OnModelCreating的代码移到ModelCreating方法中进行重写

```
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
```
### 3.正式使用
```
    // 使用示例
    static void TestChangeTable(DataAccessor dal, ITableMappable mapper)
    {
        // 数据表映射条件
        DateTime sept = DateTime.Parse("2019-09-05");
        DateTime oct = DateTime.Parse("2019-10-05");

        // 切换Post实体类的映射的数据表，切换条件为2019年10月，理论上应该切换为数据表 "Post201910"
        dal.ChangeMappingTable(typeof(Post), mapper, oct);
        // 查询该表下的所有数据
        List<Post> octData = dal.GetAll<Post>().ToList();
        Console.WriteLine("Oct. data");
        foreach (Post item in octData)
        {
            Console.WriteLine(item);
        }

        // 切换Post实体类的映射的数据表，切换条件为2019年9月，理论上应该切换为数据表 "Post201909"
        dal.ChangeMappingTable(typeof(Post), mapper, sept);
        // 查询该表下的所有数据
        List<Post> septData = dal.GetAll<Post>().ToList();
        Console.WriteLine("Sept. data");
        foreach (Post item in septData)
        {
            Console.WriteLine(item);
        }
    }
```
