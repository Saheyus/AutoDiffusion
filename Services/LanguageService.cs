using AutoDiffusion.Data;
using AutoDiffusion.Models;
using Microsoft.EntityFrameworkCore;

namespace Autodiffusion.Services
{
    public class LanguageService
    {
        private readonly AppDbContext _context;

        public LanguageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LanguageModel>> GetLanguagesAsync()
        {
            return await _context.Languages.ToListAsync();
        }

        public async Task<List<LanguageModel>> GetLanguagesWithGeneratedWordsAsync()
        {
            return await _context.Languages
                .Where(lang => _context.GeneratedWords.Any(gw => gw.Language == lang.Language))
                .ToListAsync();
        }

    }
}

