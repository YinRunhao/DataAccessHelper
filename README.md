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
### 2.重写EFCore中Context的OnConfiguring方法，调用ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>()
```
    public partial class BloggingContext : DbContext
    {
        ...
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // setp 0 : Replace Service
                optionsBuilder.UseSqlite(ConnectString)
                    .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();
            }
        }
        ...
    }
```
### 3.设置Context类型
在程序初始化时调用以下语句设置你程序中的context类型
```
BaseDataAccessor.SetContextType(typeof(BloggingContext));
```
### 4.向你的DbContext类添加一些代码
```
    public partial class BloggingContext : DbContext
    {
        public virtual DbSet<Blog> Blog { get; set; }
        public virtual DbSet<Post> Post { get; set; }

        // 增加映射规则成员变量
        private ICollection<TableMappingRule> m_TableMappingRule;

        public BloggingContext()
        {
        }

        // 可以选择通过构造方法传入
        public BloggingContext(ICollection<TableMappingRule> rules)
        {
            this.m_TableMappingRule = rules;
        }

        ...

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(e => e.BlogId);

                entity.ToTable("Blog");

                entity.Property(e => e.BlogId).ValueGeneratedNever();

                entity.Property(e => e.Url).HasColumnType("VARCHAR (1024)");
            });

            ...
            // 在OnModelCreating方法结束前调用扩展方法ChangeTableMapping，传入数据表映射规则，若规则为空则不会改变任何数据表映射
            modelBuilder.ChangeTableMapping(m_TableMappingRule);
        }
    }
```
### 5.正式使用
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
