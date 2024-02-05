using AutoDiffusion.Data;
using AutoDiffusion.Models;
using Microsoft.Data.SqlClient;

namespace AutoDiffusion.Services
{
    public class ConfigService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _dbConfiguration;
        public WordParametersModel Config { get; set; }

        public ConfigService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _dbConfiguration = configuration;
            Config = new WordParametersModel();
        }

        public Task<WordParametersModel> GetConfig()
        {
            return Task.FromResult(new WordParametersModel
            {
                SelectedLanguage = Config.SelectedLanguage,
                SelectedCategory = Config.SelectedCategory,
                MinLetters = Config.MinLetters,
                MaxLetters = Config.MaxLetters,
                SupportedLanguages = Config.SupportedLanguages,
                SupportedCategories = Config.SupportedCategories,
            });
        }

        public async Task LoadConfiguration()
        {
            try
            {
                using var connection = new SqlConnection(_dbConfiguration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();

                string query = "SELECT * FROM WordParameters WHERE Language = @SelectedLanguage AND Categorie = @SelectedCategory";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@SelectedLanguage", string.IsNullOrEmpty(Config.SelectedLanguage) ? "French" : Config.SelectedLanguage);
                    cmd.Parameters.AddWithValue("@SelectedCategory", string.IsNullOrEmpty(Config.SelectedCategory) ? "Female" : Config.SelectedCategory);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Config.MinLetters = reader["MinLetters"] is DBNull ? 2 : Convert.ToInt32(reader["MinLetters"]);
                            Config.MaxLetters = reader["MaxLetters"] is DBNull ? 10 : Convert.ToInt32(reader["MaxLetters"]);
                            Config.AccentModifier = reader["AccentModifier"] is DBNull ? 0.1 : Convert.ToDouble(reader["AccentModifier"]);
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async Task LoadCategories()
        {
            Config.SupportedLanguages = (await LoadCategories("Language")).Where(lang => !string.IsNullOrEmpty(lang) && lang != "DEFAULT").ToList();
            Config.SupportedCategories = (await LoadCategories("Categorie")).Where(cat => !string.IsNullOrEmpty(cat) && cat != "DEFAULT").ToList();
            Config.SelectedLanguage = Config.SupportedLanguages.FirstOrDefault();
            Config.SelectedCategory = Config.SupportedCategories.FirstOrDefault();
        }

        private async Task<List<string>> LoadCategories(string columnName)
        {
            string query = $"SELECT DISTINCT {columnName} FROM WordParameters";

            await using var connection = new SqlConnection(_dbConfiguration.GetConnectionString("DefaultConnection"));
            connection.Open();
            await using (SqlCommand command = new SqlCommand(query, connection))
            await using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                List<string> values = new List<string>();

                while (reader.Read())
                {
                    string value = reader[0] != DBNull.Value ? reader.GetString(0) : null;
                    values.Add(value);
                }
                return values;
            }
        }

        public async Task CreateDefaultConfigAsync(string language)
        {
            List<string> categories = new List<string> { "Male", "Female", "Last" };
            List<WordParametersModel> defaultConfigs = new List<WordParametersModel>();

            foreach (string category in categories)
            {
                WordParametersModel defaultConfig = new WordParametersModel
                {
                    SelectedLanguage = language,
                    SelectedCategory = category,
                    MinLetters = 4,
                    MaxLetters = 11,
                    AccentModifier = 0,
                    FullName = 0,
                    FullPlaceName = 0
                };
                defaultConfigs.Add(defaultConfig);
            }

            await _context.WordParameters.AddRangeAsync(defaultConfigs);
            await _context.SaveChangesAsync();
        }
    }
}
