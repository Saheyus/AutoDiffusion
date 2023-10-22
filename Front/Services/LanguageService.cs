using AutoDiffusion.Data;
using AutoDiffusion.Models;
using AutoDiffusion.Services;
using Microsoft.EntityFrameworkCore;

namespace Autodiffusion.Services
{
    public class LanguageService
    {
        private readonly AppDbContext _context;
        private readonly ConfigService _configService;

        public LanguageService(AppDbContext context, ConfigService configService)
        {
            _context = context;
            _configService = configService;
        }

        public async Task<List<LanguageModel>> GetLanguagesAsync()
        {
            return await _context.Languages.OrderBy(lang => lang.Language).ToListAsync();
        }

        public async Task<List<LanguageModel>> GetLanguagesWithGeneratedWordsAsync()
        {
            return await _context.Languages
                .Where(lang => _context.GeneratedWords.Any(gw => gw.Language == lang.Language))
                .OrderBy(lang => lang.Language)
                .ToListAsync();
        }

        public async Task AddLanguageAsync(LanguageModel newLanguage)
        {
            _context.Languages.Add(newLanguage);
            await _context.SaveChangesAsync();
        }

        public async Task AddLanguageWithConfigAsync(LanguageModel newLanguage)
        {
            await AddLanguageAsync(newLanguage);
            await _configService.CreateDefaultConfigAsync(newLanguage.Language);
        }
    }
}

