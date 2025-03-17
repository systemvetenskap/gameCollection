using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gameCollectionForelasning.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double? Value { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int? Highscore { get; set; }
        public int ConsoleID { get; set; }
        public int DeveloperID { get; set; }
        public int PublisherID { get; set; }
    }
}
