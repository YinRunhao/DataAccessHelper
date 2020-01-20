using System;
using System.Collections.Generic;

namespace Demo.Models
{
    public partial class Blog
    {
        public Blog()
        {
            Posts = new HashSet<Post>();
        }

        public long BlogId { get; set; }
        public string Url { get; set; }
        public long Rating { get; set; }

        public ICollection<Post> Posts { get; set; }

        public override string ToString()
        {
            return $"{{ID: {BlogId}, URL: {Url}, Rating: {Rating}}}";
        }
    }
}
