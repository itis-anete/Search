using System;

namespace Search.DataBase.Models
{
    public class User
    {
        public string Nickname { get; set; }
        public string Email { get; set; } // Primary key
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public enum Sex { Male, Female, Other, Unknown }
        public DateTime Birthday { get; set; }
        // Country: ? (можно создать таблицу Countries)
        // public string Countries { get; set; }
    }
}
