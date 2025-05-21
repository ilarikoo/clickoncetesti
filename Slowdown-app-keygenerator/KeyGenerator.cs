using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Slowdown_app_keygenerator
{
    
    public static class KeyGenerator
    {
        public static async Task<string> GenerateRawCourseKeyAndSendAsync(string courseName)
        {
            // 1. Luo 16 tavua satunnaista dataa
            byte[] randomBytes = RandomNumberGenerator.GetBytes(16);
            string base64 = Convert.ToBase64String(randomBytes);

            using var client = new HttpClient();

            // 2. Luo form-data (x-www-form-urlencoded) - t‰m‰ pit‰‰ tehd‰, ett‰ kontrolleri hyv‰ksyy vastaanotettavat setit
            var values = new Dictionary<string, string>
            {
                { "base64", base64 },
                { "name", courseName }
            };

            var content = new FormUrlEncodedContent(values); 
            
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", App.Token);

            
            var response = await client.PostAsync($"{ApiConfig.BaseUrl}/api/keygen", content);


            //Jos kutsu hyl‰t‰‰n (token on vanhentunut), pyydet‰‰n uudelleenkirjautumista
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                App.LoginPrompt();
            }

            string responseText = await response.Content.ReadAsStringAsync();

                                             
            return responseText;
        }

    }
}
