using gameCollectionForelasning.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gameCollectionForelasning.ViewModels
{
    public class GameDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double? Value { get; set; }
        public int? Highscore { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string ImageUrl { get; set; }
        public int ConsoleID { get; set; }
        public int DeveloperID { get; set; }
        public int PublisherID { get; set; }
        public List<Genre> Genres { get; set; } = new List<Genre>();

    }
}
