using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search.App.Models
{
    public class User
    {
        public string Nickname { get; set;}
        public string Email { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public enum Sex { male, female, undefined }
        public DateTime Birthday { get; set; }
        // Country: ? (можно создать таблицу Countries)
       // public string Countries { get; set; }
    }
}
