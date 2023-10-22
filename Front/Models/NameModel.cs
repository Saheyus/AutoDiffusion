using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDiffusion.Models
{
    [Table("Names")]
    public class NameModel
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Language { get; set; }
        public string? Type { get; set; }
    }

}
