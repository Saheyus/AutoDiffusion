using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDiffusion.Models
{
    [Table("WordParameters")]
    public class ConfigModel
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("Language")]
        public string? SelectedLanguage { get; set; }

        [Column("Categorie")]
        public string? SelectedCategory { get; set; }

        [Column("MinLetters")]
        public int MinLetters { get; set; }

        [Column("MaxLetters")]
        public int MaxLetters { get; set; }

        [Column("AccentModifier")]
        public double AccentModifier { get; set; }

        [Column("FullName")]
        public int FullName { get; set; }

        [Column("FullPlaceName")]
        public int FullPlaceName { get; set; }

        // Ignored as these are not columns in the table
        [NotMapped]
        public List<string?>? SupportedLanguages { get; set; }

        [NotMapped]
        public List<string?>? SupportedCategories { get; set; }

        public string? SecondNameChance { get; set; }
    }
}