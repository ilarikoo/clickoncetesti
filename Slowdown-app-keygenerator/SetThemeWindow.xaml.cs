//using Slowdown_app_main.Themes;
using Slowdown_app_keygenerator.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Slowdown_app_keygenerator
{
    public partial class SetThemeWindow : Window
    {
        public SetThemeWindow()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded; //Tapahtuma, joka lataa teemat ikkunan latautuessa 
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //Hae ohjelman base dir
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            //Menee kolme tasoa ylöspäin, jotta saadaan ohjelman varsinainen juurihakemisto
            var solutionRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDirectory, @"..\..\..\"));

            //Yhdistää juurihakemistoon Themes hakemistoon, jotta päästään Themes kansioon
            var themesFolderPath = System.IO.Path.Combine(solutionRoot, "Themes");

            if (Directory.Exists(themesFolderPath))
            {
                var themes = new List<dynamic>(); //Luodaan lista teemoista

                foreach (var file in Directory.GetFiles(themesFolderPath, "*.xaml")) //Käydään läpi kaikki xaml tiedostot Themes hakemistossa
                {
                    try //Yritetään ladata jokainen tiedosto
                    {
                        var rd = new ResourceDictionary { Source = new Uri(file) }; //Ladataan tiedosto ResourceDictionaryin avulla
                        var displayName = rd["ThemeDisplayName"] as string; //Haetaan teeman nimi, joka on määritelty tiedostossa ThemeDisplayName avaimen alla

                        //Lisätään teema listaan
                        themes.Add(new
                        {
                            DisplayName = displayName ?? System.IO.Path.GetFileNameWithoutExtension(file),
                            FileName = System.IO.Path.GetFileNameWithoutExtension(file)
                        });
                    }
                    catch
                    {
                        //Skippaa virheelliset tiedostot
                    }
                }

                ThemeList.ItemsSource = themes; //Asetetaan teemat listan lähteeksi
            }
            else //Mikäli hakemistoa ei löydy, näytetään virheilmoitus
            {
                MessageBox.Show("Theme directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyTheme_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var themeName = button.Tag.ToString();

            //Asettaa määritellyn teeman
            ThemeManager.ApplyTheme(themeName);

            //Tallentaa teeman ThemeSettingsin asetuksiin
            ThemeSettings.Default.Theme = themeName;
            ThemeSettings.Default.Save();
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
    }
}
