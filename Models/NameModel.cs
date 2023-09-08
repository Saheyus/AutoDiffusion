using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDiffusion.Models
{
    [Table("Names")]
    public class NameModel
    {
        [Key]
        public int ID { get; set; }
        public string? Name { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? Type { get; set; }
    }

}
