using AutoDiffusion.Data;
using AutoDiffusion.Models;
using Microsoft.Data.SqlClient;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace AutoDiffusion.Services;

public class RandomWordService
{
    private readonly AppDbContext _context;
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
        _context = context;
        _nextLetterProbabilities = new NextLetterProbabilities();
        _configService = configService;
        _nameService = nameService;
    }

    private async Task<List<string>> GetGeneratedWordsAsync(string language, string type)
    {
        return await _context.GeneratedWords
            .Where(gw => gw.Language == language && gw.Type == type)
            .Select(gw => gw.Name)
            .ToListAsync();
    }

    public async Task Generate()
    {
        await using var connection = new SqlConnection(_dbConfiguration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        GeneratedWords.Clear();

        string currentLetter = "", randomWord = "", lastLetters = " ";
        int numberOfWords = 20;

        ConfigModel config = await _configService.GetConfig();

        _nextLetterProbabilities.LoadFromDatabase(connection, config.SelectedLanguage, config.SelectedCategory);
        var existingNames = await _nameService.GetNamesByCountryAndCategoryAsync(config.SelectedLanguage, config.SelectedCategory);
        var existingGeneratedWords = await GetGeneratedWordsAsync(config.SelectedLanguage, config.SelectedCategory);

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

                    if (string.IsNullOrEmpty(currentLetter) && !existingNames.Contains(randomWord) && !existingGeneratedWords.Contains(randomWord))
                    {
                        if (!GeneratedWords.Contains(randomWord))
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
        // Fallback in case probabilities don't add up correctly, should not occur due to normalization
        return probabilities.Last().NextLetter;
    }

    private List<(string NextLetter, double Probability)> FindProbabilitiesByLastLetters(string lastLetters, int currentWordLength, ConfigModel config)
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

    private static string GetLastTwoLetters(string word)
    {
        word = word.Length >= 2 ? word[^2..] : word;
        return word;
    }

    private static bool ShouldGenerateAnotherLetter(string currentWord, string currentLetter, ConfigModel config)
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