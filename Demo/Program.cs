using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Demo.Models;
using DataAccessHelper;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Init();

            ITableMappable mapper = new PostMapper();
            DataAccessor dal = new DataAccessor();
            TestTableName(dal, mapper);

            TestChangeTable(dal, mapper);

            BasicUsage(dal);

            TestChangeDb(dal, mapper);

            TestGetTableSQL(dal, mapper);
            dal.Close();
            Console.WriteLine("Test Finish press any key to exit");

            Console.ReadKey();
        }

        static void Init()
        {
            string dbPath = Directory.GetCurrentDirectory();
            dbPath = Path.Combine(dbPath, "Blogging.db");
            string conStr = $"Data Source={dbPath}";

            // step 1: Set connect string
            BloggingContext.SetConnectString(conStr);
            // step 2: Set db context type
            DataAccessor.SetContextType(typeof(BloggingContext));
        }

        static void TestTableName(DataAccessor dal, ITableMappable mapper)
        {
            var mapping = dal.GetTableName(typeof(Post));
            Console.WriteLine($"Original table name: {mapping.TableName}");

            dal.ChangeMappingTable(typeof(Post), mapper, DateTime.Parse("2019-09-05"));
            mapping = dal.GetTableName(typeof(Post));
            Console.WriteLine($"Update table name: {mapping.TableName}\n");
        }

        static void TestChangeTable(DataAccessor dal, ITableMappable mapper)
        {
            DateTime sept = DateTime.Parse("2019-09-05");
            DateTime oct = DateTime.Parse("2019-10-05");

            dal.ChangeMappingTable(typeof(Post), mapper, oct);
            List<Post> octData = dal.GetAll<Post>().ToList();
            Console.WriteLine("Oct. data");
            foreach (Post item in octData)
            {
                Console.WriteLine(item);
            }

            dal.ChangeMappingTable(typeof(Post), mapper, sept);
            List<Post> septData = dal.GetAll<Post>().ToList();
            Console.WriteLine("Sept. data");
            foreach (Post item in septData)
            {
                Console.WriteLine(item);
            }
        }

        static void BasicUsage(DataAccessor dal)
        {
            Console.WriteLine("\nTest BasicUsage");
            long newId = 2;
            // Add
            Console.WriteLine("Test Add");
            Console.WriteLine("Org data:");
            var list = dal.GetAll<Blog>().ToList();
            PrintData(list);
            Blog newData = new Blog { BlogId = newId, Rating = 666, Url = "https://blog.test.com" };
            dal.AddRecord(newData);
            dal.Save();
            Console.WriteLine("New data:");
            list = dal.GetAll<Blog>().ToList();
            PrintData(list);
            Console.WriteLine();

            // update whole entity
            Blog target = dal.GetByID<Blog>(newId);
            Console.WriteLine("Test update whole entity");
            Console.WriteLine("Org data:");
            Console.WriteLine(target);
            target.Url = "https://newurl.test.com";
            target.Rating = 2610;
            dal.Update(target);
            dal.Save();
            var item = dal.GetMany<Blog>(s => s.BlogId == newId).FirstOrDefault();
            Console.WriteLine("New data:");
            Console.WriteLine(item);
            Console.WriteLine();

            // update one property
            TestUpdateProperty();

            // delete
            Console.WriteLine("Test Delete");
            dal.Delete(target);
            dal.Save();
            list = dal.GetAll<Blog>().ToList();
            PrintData(list);
            Console.WriteLine();
        }

        static void TestChangeDb(DataAccessor dal, ITableMappable mapper)
        {
            Console.WriteLine("\nTest change Db");
            string dbPath = Directory.GetCurrentDirectory();
            dbPath = Path.Combine(dbPath, "Blogging_1.db");
            string conStr = $"Data Source={dbPath}";

            Console.WriteLine("Org Data:");
            var list = dal.GetAll<Blog>().ToList();
            PrintData(list);

            dal.ChangeDataBase(conStr);

            Console.WriteLine("Changed Data:");
            list = dal.GetAll<Blog>().ToList();
            PrintData(list);
        }

        static void TestGetTableSQL(DataAccessor dal, ITableMappable mapper)
        {
            Console.WriteLine("\nTest Get Db SQL");
            Console.WriteLine("Original SQL");
            var sql = dal.GetTableCreateScript();
            Console.WriteLine(sql);
            dal.ChangeMappingTable(typeof(Post), mapper, DateTime.Now);
            Console.WriteLine("New SQL");
            sql = dal.GetTableCreateScript();
            Console.WriteLine(sql);
        }

        static void PrintData<T>(List<T> arr)
        {
            foreach (T item in arr)
            {
                Console.WriteLine(item);
            }
        }

        /// <summary>
        /// 测试更新单个字段
        /// </summary>
        private static void TestUpdateProperty()
        {
            var dal = new DataAccessor();
            Console.WriteLine("Test update one property");
            var data = dal.GetByID<Blog>((long)1);
            Console.WriteLine("Org data:");
            Console.WriteLine(data);
            data.Url = "https://jojo.test.com";
            data.Rating = 2;
            // only update Url, the change of Rating will be ignored
            dal.Update<Blog>(data, s => s.Url);
            dal.Save();

            // 这里需要先关掉再重开，因为EFCore的仓储模式机制，data对象虽然在数据库只更新了Url属性，但它在EFCore的缓存仓里面是改了Url和Rating
            dal.Close();
            dal = new DataAccessor();
            var itemFromDb = dal.GetByID<Blog>(data.BlogId);
            Console.WriteLine("New data:");
            Console.WriteLine(itemFromDb);
            Console.WriteLine();
        }
    }

    public class PostMapper : ITableMappable
    {
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
}
