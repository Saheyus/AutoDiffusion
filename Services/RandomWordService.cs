using AutoDiffusion.Data;
using AutoDiffusion.Models;
using AutoDiffusion.Pages;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AutoDiffusion.Services;

public class RandomWordService
{
    private readonly IConfiguration _dbConfiguration;
    private readonly Random _random = new();
    private readonly NextLetterProbabilities _nextLetterProbabilities;
    private List<(string NextLetter, double Probability)>? _probabilities;
    public List<string> GeneratedWords { get; } = new();
    private readonly ConfigService _configService;
    private readonly INameService _nameService;

    public RandomWordService(IConfiguration dbConfiguration, ConfigService configService, INameService nameService, AppDbContext context)
    {
        _dbConfiguration = dbConfiguration;
        _nextLetterProbabilities = new NextLetterProbabilities();
        _configService = configService;
        _nameService = nameService;
    }

    public async Task Generate()
    {
        await using var connection = new SqlConnection(_dbConfiguration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        GeneratedWords.Clear();

        string currentLetter = "", randomWord = "", lastLetters = " ";
        int numberOfWords = 20;

        WordParametersModel config = await _configService.GetConfig();

        _nextLetterProbabilities.LoadFromDatabase(connection, config.SelectedLanguage, config.SelectedCategory);
        var existingNames = await _nameService.GetNamesByCountryAndCategoryAsync(config.SelectedLanguage, config.SelectedCategory);
        var existingGeneratedWords = await _nameService.GetGeneratedWordsByCountryAndCategoryAsync(config.SelectedLanguage, config.SelectedCategory);

        for (int i = 0; i < numberOfWords; i++)
        {
            randomWord = "";
            currentLetter = "";

            bool continueLoop = true;

            do
            {
                lastLetters = string.IsNullOrEmpty(randomWord) ? "" : GetLastTwoLetters(randomWord);
                _probabilities = FindProbabilitiesByLastLetters(lastLetters, randomWord.Length, config);

                int additionalLength = 0;
                if (randomWord.Contains('-') || randomWord.Contains(' ') || randomWord.Contains('\''))
                {
                    additionalLength = 3;
                }

                bool generate = ShouldGenerateAnotherLetter(randomWord, currentLetter, config);
                if (generate) _probabilities.RemoveAll(x => x.NextLetter == "");

                if (_probabilities.Count == 0 || randomWord.Length >= config.MaxLetters + additionalLength ||
                    (_probabilities.Count == 1 && _probabilities[0].NextLetter == ""))
                {
                    randomWord = "";
                }
                else
                {
                    currentLetter = ReturnLetter(_probabilities);
                    randomWord += currentLetter;

                    if (string.IsNullOrEmpty(currentLetter) &&
                        !existingNames.Any(n => n.Equals(randomWord, StringComparison.OrdinalIgnoreCase)) &&
                        !existingGeneratedWords.Any(n => n.Equals(randomWord, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (!GeneratedWords.Any(n => n.Equals(randomWord, StringComparison.OrdinalIgnoreCase)))
                        {
                            randomWord = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(randomWord.ToLower());
                            GeneratedWords.Add(randomWord);
                            break;
                        }
                        randomWord = "";
                    }
                }
            } while (continueLoop);
        }
    }


    private string ReturnLetter(List<(string NextLetter, double Probability)> probabilities)
    {
        // Calculate total sum of probabilities
        double totalProbability = probabilities.Sum(item => item.Probability);

        var randomVar = _random.NextDouble() * totalProbability; // Scale the random variable
        double cumulativeProbability = 0.0;

        foreach (var item in probabilities)
        {
            cumulativeProbability += item.Probability;
            if (randomVar <= cumulativeProbability)
            {
                return item.NextLetter;
            }
        }
        if (probabilities.Any())
        {
            return probabilities.Last().NextLetter;
        }
        else
        {
            // Handle the error, maybe by logging or defaulting to some value
            Console.WriteLine("No probabilities found, using fallback value.");
            return "";
        }
    }

    private List<(string NextLetter, double Probability)> FindProbabilitiesByLastLetters(string lastLetters, int currentWordLength, WordParametersModel config)
    {
        List<(string NextLetter, double Probability)> probabilities = new();

        var matchingProbabilities = _nextLetterProbabilities.Probabilities
            .Where(p => p.LastLetters == lastLetters)
            .ToList();

        foreach (var nextLetterProbability in matchingProbabilities)
        {
            var adjustedProbability = nextLetterProbability.Probability;

            // Ajuster la probabilité pour l'espace en fonction de la longueur actuelle du mot
            if (nextLetterProbability.NextLetter == "")
            {
                var averageLength = (config.MaxLetters + config.MinLetters) / 2.0;
                var ratio = currentWordLength / averageLength;

                // Decrease the probability if the current word length is less than average.
                if (ratio < 1)
                {
                    var adjustmentFactor = 1 - ratio;
                    adjustedProbability *= (1 - adjustmentFactor);
                }
            }

            int index = probabilities.FindIndex(x => x.NextLetter == nextLetterProbability.NextLetter);

            if (index != -1)
            {
                probabilities[index] = (nextLetterProbability.NextLetter, adjustedProbability);
            }
            else
            {
                probabilities.Add((nextLetterProbability.NextLetter, adjustedProbability));
            }
        }
        double sumOfProbabilities = probabilities.Select(p => p.Probability).Sum();
        probabilities = probabilities
            .Select(p => (p.NextLetter, p.Probability / sumOfProbabilities))
            .ToList();
        return probabilities;
    }

    public async Task GenerateWordBasedOn(string baseWord)
    {
        await using var connection = new SqlConnection(_dbConfiguration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        GeneratedWords.Clear();
        const int uniqueWordsToGenerate = 4;
        int generatedWordsCount = 0;

        // Fetch existing names and config
        var existingNames = await _nameService.GetNamesByCountryAndCategoryAsync("language", "category");
        var existingGeneratedWords = await _nameService.GetGeneratedWordsByCountryAndCategoryAsync("language", "category");
        var config = await _configService.GetConfig();
        _nextLetterProbabilities.LoadFromDatabase(connection, config.SelectedLanguage, config.SelectedCategory);

        var rand = new Random();  // Initialize Random once instead of in loop

        while (generatedWordsCount < uniqueWordsToGenerate)
        {
            var letters = baseWord.ToCharArray().ToList();
            var maxAttempts = 100;
            var attempts = 0;

            do
            {
                int indexToReplace = rand.Next(1, letters.Count - 1);  // Use the same Random instance
                string lastLetters = GetLastTwoLetters(string.Concat(letters.Take(indexToReplace))).ToLower();
                var probabilities = FindProbabilitiesByLastLetters(lastLetters, indexToReplace, config);
                char currentLetter = letters[indexToReplace];

                if (!probabilities.Any(p => !string.IsNullOrEmpty(p.NextLetter) && p.NextLetter[0] == currentLetter) && probabilities.Count > 0)
                {
                    string replacementString = ReturnLetter(probabilities);
                    if (!string.IsNullOrEmpty(replacementString))
                    {
                        char replacementLetter = replacementString[0];
                        letters[indexToReplace] = replacementLetter;
                    }
                }


                string newWord = string.Concat(letters);

                if (!GeneratedWords.Contains(newWord, StringComparer.OrdinalIgnoreCase) &&
                    !existingNames.Contains(newWord, StringComparer.OrdinalIgnoreCase) &&
                    !existingGeneratedWords.Contains(newWord, StringComparer.OrdinalIgnoreCase))
                {
                    GeneratedWords.Add(newWord);
                    generatedWordsCount++;
                    break;
                }

                attempts++;
            } while (attempts < maxAttempts);
        }
    }



    private static string GetLastTwoLetters(string word)
    {
        word = word.Length >= 2 ? word[^2..] : word;
        return word;
    }

    private static bool ShouldGenerateAnotherLetter(string currentWord, string currentLetter, WordParametersModel config)
    {
        var currentLength = currentWord.Length;
        var additionalLength = 0;

        if (currentWord.Contains('-') || currentWord.Contains(' ') || currentWord.Contains('\''))
        {
            additionalLength = 3;
        }

        if (currentLength >= config.MinLetters + additionalLength)
        {
            return false;
        }

        return true;
    }

    public void SaveGeneratedWord(NameModel generatedWord)
    {
        using var connection = new SqlConnection(_dbConfiguration.GetConnectionString("DefaultConnection"));
        connection.Open();

        if (IsWordUnique(connection, generatedWord))
        {
            InsertWord(connection, generatedWord);
        }
    }

    private bool IsWordUnique(SqlConnection connection, NameModel generatedWord)
    {
        const string checkQuery = "SELECT COUNT(*) FROM GeneratedWords WHERE GeneratedWord = @word AND Language = @countryName AND Type = @type";
        using var checkCmd = new SqlCommand(checkQuery, connection);

        checkCmd.Parameters.AddWithValue("@word", generatedWord.Name);
        checkCmd.Parameters.AddWithValue("@countryName", generatedWord.Language);
        checkCmd.Parameters.AddWithValue("@type", generatedWord.Type);

        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
        return count == 0;
    }

    private void InsertWord(SqlConnection connection, NameModel generatedWord)
    {
        const string insertQuery = "INSERT INTO GeneratedWords (GeneratedWord, Language, Type) VALUES (@word, @countryName, @type)";
        using var insertCmd = new SqlCommand(insertQuery, connection);

        insertCmd.Parameters.AddWithValue("@word", generatedWord.Name);
        insertCmd.Parameters.AddWithValue("@countryName", generatedWord.Language);
        insertCmd.Parameters.AddWithValue("@type", generatedWord.Type);

        insertCmd.ExecuteNonQuery();
    }


}