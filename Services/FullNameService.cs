using AutoDiffusion.Data;
using AutoDiffusion.Models;
using Microsoft.EntityFrameworkCore;

namespace Autodiffusion.Services
{
    public class FullNameService
    {
        private readonly AppDbContext _context;
        private readonly Random _random;

        public FullNameService(AppDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        public async Task<List<FullNameModel>> GetAllAsync()
        {
            return await _context.FullNames.ToListAsync();
        }

        public async Task<List<FullNameModel>> GetAsync(string gender = "male", string language = "french")
        {
            return await _context.FullNames
                .Where(x => x.Gender == gender && x.Language == language)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var fullName = await _context.FullNames.FindAsync(id);
            if (fullName != null)
            {
                _context.FullNames.Remove(fullName);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GenerateFullNameAsync(string language, string gender, int chanceForSecondFirstName, int count = 1)
        {
            List<string> fullNames = new List<string>();

            // Fetch existing names from the FullNames table
            List<string> existingFullNames = await _context.FullNames.Select(f => f.FullName).ToListAsync();

            for (int i = 0; i < count; i++)
            {
                string fullName = "";

                do
                {
                    // Fetch first name
                    string firstName = await GenerateNameAsync(language, gender);

                    // Decide whether to add a second first name
                    bool hasSecondFirstName = _random.Next(100) < chanceForSecondFirstName;
                    if (hasSecondFirstName)
                    {
                        string secondFirstName = await GenerateNameAsync(language, gender);
                        firstName += " " + secondFirstName;
                    }

                    // Fetch last name
                    string lastName = await GenerateNameAsync(language, "Last");

                    // Combine first and last names
                    fullName = $"{firstName} {lastName}";

                } while (fullNames.Contains(fullName) || existingFullNames.Contains(fullName));

                fullNames.Add(fullName);
            }

            return fullNames;
        }

        private async Task<string> GenerateNameAsync(string language, string gender)
        {
            // Query the database for names of the given gender and type
            var names = await _context.GeneratedWords
                .Where(x => x.Type.Equals(gender) && x.Language.Equals(language))
                .ToListAsync();

            if (names.Count == 0)
            {
                throw new InvalidOperationException("No names found for the specified gender and language.");
            }

            // Select a random name
            int index = _random.Next(names.Count);
            return names[index].Name;
        }

        public async Task SaveGeneratedFullNameAsync(FullNameModel model)
        {
            try
            {
                _context.FullNames.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveGeneratedFullNameAsync: {ex.Message}");
            }
        }

    }
}