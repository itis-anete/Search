using Microsoft.AspNetCore.Mvc.RazorPages;
using Search.App.DataBase.Context;
using Search.DataBase.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Search.App.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SearchDbContext dbContext;

        public IndexModel(SearchDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void OnGet()
        {
            this.Sources = this.dbContext.Sources.ToList();
        }

        public List<Source> Sources { get; set; }
    }
}
