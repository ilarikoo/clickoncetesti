using System.Globalization;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Slowdown_app_keygenerator;

namespace Slowdown_app_keygenerator
{



    public partial class MainWindow : Window
    {

        private List<Key> allKeys = new();
        private int currentPage = 0;
        private const int itemsPerPage = 15;

        private string currentSortColumn = "CourseName";
        private bool isSortAscending = true;
        private bool _isComboBoxInitialized = false;


        public MainWindow()
        {
            InitializeComponent();

            int savedDelay = Properties.Settings.Default.DelaySeconds; // Valittu hidastusaika tallentuu
           
            // Sopivan ComboBox- elementin asentaminen
            foreach (ComboBoxItem item in DelayComboBox.Items)
            {
                if (item.Tag != null && int.TryParse(item.Tag.ToString(), out int tagValue))
                {
                    if (tagValue == savedDelay)
                    {
                        DelayComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            // Kun sovellus avataan, käyttäjä ei ole kirjautunut sisään ja pääikkuna on piilotettu
            App.Login = false;

            this.Visibility = Visibility.Hidden;

            var loginWindow = new LoginWindow();

            loginWindow.ShowDialog();            

            if (App.Login == true) {
                this.Visibility = Visibility.Visible;
                
                // Avaa uuden avaimen luontinäkymän oletuksena
                ShowUusiAvain(null, null);
                
            }
            else
            {
                this.Close();
            }
            
        }

        //Kustomisoidun yläpalkin napit
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        //Ikkunaa voidaan siirtää
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        //Ikkunan maksimointi ja minimointi
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaximizeButton.Content = "🗖";
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaximizeButton.Content = "🗗";
            }
        }

        //Avaa teeman asetukset
        private void ThemeSettings_Click(object sender, RoutedEventArgs e)
        {
            var themeWindow = new SetThemeWindow();
            themeWindow.ShowDialog();
        }

        //Välilehdet
        private void ShowUusiAvain(object sender, RoutedEventArgs e)
        {
            UusiAvainContent.Visibility = Visibility.Visible;
            KurssienLuetteloContent.Visibility = Visibility.Collapsed;
            newKeyHighlighter.Visibility = Visibility.Visible;
            keylistHighlighter.Visibility = Visibility.Hidden;
        }

        private async void ShowKurssienLuettelo(object sender, RoutedEventArgs e)
        {
            UusiAvainContent.Visibility = Visibility.Collapsed;
            KurssienLuetteloContent.Visibility = Visibility.Visible;
            newKeyHighlighter.Visibility = Visibility.Hidden;
            keylistHighlighter.Visibility = Visibility.Visible;

            await LoadKeysAsync();
        }



        //käyttäjältä pyydettävä avaimen nimi 
        private async void GenerateKey_Click(object sender, RoutedEventArgs e)
        {
            string courseName = CourseNameInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(courseName))
            {
                MessageBox.Show("Syötä session nimi.", "Puuttuva tieto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string key = await KeyGenerator.GenerateRawCourseKeyAndSendAsync(courseName);
            KeyOutputBox.Text = key;
        }

        //etusivun kopiointinappula
        private void CopyKey_Click(object sender, RoutedEventArgs e)
        {
            string key = KeyOutputBox.Text;

            if (!string.IsNullOrWhiteSpace(key))
            {
                Clipboard.SetText(key);
                MessageBox.Show("Avain kopioitu leikepöydälle!", "Kopioitu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ei avainta kopioitavaksi.", "Virhe", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }      

        //avainten haku listausta varten
        private async Task LoadKeysAsync()
        {
            using HttpClient client = new();

            
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", App.Token);

            var response = await client.GetAsync($"{ApiConfig.BaseUrl}/Data/showkeyList");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };
                
                options.Converters.Add(new DateOnlyJsonConverter());

                allKeys = JsonSerializer.Deserialize<List<Key>>(json, options) ?? new();
                
                UpdateKeyListView();
            }
        }

        //listausnäkymän päivitys
        private void UpdateKeyListView()
        {
            var searchText = SearchBox.Text.ToLower();
            var filtered = allKeys
                .Where(k => k.CourseName.ToLower().Contains(searchText))
                .ToList();

            IEnumerable<Key> sortedKeys = filtered;

            switch (currentSortColumn)
            {
                case "CourseName":
                    sortedKeys = isSortAscending
                        ? sortedKeys.OrderBy(k => k.CourseName)
                        : sortedKeys.OrderByDescending(k => k.CourseName);
                    break;
                case "Expires":
                    sortedKeys = isSortAscending
                        ? sortedKeys.OrderBy(k => k.Expires)
                        : sortedKeys.OrderByDescending(k => k.Expires);
                    break;
                case "IsValid":
                    sortedKeys = isSortAscending
                        ? sortedKeys.OrderBy(k => k.IsValid)
                        : sortedKeys.OrderByDescending(k => k.IsValid);
                    break;
            }

            if (currentPage * itemsPerPage >= filtered.Count && currentPage > 0)
            {
                currentPage--;
            }

            var visibleKeys = sortedKeys
                .Skip(currentPage * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            int shownCount = Math.Min((currentPage + 1) * itemsPerPage, filtered.Count);

            PageInfoText.Text = $"Sivu {currentPage + 1} / {Math.Max(1, (int)Math.Ceiling((double)filtered.Count / itemsPerPage))}";
            ItemCountText.Text = $"Näytetty {shownCount} / {filtered.Count} avainta";

            KeyListView.ItemsSource = visibleKeys;
        }


        //sortteeraus otsikkoa painamalla
        private void SortByColumn(object sender, RoutedEventArgs e)
        {
            if (sender is GridViewColumnHeader header && header.Tag is string column)
            {
                if (column == currentSortColumn)
                {
                    // Jos sama sarake uudestaan → vaihda järjestyssuunta
                    isSortAscending = !isSortAscending;
                }
                else
                {
                    currentSortColumn = column;
                    isSortAscending = true;
                }

                UpdateKeyListView();
            }
        }

        //sivunvaihto listauksessa
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage + 1) * itemsPerPage < allKeys.Count)
            {
                currentPage++;
                UpdateKeyListView();
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdateKeyListView();
            }
        }

        //liittyy hakukenttään listaussivulla ja miten niitä tuloksia filteröidään
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            currentPage = 0; // Resetoi sivun ensimmäiseksi haussa

            var filtered = allKeys
                .Where(k => k.CourseName.ToLower().Contains(searchText))
                .OrderByDescending(k => k.CourseName.ToLower().StartsWith(searchText))
                .ThenBy(k => k.CourseName) 
                .Skip(currentPage * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            KeyListView.ItemsSource = filtered;
            PageInfoText.Text = $"Sivu {currentPage + 1} / {Math.Max(1, (int)Math.Ceiling((double)filtered.Count / itemsPerPage))}";
        }

        //listauksessa oleva hakukentän tyhjennys
        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            currentPage = 0;
            UpdateKeyListView();
        }

        //listauksessa käytettävä poistonappula
        private async void DeleteKey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int id))
            {
                var result = MessageBox.Show("Haluatko varmasti poistaa avaimen?", "Vahvista", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    using HttpClient client = new();

                    //Tokenin lisäys
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", App.Token);

                    var response = await client.DeleteAsync($"{ApiConfig.BaseUrl}/Data/deletekey?id={id}");
                    if (response.IsSuccessStatusCode)
                        await LoadKeysAsync();
                    else
                        MessageBox.Show("Avaimen poisto epäonnistui.");
                }
            }
        }

        //listauksessa käytettävä kopiointinappula
        private void CopyFromList_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string key && !string.IsNullOrWhiteSpace(key))
            {
                Clipboard.SetText(key);
                MessageBox.Show("Avain kopioitu leikepöydälle!", "Kopioitu", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



        // Hidastuksen keston säätäminen 
        private async void DelayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (!_isComboBoxInitialized)
            {
                _isComboBoxInitialized = true;
                return; // Estetään ensimmäinen automaattinen laukaisu
            }

            if (DelayComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Tag.ToString(), out int selectedDelay))
                {
                 
                    Properties.Settings.Default.DelaySeconds = selectedDelay;
                    Properties.Settings.Default.Save();

                    
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {

                            client.BaseAddress = new Uri(ApiConfig.BaseUrl);

                            var json = JsonSerializer.Serialize(selectedDelay);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            HttpResponseMessage response = await client.PostAsync("/api/classroomstatistics/update-delay", content);
                            
                            if (response.IsSuccessStatusCode)
                            {
                                MessageBox.Show($"Viive asetettu palvelimelle: {selectedDelay} sekuntia");
                            }
                            else
                            {
                                MessageBox.Show($"Virhe: {response.StatusCode}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Virhe palvelimeen yhdistettäessä: " + ex.Message);
                    }



                }
            }
        }



    }

        //Nämä on konverttereita bool-arvoille. Saadaan tietyt avaimen arvot näkymään järkevästi siinä listauksessa

        public class BoolToStatusTextConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool isValid)
                    return isValid ? "Voimassa" : "Vanhentunut";
                return "Tuntematon";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class BoolToColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool isValid)
                {
                    return isValid ? Brushes.Green : Brushes.Red;
                }

                return Brushes.Gray;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        // muuttaa avaimen vanhenemisarvoa avainlistaukseen
        // ei näy vanhenemisajankohta "jenkkiläisittäin"
         public class DateOnlyToFinnishFormatConverter : IValueConverter
            {
                 public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                     {
                        if (value is DateOnly dateOnly)
                        {
                        return dateOnly.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                        }
                        return value?.ToString();
                     }

                public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                     {
                         throw new NotImplementedException();
                     }
            }



}