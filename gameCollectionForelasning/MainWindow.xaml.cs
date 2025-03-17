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
using gameCollectionForelasning.Models.ViewModels;
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
            FillComboboxes();
            FillStackPanelWithGames();
        }

        private async void FillStackPanelWithGames()
        {
            spGames.Children.Clear();
            List<GameSPVM> games = await _dbRepo.GetAllGameSPVM();

            foreach (GameSPVM g in games)
            {
                Button btn = new Button
                {
                    Width = 60,
                    Height = 120,
                    Margin = new Thickness(3),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Bottom
                };

                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(g.ImageURL, UriKind.RelativeOrAbsolute)),
                    Width = 60,
                    Height = 120,
                    VerticalAlignment = VerticalAlignment.Bottom
                };

                btn.Content = img;
                btn.Tag = g;
                btn.Click += GameImage_Click;

                spGames.Children.Add(btn);
            }
        }

        private async void GameImage_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            GameSPVM gamespvm = (GameSPVM)button.Tag;

            if(gamespvm != null)
            {
                Game game = await _dbRepo.GetGameByIdAsync(gamespvm.Id);
                RefreshGameDataToUI(game);
            }
        }

        private async void FillComboboxes()
        {
            List<Company> companies = await _dbRepo.GetAllCompanies();
            List<Models.Console> consoles = await _dbRepo.GetAllConsoles();

            FillCombobox<Models.Console>(cbConsoles, consoles);
            FillCombobox<Company>(cbDeveloper, companies);
            FillCombobox<Company>(cbPublisher, companies);
        }

        private async void FillCombobox<T>(ComboBox cb, List<T> list)
        {
            cb.ItemsSource = list;
            cb.DisplayMemberPath = "Name";
            cb.SelectedValuePath = "Id";
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