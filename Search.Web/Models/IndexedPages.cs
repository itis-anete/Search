using System;
using System.Collections.Generic;
using System.Web;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Search.Web.Models
{
    public class IndexedPagesContext : DbContext
    {
        public DbSet<IndexedPages> IndexedPages { get; set; }

    }

    public class IndexedPages
    {
        public int Index { get; set; }
        public string Pages { get; set; }
    }
}
