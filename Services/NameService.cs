using AutoDiffusion.Data;
using AutoDiffusion.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace AutoDiffusion.Services
{
    public interface INameService
    {
        Task<List<NameModel>> GetAllNamesAsync();
        Task<List<string>> GetNamesByCountryAndCategoryAsync(string country, string category);
        Task<NameModel> GetNameByIdAsync(int id);
        Task AddNameAsync(NameModel nameModel);
        Task UpdateNameAsync(NameModel nameModel);
        Task DeleteNameAsync(int id);
        Task DeleteGeneratedNameAsync(string word);
        Task<HashSet<string>> GetUniqueCountriesAsync();
        Task<HashSet<string>> GetUniqueTypesAsync();
        Task<List<string>> GetGeneratedWordsByCountryAndCategoryAsync(string country, string category);
        Task BulkReplaceNamesAsync(List<NameModel> nameModels, string language, string type);
        Task BulkAddNamesAsync(List<NameModel> nameModels);
    }

    public class NameService : INameService
    {
        private readonly AppDbContext _dbContext;

        public NameService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<NameModel>> GetAllNamesAsync()
        {
            return await _dbContext.Names.ToListAsync();
        }

        public async Task<List<string>> GetNamesByCountryAndCategoryAsync(string country, string category)
        {
            return await _dbContext.Names
                .Where(n => n.Language == country && n.Type == category)
                .Select(n => n.Name)
                .ToListAsync();
        }
        public async Task<List<string>> GetGeneratedWordsByCountryAndCategoryAsync(string country, string category)
        {
            return await _dbContext.GeneratedWords
                .Where(g => g.Language == country && g.Type == category)
                .Select(g => g.Name)
                .ToListAsync();
        }

        public async Task<NameModel> GetNameByIdAsync(int id)
        {
            return await _dbContext.Names.FindAsync(id);
        }

        public async Task AddNameAsync(NameModel nameModel)
        {
            _dbContext.Names.Add(nameModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateNameAsync(NameModel nameModel)
        {
            _dbContext.Names.Update(nameModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteNameAsync(int id)
        {
            var name = await _dbContext.Names.FindAsync(id);
            if (name != null)
            {
                _dbContext.Names.Remove(name);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteGeneratedNameAsync(string word)
        {
            var generatedWordModel = await _dbContext.GeneratedWords.FirstOrDefaultAsync(n => n.Name == word);
            if (generatedWordModel != null)
            {
                _dbContext.GeneratedWords.Remove(generatedWordModel);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<HashSet<string>> GetUniqueCountriesAsync()
        {
            return new HashSet<string>(
                await _dbContext.Names
                    .Select(n => n.Language)
                    .Distinct()
                    .ToListAsync()
            );
        }

        public async Task<HashSet<string>> GetUniqueTypesAsync()
        {
            return new HashSet<string>(
                await _dbContext.Names
                    .Select(n => n.Type)
                    .Distinct()
                    .ToListAsync()
            );
        }

        public async Task BulkReplaceNamesAsync(List<NameModel> nameModels, string language, string type)
        {

            // Remove existing records with the same language and type
            var existingNames = _dbContext.Names
                .Where(n => n.Language == language && n.Type == type);

            _dbContext.Names.RemoveRange(existingNames);

            await _dbContext.SaveChangesAsync();

            // Now, add the new records
            await _dbContext.AddRangeAsync(nameModels);
            await _dbContext.SaveChangesAsync();
        }

        public async Task BulkAddNamesAsync(List<NameModel> nameModels)
        {
            string? targetLanguage = nameModels[0].Language;
            string? targetType = nameModels[0].Type;

            var existingNames = await _dbContext.Names
                .Where(n => n.Language == targetLanguage && n.Type == targetType)
                .Select(n => n.Name)
                .ToListAsync();

            var nonDuplicateNames = nameModels
                .Where(nm => !existingNames.Contains(nm.Name))
                .ToList();

            await _dbContext.AddRangeAsync(nonDuplicateNames);
            await _dbContext.SaveChangesAsync();
        }

    }
}