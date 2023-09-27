using System.ComponentModel.DataAnnotations;

namespace AutoDiffusion.Models
{
    public class LanguageModel
    {
        [Key]
        public string? Language { get; set; }
        public string? Description { get; set; }
    }
}