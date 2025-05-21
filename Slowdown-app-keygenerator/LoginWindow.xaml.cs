using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
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
    
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string key = KeyField.Password;

            try
            {
                LoginButton.IsEnabled = false;

                using HttpClient client = new HttpClient();                
                string requestUrl = $"{ApiConfig.BaseUrl}/api/Authentication/TeacherLogin";

                using StringContent jsonContent = new(JsonSerializer.Serialize(key), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(requestUrl, jsonContent);  

                if (response.IsSuccessStatusCode)
                {
                    string token = await response.Content.ReadAsStringAsync();

                    // Token tallennetaan App-luokkaan, jotta se on käytettävissä koko sovelluksessa.
                    App.Token = token;
                    App.Login = true;
                    this.Close();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    
                    ResponseText.Content = "Avain on virheellinen.";
                    LoginButton.IsEnabled = true;
                }
                else
                {
                    
                    ResponseText.Content = $"Virhe: {response.StatusCode}";
                    LoginButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                
                ResponseText.Content = $"Yhteysvirhe: {ex.Message}";
                LoginButton.IsEnabled = true;
            }


            
        }
    }
}
