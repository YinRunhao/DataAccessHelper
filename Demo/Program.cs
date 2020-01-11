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

            // Basic usage
            BasicUsage(dal);

            dal.Close();
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
            mapping =  dal.GetTableName(typeof(Post));
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
            long newId = 2;
            // Add
            Blog newData = new Blog { BlogId = newId, Rating = 666, Url = "https://blog.test.com" };
            dal.AddRecord(newData);
            dal.Save();

            // select
            Blog target = dal.GetByID<Blog>(newId);
            //Blog target = dal.GetMany<Blog>(s=>s.BlogId == newId).FirstOrDefault();
            //Blog target = dal.GetAll<Blog>().Where(s=>s.BlogId == newId).FirstOrDefault();

            // update
            target.Url = "https://newurl.test.com";
            dal.Update(target);
            dal.Save();

            // delete
            dal.Delete(target);
            dal.Save();
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
