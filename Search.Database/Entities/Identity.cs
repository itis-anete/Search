using System.ComponentModel.DataAnnotations;

namespace Search.DataBase.Entities
{
    public abstract class Identity
    {
        [Key]
        public int Id { get; set; }
    }
}
