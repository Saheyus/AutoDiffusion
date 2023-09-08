using System.ComponentModel.DataAnnotations;

namespace AutoDiffusion.Models
{
    public class ProbabilityModel
    {
        [Key]
        public int ID { get; set; }
        public string LastLetters { get; set; }
        public string NextLetter { get; set; }
        public double Probability { get; set; }
        public string CountryName { get; set; }
        public string Type { get; set; }
    }
}

