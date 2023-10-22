using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDiffusion.Models
{
    [Table("GeneratedWords")]
    public class GeneratedWordModel
    {
        public int Id { get; set; }
        [Column("GeneratedWord")]
        public string? Name { get; set; }
        public string? Language { get; set; }
        public string? Type { get; set; }
    }
}