using AutoDiffusion.Data;
using AutoDiffusion.Models;
using System.Text.RegularExpressions;

namespace AutoDiffusion.Services
{
    public class ProbabilityService
    {
        private readonly AppDbContext _dbContext;
        private readonly ConfigService _configService;

        List<string> testNames = new()
        {
            "Jean-Pierre",
            "Marie-Claire",
            "Antoine",
            "Elise",
            "François",
            "Jeanne",
            "Pierre-Antoine",
            "Lucie",
            "David",
            "Sophie-Anne",
            "Olivier",
            "Isabelle",
            "Étienne",
            "Jean-Marc",
            "Anne-Laure",
            "Théo",
            "Mathilde",
            "Émile",
            "Nicolas",
            "Margaux"
        };


        public ProbabilityService(AppDbContext dbContext, ConfigService configService)
        {
            _dbContext = dbContext;
            _configService = configService;
        }

        public async Task GenerateAllProbabilities(List<NameModel> allNames)
        {
            var groupedNames = allNames.GroupBy(n => new { n.CountryName, n.Type });

            foreach (var group in groupedNames)
            {
                string language = group.Key.CountryName;
                string nameType = group.Key.Type;
                var namesList = group.ToList();

                await GenerateProbabilities(language, nameType, namesList);
            }
        }

        public async Task GenerateProbabilities(string language, string nameType, List<NameModel> names)
        {
            List<string> normalizedNames = names.Select(n => n.Name.ToLower()).ToList();

            var freqMap = new Dictionary<string, int>();

            foreach (var name in normalizedNames)
            {
                Console.WriteLine($"Processing name: {name}");
                // Case for the first letter of the word
                string separator = "|"; 
                string key = "";
                key = separator + name[0];
                freqMap.TryAdd(key, 0);
                freqMap[key]++;

                for (int i = 0; i < name.Length - 1; i++)
                {
                    // Case for letter following another at the beginning
                    if (i == 0)
                    {
                        key = name[i] + "|";
                        key += name[i + 1];
                        freqMap.TryAdd(key, 0);
                        freqMap[key]++;
                    }

                    key = name.Substring(i, 2) + "|";

                    // Case for the letter following the two letters
                    if (i + 2 < name.Length)
                    {
                        key += name[i + 2];
                        freqMap.TryAdd(key, 0);
                        freqMap[key]++;
                    }

                    if (i == name.Length - 2)
                    {
                        key = name.Substring(i, 2) + "|";
                        freqMap.TryAdd(key, 0);
                        freqMap[key]++;
                    }
                    
                    if (key.EndsWith("-|") || Regex.IsMatch(key, @"-\w\|$"))
                    {
                        Console.WriteLine($"Unexpected key found while processing name: {name}");
                    }
                }
            }

            // Convert frequencies to probabilities
            int totalCounts = freqMap.Values.Sum();
            var probMap = freqMap.ToDictionary(k => k.Key, v => (double)v.Value / totalCounts);

            List<string> nameStrings = names.Select(n => n.Name).ToList();
            var (minLetters, maxLetters) = CalculateMinMaxLetters(nameStrings);
            InsertIntoWordParameters(minLetters, maxLetters, language, nameType);
            await SaveProbabilities(probMap, language, nameType);

            Console.WriteLine("Parameters updated");
        }

        private async Task SaveProbabilities(Dictionary<string, double> probMap, string language, string nameType)
        {

            // Clear existing probabilities from the database
            _dbContext.Probabilities.RemoveRange(_dbContext.Probabilities.Where(p => p.CountryName == language && p.Type == nameType));
            await _dbContext.SaveChangesAsync();

            // Save the new probabilities
            foreach (var entry in probMap)
            {
                var parts = entry.Key.Split('|');
                string lastLetters = parts[0];
                string nextLetter = parts.Length > 1 ? parts[1] : "";

                var tempContext = new ProbabilityModel
                {
                    LastLetters = lastLetters,
                    NextLetter = nextLetter,
                    Probability = entry.Value,
                    CountryName = language,
                    Type = nameType
                };
                _dbContext.Probabilities.Add(tempContext);
            }

            await _dbContext.SaveChangesAsync();
            Console.WriteLine("Database updated");
        }

        private (int minLetters, int maxLetters) CalculateMinMaxLetters(List<string> names)
        {
            var filteredNames = names
                .Where(name => !name.Contains(" ") && !name.Contains(".") && !name.Contains("-")).ToList();
            if (!filteredNames.Any())
            {
                return (0, 0);
            }

            return (filteredNames.Min(name => name.Length), filteredNames.Max(name => name.Length));
        }

        private void InsertIntoWordParameters(int minLetters, int maxLetters, string language, string category)
        {
            var config = _configService.GetConfig();
            config.MinLetters = minLetters;
            config.MaxLetters = maxLetters;
            var existingRecord = _dbContext.Config
                .FirstOrDefault(wp =>
                    wp.SelectedLanguage == language &&
                    wp.SelectedCategory == category);

            if (existingRecord != null)
            {
                // Update the existing record
                existingRecord.MinLetters = minLetters;
                existingRecord.MaxLetters = maxLetters;
            }
            else
            {
                // Create a new record
                var newRecord = new ConfigModel()
                {
                    SelectedLanguage = language,
                    SelectedCategory = category,
                    MinLetters = minLetters,
                    MaxLetters = maxLetters,
                    FullName = 0,
                    FullPlaceName = 0
                };
                _dbContext.Config.Add(newRecord);
            }

            _dbContext.SaveChanges();
        }

    }
}
