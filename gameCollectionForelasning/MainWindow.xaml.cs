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
            FillStackPanelWithGenres();
        }

        private async void FillStackPanelWithGenres()
        {
            spGenres.Children.Clear();
            List<Genre> genres = await _dbRepo.GetAllGenres();

            foreach (Genre genre in genres)
            {
                CheckBox cb = new CheckBox
                {
                    Content = genre.Name,
                    Tag = genre.Id,
                    Margin = new Thickness(5)
                };

                spGenres.Children.Add(cb);
            }
        }

        private async void FillStackPanelWithGames()
        {
            spGames.Children.Clear();
            List<GameStackPanelViewModel> games = await _dbRepo.GetAllGameSPVM();

            foreach (GameStackPanelViewModel g in games)
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

            GameStackPanelViewModel gamespvm = (GameStackPanelViewModel)button.Tag;

            if(gamespvm != null)
            {
                //Game game = await _dbRepo.GetGameByIdAsync(gamespvm.Id);
                GameDetailViewModel gameDetailViewModel = await _dbRepo.GetGameDetailViewModelById(gamespvm.Id);
                RefreshGameDataToUI(gameDetailViewModel);
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
            //List<int> gameIds = await _dbRepo.GetAllGameIDs();
            //int index = random.Next(gameIds.Count);

            //int randomId = gameIds[index];

            //Game game = await _dbRepo.GetGameByIdAsync(randomId);
            //_currentGame = game;
            //RefreshGameDataToUI(game);
        }
        private async void RefreshGameDataToUI(GameDetailViewModel gameViewModel)
        {
            _currentGameId = gameViewModel.Id;
            txtGameName.Text = gameViewModel.Name;
            txtValue.Text = gameViewModel.Value.ToString();
            txtHighscore.Text = gameViewModel.Highscore.ToString();
            txtPurchaseDate.Text = gameViewModel.PurchaseDate?.ToString("yyyy-MM-dd");
            txtImageURL.Text = gameViewModel.ImageUrl;

            imgGameBoxart.Source = new BitmapImage(new Uri(gameViewModel.ImageUrl));

            cbConsoles.SelectedValue = gameViewModel.ConsoleID;
            cbDeveloper.SelectedValue = gameViewModel.DeveloperID;
            cbPublisher.SelectedValue = gameViewModel.PublisherID;

            foreach (CheckBox cb in spGenres.Children)
            {
                cb.IsChecked = false;

                if (gameViewModel.Genres.Any(x => x.Id == (int)cb.Tag))
                {
                    cb.IsChecked = true;
                }
            }

        }
        private async void btnCreateCompany_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCompanyName.Text.Length > 0)
                {
                    Company company = new Company { Name = txtCompanyName.Text };
                    await _dbRepo.CreateNewCompany(company);
                    MessageBox.Show($"{company.Name} är nu tillagd i databasen.");
                    FillComboboxes();
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
                    MessageBox.Show($"Spelet togs inte bort...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Något blev fel: {ex.Message}");
            }
        }
    }
}