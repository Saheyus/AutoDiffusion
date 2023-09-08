using Microsoft.Data.SqlClient;
using AutoDiffusion.Models;

namespace AutoDiffusion.Services
{
    public class ConfigService
    {
        private readonly IConfiguration _DBConfiguration;
        public ConfigModel Config { get; set; }

        public ConfigService(IConfiguration configuration)
        {
            _DBConfiguration = configuration;
            Config = new ConfigModel();
        }

        public ConfigModel GetConfig()
        {
            return new ConfigModel
            {
                SelectedLanguage = Config.SelectedLanguage,
                SelectedCategory = Config.SelectedCategory,
                MinLetters = Config.MinLetters,
                MaxLetters = Config.MaxLetters,
                SupportedLanguages = Config.SupportedLanguages,
                SupportedCategories = Config.SupportedCategories,
            };
        }

        public void LoadConfiguration()
        {
            using var connection = new SqlConnection(_DBConfiguration.GetConnectionString("DefaultConnection"));
            connection.Open();
            string query = "SELECT * FROM WordParameters WHERE Langue = @SelectedLanguage AND Categorie = @SelectedCategory";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@SelectedLanguage", string.IsNullOrEmpty(Config.SelectedLanguage) ? "France" : Config.SelectedLanguage);
                cmd.Parameters.AddWithValue("@SelectedCategory", string.IsNullOrEmpty(Config.SelectedCategory) ? "Female" : Config.SelectedCategory);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Config.MinLetters = reader["MinLetters"] is DBNull ? 2 : Convert.ToInt32(reader["MinLetters"]);
                        Config.MaxLetters = reader["MaxLetters"] is DBNull ? 10 : Convert.ToInt32(reader["MaxLetters"]);
                        Config.AccentModifier = reader["AccentModifier"] is DBNull ? 0.1 : Convert.ToDouble(reader["AccentModifier"]);
                    }
                }
            }
            connection.Close();
        }

        public void LoadCategories()
        {
            Config.SupportedLanguages = LoadCategories("Langue").Where(lang => !string.IsNullOrEmpty(lang) && lang != "DEFAULT").ToList();
            Config.SupportedCategories = LoadCategories("Categorie").Where(cat => !string.IsNullOrEmpty(cat) && cat != "DEFAULT").ToList();
            Config.SelectedLanguage = Config.SupportedLanguages.FirstOrDefault();
            Config.SelectedCategory = Config.SupportedCategories.FirstOrDefault();
        }
        public List<string> LoadCategories(string columnName)
        {
            using var connection = new SqlConnection(_DBConfiguration.GetConnectionString("DefaultConnection"));
            connection.Open();
            string query = $"SELECT DISTINCT {columnName} FROM WordParameters";

            using (connection)
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
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
        }
    }
}
