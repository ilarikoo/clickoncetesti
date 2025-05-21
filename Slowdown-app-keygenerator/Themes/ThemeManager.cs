using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Slowdown_app_keygenerator.Themes
{
    class ThemeManager
    {
        /// <summary>
        /// Ottaa käyttöän string muuttujalla annetun teeman.
        /// </summary>
        /// <param name="themeName"></param>
        public static void ApplyTheme(string themeName)
        {
            var dict = new ResourceDictionary(); //Uusi ResourceDictionary luodaan, johon ladataan teema
            dict.Source = new Uri($"Themes/{themeName}.xaml", UriKind.Relative); //Määritetään teeman sijainti

            Application.Current.Resources.MergedDictionaries.Clear(); //Tyhjennetään nykyiset resurssit, jotta uusi teema voidaan ladata
            Application.Current.Resources.MergedDictionaries.Add(dict); //Lisätään uusi teema resurssit
        }
    }
}
