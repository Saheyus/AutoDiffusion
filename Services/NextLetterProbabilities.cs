﻿using Microsoft.Data.SqlClient;
using System.Data;

namespace AutoDiffusion.Services
{
    public class NextLetterProbability
    {
        public string? LastLetters { get; init; }
        public string? NextLetter { get; init; }
        public double Probability { get; init; }
    }

    public class NextLetterProbabilities
    {
        public List<NextLetterProbability> Probabilities { get; } = new();

        public void LoadFromDatabase(SqlConnection connection, string language, string category)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            Probabilities.Clear();
            string query = $"SELECT LastLetters, NextLetter, Probability  FROM probabilities WHERE Language = '{language}' AND Type = '{category}'";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Probabilities.Add(new NextLetterProbability
                        {
                            LastLetters = reader["LastLetters"].ToString(),
                            NextLetter = reader["NextLetter"].ToString(),
                            Probability = Convert.ToDouble(reader["Probability"])
                        });
                    }
                }
            }
        }

        public List<NextLetterProbability> FindByLastLetters(string lastLetters)
        {
            // Rechercher et retourner les probabilités pour les 'LastLetters' spécifiés
            return Probabilities.FindAll(p => p.LastLetters == lastLetters);
        }
    }
}

