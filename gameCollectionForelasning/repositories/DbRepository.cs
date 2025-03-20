using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gameCollectionForelasning.Models;
using gameCollectionForelasning.ViewModels;
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
                using var conn = await GetNewConnection();

                using var command = new NpgsqlCommand("insert into company(name)" +
                                                        "values(@company_name)", conn);

                command.Parameters.AddWithValue("company_name", company.Name);

                var result = await command.ExecuteNonQueryAsync();
            }
            catch (PostgresException pgEx)
            {
                switch (pgEx.SqlState)
                {
                    case "23505":
                        //Unique-constraint som är brutet
                        throw new Exception($"Bolagsnamn får bara förekomma en gång i databasen och {company.Name} förekommer redan.", pgEx);
                    case "23503":
                        //ForeignKey-constraint
                        throw new Exception($"Ett fel har inträffat med någon främmande nyckel.", pgEx);
                    case "23514":
                        //Check-constraints
                        throw new Exception($"Ett check-constraint har brutits.", pgEx);
                    default:
                        throw new Exception($"Ett postgresqlfel inträffade", pgEx);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<bool> DeleteGame(Game game)
        {
            try
            {
                using var conn = await GetNewConnection();

                using var command = new NpgsqlCommand("delete from game " +
                                                        "where id=@id", conn);

                command.Parameters.AddWithValue("id", game.Id);

                return await command.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception)  
            {
                throw;
            }
        }
        public async Task<bool> UpdateGame(Game game)
        {
            try
            {
                using NpgsqlConnection conn = await GetNewConnection();

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
            catch (PostgresException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }            
        }
        private async Task<NpgsqlConnection> GetNewConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }
        public async Task<List<Company>> GetAllCompanies()
        {
            try
            {
                List<Company> companies = new List<Company>();
                using var conn = await GetNewConnection();
                using var command = new NpgsqlCommand("select id, name from company", conn);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Company company = new Company
                        {
                            Id = (int)reader["id"],
                            Name = reader["name"].ToString()
                        };
                        companies.Add(company);
                    }
                }
                return companies;
            }
            catch(Exception)
            {
                throw;
            }
        }
        public async Task<List<Genre>> GetAllGenres()
        {
            try
            {
                List<Genre> genres = new List<Genre>();
                using var conn = await GetNewConnection();               

                using var command = new NpgsqlCommand("select id, name from genre", conn);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Genre genre = new Genre
                        {
                            Id = (int)reader["id"],
                            Name = reader["name"].ToString()
                        };
                        genres.Add(genre);
                    }
                }
                return genres;            
            }
            catch(Exception)
            {
                throw;
            }
        }
        public async Task<List<Models.Console>> GetAllConsoles()
        {
            try
            {
                List<Models.Console> consoles = new List<Models.Console>();
                using var conn = await GetNewConnection();

                using var command = new NpgsqlCommand("select id, name from console", conn);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Models.Console console = new Models.Console
                        {
                            Id = (int)reader["id"],
                            Name = reader["name"].ToString()
                        };
                        consoles.Add(console);
                    }
                }
                return consoles;
            }            
            catch(Exception)
            {
                throw;
            }
        }        
        public async Task<List<int>> GetAllGameIDs()
        {
            try
            {

                List<int> gameIds = new List<int>();
                using var conn = await GetNewConnection();

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
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Game> GetGameByIdAsync(int id)
        {
            try
            {
                Game game = null;
                using var conn = await GetNewConnection();

                using var command = new NpgsqlCommand("select id, name, value, image_url, highscore, purchase_date, console_id, developer_id, publisher_id " +
                                                        "from game " +
                                                        "where id=@id", conn);

                command.Parameters.AddWithValue("id", id);

                using (var reader = await command.ExecuteReaderAsync())
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
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Genre>> GetGenresForGame(int gameId)
        {
            try
            {
                List<Genre> genres = new List<Genre>();
                using var conn = await GetNewConnection();

                using var genreCommando = new NpgsqlCommand("select gg.genre_id, " +
                                                                "ge.name " +
                                                                "from game_genre gg " +
                                                                "join genre ge on ge.id = gg.genre_id " +
                                                                "where gg.game_id = @id", conn);

                genreCommando.Parameters.AddWithValue("id", gameId);

                using (var genreReader = await genreCommando.ExecuteReaderAsync())
                {
                    while (await genreReader.ReadAsync())
                    {
                        Genre genre = new Genre
                        {
                            Id = (int)genreReader["genre_id"],
                            Name = genreReader["name"].ToString()
                        };
                        genres.Add(genre);
                    }
                }

                return genres;
            }
            catch(Exception)
            {
                throw;
            }
        }
        public async Task<List<GameStackPanelViewModel>> GetAllGameSPVM()
        {
            try
            {
                List<GameStackPanelViewModel> games = new List<GameStackPanelViewModel>();
                using var conn = await GetNewConnection();

                using var command = new NpgsqlCommand("select id, image_url from game", conn);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GameStackPanelViewModel gameSPVM = new GameStackPanelViewModel
                        {
                            Id = (int)reader["id"],
                            ImageURL = reader["image_url"].ToString()
                        };

                        games.Add(gameSPVM);
                    }
                }
                return games;
            }
            catch (Exception)
            {
                throw;
            }
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
