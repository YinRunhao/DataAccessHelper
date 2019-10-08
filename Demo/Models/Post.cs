using System;
using System.Collections.Generic;

namespace Demo.Models
{
    public partial class Post
    {
        public long PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public long BlogId { get; set; }
        public DateTime PostDate { get; set; }

        public Blog Blog { get; set; }

        public override string ToString()
        {
            return $"ID: {PostId}   Title: {Title}   PostDate: {PostDate.ToShortDateString()}";
        }
    }
}
