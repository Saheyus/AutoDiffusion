using AutoDiffusion.Data;
using AutoDiffusion.Models;
using Microsoft.EntityFrameworkCore;


namespace AutoDiffusion.Services
{
    public interface INameService
    {
        Task<List<NameModel>> GetAllNamesAsync();
        Task<List<string>> GetNamesByCountryAndCategoryAsync(string country, string category);
        Task<List<string>> GetPopularNamesByCountryAndCategoryAsync(string country, string category);
        Task<NameModel> GetNameByIdAsync(int id);
        Task AddNameAsync(NameModel nameModel);
        Task UpdateNameAsync(NameModel nameModel);
        Task DeleteNameAsync(int id);
        Task DeleteGeneratedNameAsync(string word);
        Task<HashSet<string>> GetUniqueCountriesAsync();
        Task<HashSet<string>> GetUniqueTypesAsync();
        Task<List<string>> GetGeneratedWordsByCountryAndCategoryAsync(string country, string category);
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
                .Where(n => n.CountryName == country && n.Type == category)
                .Select(n => n.Name)
                .ToListAsync();
        }
        public async Task<List<string>> GetGeneratedWordsByCountryAndCategoryAsync(string country, string category)
        {
            return await _dbContext.GeneratedWords
                .Where(g => g.CountryName == country && g.Type == category)
                .Select(g => g.Name)
                .ToListAsync();
        }


        public async Task<List<string>> GetPopularNamesByCountryAndCategoryAsync(string country, string category)
        {
            return await _dbContext.Names
                .Where(n => n.CountryName == country && n.Type == category)
                .Take(100)
                .Select(n => n.Name)  // Replace 'Name' with the actual property that contains the name string
                .ToListAsync();
        }


        public async Task<List<NameModel>> GetNamesByPageAsync(int page, int pageSize)
        {
            // Query the database to fetch only a subset of records based on page and pageSize
            // For example, using Entity Framework, you could do:
            return await _dbContext.Names.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
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
                    .Select(n => n.CountryName)
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
    }
}