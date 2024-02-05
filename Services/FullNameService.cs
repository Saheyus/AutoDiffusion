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

        public async Task<(List<string> FullNames, string ErrorMessage)> GenerateFullNameAsync(string language, string gender, int chanceForSecondFirstName, int count = 1)
        {
            List<string> fullNames = new List<string>();

            for (int i = 0; i < count; i++)
            {
                // Fetch first name and check for errors
                var (firstName, firstNameError) = await GenerateNameAsync(language, gender);
                if (firstNameError)
                {
                    return (null, "Failed to generate a first name due to no names being found for the specified criteria.");
                }

                // Decide whether to add a second first name
                bool hasSecondFirstName = _random.Next(100) < chanceForSecondFirstName;
                if (hasSecondFirstName)
                {
                    var (secondFirstName, secondNameError) = await GenerateNameAsync(language, gender);
                    if (secondNameError)
                    {
                        return (null, "Failed to generate a second first name due to no names being found for the specified criteria.");
                    }
                    firstName += " " + secondFirstName;
                }

                // Fetch last name
                var (lastName, lastNameError) = await GenerateNameAsync(language, "Last");
                if (lastNameError)
                {
                    // Handle the error for the last name
                    return (null, "Failed to generate a last name due to no names being found for the specified criteria.");
                }

                string fullName = $"{firstName} {lastName}";
                fullNames.Add(fullName);
            }

            return (fullNames, null);
        }

        private async Task<(string Name, bool IsError)> GenerateNameAsync(string language, string gender)
        {
            var names = await _context.GeneratedWords
                .Where(x => x.Type.Equals(gender) && x.Language.Equals(language))
                .ToListAsync();

            if (!names.Any())
            {
                // Return with IsError set to true to indicate failure
                return (null, true);
            }

            int index = _random.Next(names.Count);

            // Name is found, IsError is false
            return (names[index].Name, false);
        }

        public async Task SaveGeneratedFullNameAsync(FullNameModel model)
        {
            try
            {
                if (model.Gender != null && model.Language != null)
                {
                    _context.FullNames.Add(model);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine("Error in SaveGeneratedFullNameAsync: Gender or Language is null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveGeneratedFullNameAsync: {ex.Message}");
            }
        }

    }
}