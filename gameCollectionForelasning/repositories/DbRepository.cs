using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gameCollectionForelasning.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace gameCollectionForelasning.repositories
{
    public class DbRepository
    {
        private readonly string _connectionString;
        public DbRepository()
        {
            var config = new ConfigurationBuilder()
                            .AddUserSecrets<DbRepository>()
                            .Build();

            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task CreateNewCompany(Company company)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var command = new NpgsqlCommand("insert into company(name)" +
                                                        "values(@company_name)", conn);

                command.Parameters.AddWithValue("company_name", company.Name);

                var result = await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteGame(Game game)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var command = new NpgsqlCommand("delete from game " +
                                                        "where id=@id", conn);

                command.Parameters.AddWithValue("id", game.Id);

                return await command.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)  
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateGame(Game game)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var command = new NpgsqlCommand("update game " +
                                                        "set value=@value, " +
                                                        "name=@name, " +
                                                        "image_url=@url, " +
                                                        "purchase_date=@date, " +
                                                        "highscore=@highscore " +
                                                        "where id=@id", conn);

                command.Parameters.AddWithValue("value", game.Value);
                command.Parameters.AddWithValue("name", game.Name);
                command.Parameters.AddWithValue("url", game.ImageUrl);
                command.Parameters.AddWithValue("date", game.PurchaseDate);
                command.Parameters.AddWithValue("highscore", ConvertToDBVal<int?>(game.Highscore));
                command.Parameters.AddWithValue("id", game.Id);

                return await command.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }

        public async Task<List<int>> GetAllGameIDs()
        {
            List<int> gameIds = new List<int>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var command = new NpgsqlCommand("select id from game", conn);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = (int)reader["id"];
                    gameIds.Add(id);
                }
            }
            return gameIds;
        }

        public async Task<Game> GetGameByIdAsync(int id)
        {
            Game game = null;
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
                        
            using var command = new NpgsqlCommand("select id, name, value, image_url, highscore, purchase_date, console_id, developer_id, publisher_id " +
                                                    "from game " +
                                                    "where id=@id", conn);

            command.Parameters.AddWithValue("id", id);            

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    game = new Game
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        Value = (double)reader["value"],
                        ImageUrl = reader["image_url"].ToString(),
                        Highscore = ConvertFromDBVal<int?>(reader["highscore"]),
                        PurchaseDate = (DateTime)reader["purchase_date"],
                        ConsoleID = (int)reader["console_id"],
                        DeveloperID = (int)reader["developer_id"],
                        PublisherID = (int)reader["publisher_id"]
                    };
                }
            }

            return game;
        }


        private static T? ConvertFromDBVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default; // returns the default value for the type
            }
            return (T)obj;
        }

        private static object ConvertToDBVal<T>(object obj)
        {
            if (obj == null || obj == string.Empty)
            {
                return DBNull.Value;
            }
            return (T)obj;
        }
    }
}
