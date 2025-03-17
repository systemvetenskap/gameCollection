using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gameCollectionForelasning.Models;
using gameCollectionForelasning.repositories;

namespace gameCollectionForelasning
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DbRepository _dbRepo = new DbRepository();
        Random random = new Random();
        int? _currentGameId;
        Game _currentGame;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnGetGame_Click(object sender, RoutedEventArgs e)
        {
            List<int> gameIds = await _dbRepo.GetAllGameIDs();
            int index = random.Next(gameIds.Count);

            int randomId = gameIds[index];

            Game game = await _dbRepo.GetGameByIdAsync(randomId);
            _currentGame = game;
            RefreshGameDataToUI(game);
        }

        private async void RefreshGameDataToUI(Game game)
        {
            _currentGameId = game.Id;
            txtGameName.Text = game.Name;
            txtValue.Text = game.Value.ToString();
            txtHighscore.Text = game.Highscore.ToString();
            txtPurchaseDate.Text = game.PurchaseDate?.ToString("yyyy-MM-dd");
            txtImageURL.Text = game.ImageUrl;

            imgGameBoxart.Source = new BitmapImage(new Uri(game.ImageUrl));
        }

        private async void btnCreateCompany_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCompanyName.Text.Length > 0)
                {
                    Company company = new Company { Name = txtCompanyName.Text };
                    await _dbRepo.CreateNewCompany(company);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Aj aj det blev något fel: {ex.Message}");
            }            
        }

        

        private async void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGameId == null)
                return;

            try
            {                
                Game game = new Game
                {
                    Id = (int)_currentGameId,
                    Name = txtGameName.Text,
                    Value = double.Parse(txtValue.Text),
                    PurchaseDate = DateTime.Parse(txtPurchaseDate.Text),
                    Highscore = txtHighscore.Text.Length > 0 ? int.Parse(txtHighscore.Text) : null,
                    ImageUrl = txtImageURL.Text
                };

                bool updateDone = await _dbRepo.UpdateGame(game);
                if (updateDone)
                    MessageBox.Show($"Det gick bra, nu är allt sparat!");
                else
                    MessageBox.Show($"Ingen ändring gjordes... :( ");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Något blev fel: {ex.Message}");
            }
        }

        private async void btnDeleteGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isGameDeleted = await _dbRepo.DeleteGame(_currentGame);

                if (isGameDeleted)
                    MessageBox.Show($"Nu är spelet borta ur din samling!");
                else
                    MessageBox.Show($"Inget hände...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Något blev fel: {ex.Message}");
            }
        }
    }
}