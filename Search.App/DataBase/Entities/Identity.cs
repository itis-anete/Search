using System.ComponentModel.DataAnnotations;

namespace Search.App.DataBase.Entities
{
    public abstract class Identity
    {
        [Key]
        public int Id { get; set; }
    }
}
