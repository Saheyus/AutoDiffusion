using System.ComponentModel.DataAnnotations;

namespace AutoDiffusion.Models
{
    public class ProbabilityModel
    {
        [Key]
        public int Id { get; set; }
        public string? LastLetters { get; init; }
        public string? NextLetter { get; init; }
        public double Probability { get; set; }
        public string? Language { get; init; }
        public string? Type { get; init; }
    }
}

