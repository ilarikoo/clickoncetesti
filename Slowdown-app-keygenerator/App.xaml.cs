using Slowdown_app_keygenerator.Themes;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Slowdown_app_keygenerator
{
    
    public partial class App : Application
    {
        public static bool Login;

        //Admin-toimintojen vaatiman jwt-tokenin säilytys
        public static string Token;

        //Metodi, jolla käyttäjä pyydetään kirjautumaan uudelleen. Hyödyllinen, jos token vanhenee
        public static void LoginPrompt()
        {
            //Merkitään käyttäjä uloskirjautuneeksi
            App.Login = false;

            MessageBox.Show("Kirjautumisesi on vanhentunut. Kirjaudu uudestaan.", "Uudelleenkirjautuminen vaaditaan", MessageBoxButton.OK, MessageBoxImage.Information);

            //Avaa kirjautumisikkunan
            var loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
        }

        //Laukaisee tapahtuman, kun ohjelma käynnistyy. Tällä asetetaan tallennettu teema.
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e); //Kutsuu perusluokan OnStartup metodia
            ThemeManager.ApplyTheme(ThemeSettings.Default.Theme); //Lataa teeman, joka on tallennettu asetuksiin
        }
    }

}
