using gameCollectionForelasning.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gameCollectionForelasning.repositories;
using gameCollectionForelasning.ViewModels;

namespace gameCollectionForelasning
{
    public class GameService :DbRepository
    {
        DbRepository _dbRepo = new DbRepository();

        public async Task<GameDetailViewModel?> GetGameDetailViewModelById(int gameId)
        {
            var gameTask = _dbRepo.GetGameByIdAsync(gameId);
            var genresTask = _dbRepo.GetGenresForGame(gameId);

            await Task.WhenAll(gameTask, genresTask); // Kör båda anropen parallellt

            var game = gameTask.Result;
            if (game == null)
            {
                return null; // Tidig retur istället för att kapsla koden i en if-sats
            }

            return new GameDetailViewModel
            {
                Id = game.Id,
                Name = game.Name,
                Value = game.Value,
                Highscore = game.Highscore,
                PurchaseDate = game.PurchaseDate,
                ImageUrl = game.ImageUrl,
                ConsoleID = game.ConsoleID,
                DeveloperID = game.DeveloperID,
                PublisherID = game.PublisherID,
                Genres = genresTask.Result // Tilldelar genres direkt
            };
        }
                
    }
}
