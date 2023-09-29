using AutoDiffusion.Data;
using AutoDiffusion.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoDiffusion.Services
{
    public class ProbabilityService
    {
        private readonly AppDbContext _dbContext;
        private readonly ConfigService _configService;


        public ProbabilityService(AppDbContext dbContext, ConfigService configService)
        {
            _dbContext = dbContext;
            _configService = configService;
        }

        public async Task GenerateAllProbabilities(List<NameModel> allNames)
        {
            var groupedNames = allNames.GroupBy(n => new { CountryName = n.Language, n.Type });

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
                //Console.WriteLine($"Processing name: {name}");
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
            var existingEntries = _dbContext.Probabilities
                .Where(p => p.Language == language && p.Type == nameType)
                .ToList();

            _dbContext.Probabilities.RemoveRange(existingEntries);
            await _dbContext.SaveChangesAsync();

            // Prepare new probabilities
            var newProbabilities = probMap.Select(entry => {
                var parts = entry.Key.Split('|');
                string lastLetters = parts[0];
                string nextLetter = parts.Length > 1 ? parts[1] : "";
                return new ProbabilityModel
                {
                    LastLetters = lastLetters,
                    NextLetter = nextLetter,
                    Probability = entry.Value,
                    Language = language,
                    Type = nameType
                };
            }).ToList();

            // Save new probabilities
            _dbContext.Probabilities.AddRange(newProbabilities);

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
            config.Result.MinLetters = minLetters;
            config.Result.MaxLetters = maxLetters;
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

        public async Task MixLanguageProbabilities(List<(string Language, double Weight)> mixedLanguages, string newLanguageName)
        {
            // Clear existing probabilities from the database
            _dbContext.Probabilities.RemoveRange(_dbContext.Probabilities.Where(p => p.Language == newLanguageName));
            await _dbContext.SaveChangesAsync();

            //Add to primary table Language, prerequisite
            LanguageModel generatedLanguage = new();
            generatedLanguage.Language = newLanguageName;
            StringBuilder description = new();
            foreach (var (language, weight) in mixedLanguages)
            {
                description.Append(language + "_" + weight + " ");
            }
            generatedLanguage.Description = description.ToString();
            _dbContext.Languages.Add(generatedLanguage);

            ValidateInputs(mixedLanguages);
            var allProbabilities = CollectAndWeightProbabilities(mixedLanguages, newLanguageName);
            var combinedProbabilities = CombineProbabilities(allProbabilities);
            var minLetters = 3;
            var maxLetters = 12;
            string[] nameTypes = new[] { "Male", "Female", "Last" };

            foreach (string nameType in nameTypes)
            {
                InsertIntoWordParameters(minLetters, maxLetters, newLanguageName, nameType);
            }

            SaveCombinedProbabilities(combinedProbabilities, newLanguageName);
        }

        public async Task DeleteLanguageAndCascade(string languageToDelete)
        {
            // Delete records from related tables
            var wordParameters = _dbContext.Config.Where(wp => wp.SelectedLanguage == languageToDelete);
            _dbContext.Config.RemoveRange(wordParameters);

            var probabilities = _dbContext.Probabilities.Where(p => p.Language == languageToDelete);
            _dbContext.Probabilities.RemoveRange(probabilities);

            var generatedWords = _dbContext.GeneratedWords.Where(gw => gw.Language == languageToDelete);
            _dbContext.GeneratedWords.RemoveRange(generatedWords);

            // Delete the specific language entry
            var language = _dbContext.Languages.SingleOrDefault(l => l.Language == languageToDelete);
            if (language != null)
            {
                _dbContext.Languages.Remove(language);
            }

            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"Deleted all data related to the language: {languageToDelete}");
        }

        private void ValidateInputs(List<(string Language, double Weight)> mixedLanguages)
        {
            if (mixedLanguages == null || mixedLanguages.Count == 0)
            {
                throw new ArgumentException("No languages provided for mixing.");
            }

            if (Math.Abs(mixedLanguages.Sum(x => x.Weight) - 1) > 0.01)
            {
                throw new ArgumentException("The sum of weights must be approximately 1.");
            }
        }

        private List<ProbabilityModel> CollectAndWeightProbabilities(List<(string Language, double Weight)> mixedLanguages, string newLanguageName)
        {
            var allProbabilities = new List<ProbabilityModel>();
            foreach (var (language, weight) in mixedLanguages)
            {
                var probabilities = _dbContext.Probabilities
                    .Where(p => p.Language == language)
                    .ToList();

                foreach (var prob in probabilities)
                {
                    allProbabilities.Add(new ProbabilityModel
                    {
                        LastLetters = prob.LastLetters,
                        NextLetter = prob.NextLetter,
                        Probability = prob.Probability * weight,
                        Language = newLanguageName,
                        Type = prob.Type
                    });
                }
            }
            return allProbabilities;
        }

        private Dictionary<(string LastLetters, string NextLetter, string Type), double> CombineProbabilities(List<ProbabilityModel> allProbabilities)
        {
            var combinedProbabilities = new Dictionary<(string LastLetters, string NextLetter, string Type), double>();
            foreach (var prob in allProbabilities)
            {
                var key = (prob.LastLetters, prob.NextLetter, prob.Type);
                if (combinedProbabilities.ContainsKey(key))
                {
                    combinedProbabilities[key] += prob.Probability;
                }
                else
                {
                    combinedProbabilities[key] = prob.Probability;
                }
            }
            return combinedProbabilities;
        }

        private void SaveCombinedProbabilities(Dictionary<(string LastLetters, string NextLetter, string Type), double> combinedProbabilities, string newLanguageName)
        {
            var newProbabilities = combinedProbabilities.Select(kvp => new ProbabilityModel
            {
                LastLetters = kvp.Key.LastLetters,
                NextLetter = kvp.Key.NextLetter,
                Probability = kvp.Value,
                Language = newLanguageName,
                Type = kvp.Key.Type
            }).ToList();

            Console.WriteLine($"Debug: Prepared {newProbabilities.Count} new probabilities for {newLanguageName}.");

            _dbContext.Probabilities.AddRange(newProbabilities);
            _dbContext.SaveChanges();
        }

        public Dictionary<string, (int MatchingScore, string ClosestWord, string Description)> CheckWordAgainstLanguages(string word)
        {
            int threshold = 6;

            // Fetch relevant name and language data from the database
            var nameData = _dbContext.Names.ToList();
            var languageDescriptions = _dbContext.Languages.ToDictionary(l => l.Language, l => l.Description);

            // Convert data into the required format for CheckLanguage
            Dictionary<string, List<string>> languageWords = new Dictionary<string, List<string>>();

            foreach (var item in nameData)
            {
                if (!languageWords.ContainsKey(item.Language))
                {
                    languageWords[item.Language] = new List<string>();
                }
                if (item.Name != null)
                {
                    languageWords[item.Language].Add(item.Name);
                }
            }

            // Call CheckLanguage
            var unsortedResults = CheckLanguage(word, languageWords, threshold);

            // Create the result dictionary
            Dictionary<string, (int MatchingScore, string ClosestWord, string Description)> results = new Dictionary<string, (int, string, string)>();

            foreach (var kvp in unsortedResults)
            {
                string description = languageDescriptions.ContainsKey(kvp.Key) ? languageDescriptions[kvp.Key] : "Unknown";
                results[kvp.Key] = (kvp.Value.MatchingScore, kvp.Value.ClosestWord, description);
            }

            // Sort the dictionary by MatchingScore
            var sortedResults = results.OrderBy(x => x.Value.MatchingScore).ToDictionary(x => x.Key, x => x.Value);

            return sortedResults;
        }

        // Method to calculate Levenshtein Distance
        public static int LevenshteinDistance(string a, string b)
        {
            int lenA = a.Length;
            int lenB = b.Length;
            var d = new int[lenA + 1, lenB + 1];

            for (int i = 0; i <= lenA; i++)
                d[i, 0] = i;

            for (int j = 0; j <= lenB; j++)
                d[0, j] = j;

            for (int i = 1; i <= lenA; i++)
            {
                for (int j = 1; j <= lenB; j++)
                {
                    int cost = (b[j - 1] == a[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[lenA, lenB];
        }

        // Method to Check Language based on similarity
        public Dictionary<string, (int MatchingScore, string ClosestWord)> CheckLanguage(string word, Dictionary<string, List<string>> languageWords, int threshold)
        {
            Dictionary<string, (int MatchingScore, string ClosestWord)> matchingLanguages = new Dictionary<string, (int, string)>();

            foreach (var language in languageWords.Keys)
            {
                int minDistance = int.MaxValue;
                string closestWord = null;

                foreach (var languageWord in languageWords[language])
                {
                    int distance = LevenshteinDistance(word.ToLower(), languageWord.ToLower());
                    if (distance <= threshold && distance < minDistance)
                    {
                        minDistance = distance;
                        closestWord = languageWord;
                    }
                }

                if (minDistance != int.MaxValue)
                {
                    matchingLanguages[language] = (minDistance, closestWord);
                }
            }
            return matchingLanguages;
        }
    }
}
