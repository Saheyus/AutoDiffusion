using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDiffusion.Models
{
    public class LanguageModel
    {
        [Key]
        public string? Language { get; set; }
        public string? Description { get; set; }
        [NotMapped]
        public int ConfigModelId { get; set; }
        [NotMapped]
        public WordParametersModel? Config { get; set; }
    }
}