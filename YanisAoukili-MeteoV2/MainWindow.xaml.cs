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
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.RightsManagement;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace YanisAoukili_MeteoV2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GetWeather("Annecy");
            LoadFavorites();
        }

        string FavoritesFilePath = "VilleFavoris.txt";

        public async Task GetWeather(string cityurl)
        {
            HttpClient client = new HttpClient();
            string url = $"https://www.prevision-meteo.ch/services/json/{cityurl}";
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                //  verifie l'api 
                if (string.IsNullOrEmpty(content))
                {
                    MessageBox.Show("Vide");
                    return;
                }

                Root root;

                // Trouve des erreurs de  deserialisation
                try
                {
                    root = JsonConvert.DeserializeObject<Root>(content);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur de désérialisation : {ex.Message}");
                    return;
                }

                // recup les class
                CityInfo city_info = root.city_info;
                CurrentCondition current_condition = root.current_condition;
                FcstDay1 fcst_fday = root.fcst_day_1;
                FcstDay2 fcst_sday = root.fcst_day_2;
                FcstDay3 fcst_tday = root.fcst_day_3;

                // temp de la ville
                var city = city_info.name;
                var temp = current_condition.tmp;

                //  jour de la semaine 
                var fday = fcst_fday.day_long;
                var sday = fcst_sday.day_long;
                var tday = fcst_tday.day_long;

                //  Previsions des temp 
                var fday_tempmin = fcst_fday.tmin;
                var sday_tempmin = fcst_sday.tmin;
                var tday_tempmin = fcst_tday.tmin;
                var fday_tempmax = fcst_fday.tmax;
                var sday_tempmax = fcst_sday.tmax;
                var tday_tempmax = fcst_tday.tmax;

                //  ICONs metto

                string current_condition_icon = current_condition.icon_big;
                Uri asset0 = new Uri(current_condition_icon);
                ImgDay.Source = new BitmapImage(asset0);
                string fday_icon = fcst_fday.icon_big;
                Uri asset1 = new Uri(fday_icon);
                ImgDay1.Source = new BitmapImage(asset1);
                string sday_icon = fcst_sday.icon_big;
                Uri asset2 = new Uri(sday_icon);
                ImgDay2.Source = new BitmapImage(asset2);
                string tday_icon = fcst_tday.icon_big;
                Uri asset3 = new Uri(tday_icon);
                ImgDay3.Source = new BitmapImage(asset3);

                // Texte box affichage 
                TB_City.Text = city;
                TB_Temp.Text = temp.ToString() + "°";
                TB_Fcst_Fday.Text = fday;
                TB_Fcst_Sday.Text = sday;
                TB_Fcst_Tday.Text = tday;
                TB_Tempmin_Fday.Text = fday_tempmin.ToString() + "°";
                TB_Tempmin_Sday.Text = sday_tempmin.ToString() + "°";
                TB_Tempmin_Tday.Text = tday_tempmin.ToString() + "°";
                TB_Tempmax_Fday.Text = fday_tempmax.ToString() + "°";
                TB_Tempmax_Sday.Text = sday_tempmax.ToString() + "°";
                TB_Tempmax_Tday.Text = tday_tempmax.ToString() + "°";

            }
            return;
        }

        // event pour ajouter une ville dans la textbox
        private void BTN_Add_Click(object sender, RoutedEventArgs e)
        {
            string newCity = TB_EnterCity.Text.Trim();

            if (!string.IsNullOrEmpty(newCity))
            {
                if (!CB_Cities.Items.Contains(newCity))
                {
                    CB_Cities.Items.Add(newCity);
                    AddCityToFile(newCity);
                    TB_EnterCity.Text = string.Empty;

                    MessageBox.Show($"{newCity} a été ajoutée aux favoris !");
                }
                else
                {
                    MessageBox.Show("Cette ville est déjà dans les favoris !");
                }
            }
            else
            {
                MessageBox.Show("Veuillez entrer une ville valide !");
            }
        }

        // fonction pour ajouter la ville de la textbox dans le fichier texte
        private void AddCityToFile(string city)
        {
            try
            {
                if (!File.Exists(FavoritesFilePath))
                {
                    using (File.Create(FavoritesFilePath)) { }
                }
                using (StreamWriter sw = File.AppendText(FavoritesFilePath))
                {
                    sw.WriteLine(city);
                }
                MessageBox.Show($"Ville ajoutée au fichier : {city}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout au fichier : {ex.Message}");
            }
        }


        // FONCTION POUR CHARGER LES VILLES STOCKEES DANS LE FICHIER TEXTE DANS LA COMBOBOX
        private void LoadFavorites()
        {
            try
            {
                if (File.Exists(FavoritesFilePath))
                {
                    var cities = File.ReadAllLines(FavoritesFilePath).Distinct().ToList();

                    foreach (var city in cities)
                    {
                        if (!string.IsNullOrEmpty(city) && !CB_Cities.Items.Contains(city))
                        {
                            CB_Cities.Items.Add(city);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des favoris : {ex.Message}");
            }
        }

        private void ClearFavorites()
        {
            try
            {
                if (File.Exists(FavoritesFilePath))
                {
                    File.WriteAllText(FavoritesFilePath, string.Empty);
                    MessageBox.Show("Tous les favoris ont été supprimés !");
                }
                else
                {
                    MessageBox.Show("Aucun fichier de favoris trouvé !");
                }
                CB_Cities.Items.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression des favoris : {ex.Message}");
            }
        }

        private void BTN_Reset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer tous les favoris ?",
                                         "Confirmation",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ClearFavorites();
            }
        }

        private async void CB_Cities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CB_Cities.SelectedItem != null)
            {
                string selectedCity = CB_Cities.SelectedItem.ToString();
                await GetWeather(selectedCity);
            }
        }
    }
    public class _0H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _10H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _11H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _12H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _13H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _14H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _15H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _16H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public int RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _17H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _18H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _19H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _1H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _20H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _21H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _22H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _23H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _2H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _3H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _4H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _5H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _6H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _7H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _8H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class _9H00
    {
        public string ICON { get; set; }
        public string CONDITION { get; set; }
        public string CONDITION_KEY { get; set; }
        public double TMP2m { get; set; }
        public double DPT2m { get; set; }
        public double? WNDCHILL2m { get; set; }
        public object HUMIDEX { get; set; }
        public double RH2m { get; set; }
        public double PRMSL { get; set; }
        public double APCPsfc { get; set; }
        public double WNDSPD10m { get; set; }
        public double WNDGUST10m { get; set; }
        public double WNDDIR10m { get; set; }
        public string WNDDIRCARD10 { get; set; }
        public double ISSNOW { get; set; }
        public string HCDC { get; set; }
        public string MCDC { get; set; }
        public string LCDC { get; set; }
        public double HGT0C { get; set; }
        public double KINDEX { get; set; }
        public string CAPE180_0 { get; set; }
        public double CIN180_0 { get; set; }
    }

    public class CityInfo
    {
        public string name { get; set; }
        public string country { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string elevation { get; set; }
        public string sunrise { get; set; }
        public string sunset { get; set; }
    }

    public class CurrentCondition
    {
        public string date { get; set; }
        public string hour { get; set; }
        public int tmp { get; set; }
        public int wnd_spd { get; set; }
        public int wnd_gust { get; set; }
        public string wnd_dir { get; set; }
        public double pressure { get; set; }
        public int humidity { get; set; }
        public string condition { get; set; }
        public string condition_key { get; set; }
        public string icon { get; set; }
        public string icon_big { get; set; }
    }

    public class FcstDay0
    {
        public string date { get; set; }
        public string day_short { get; set; }
        public string day_long { get; set; }
        public int tmin { get; set; }
        public int tmax { get; set; }
        public string condition { get; set; }
        public string condition_key { get; set; }
        public string icon { get; set; }
        public string icon_big { get; set; }
        public HourlyData hourly_data { get; set; }
    }

    public class FcstDay1
    {
        public string date { get; set; }
        public string day_short { get; set; }
        public string day_long { get; set; }
        public int tmin { get; set; }
        public int tmax { get; set; }
        public string condition { get; set; }
        public string condition_key { get; set; }
        public string icon { get; set; }
        public string icon_big { get; set; }
        public HourlyData hourly_data { get; set; }
    }

    public class FcstDay2
    {
        public string date { get; set; }
        public string day_short { get; set; }
        public string day_long { get; set; }
        public int tmin { get; set; }
        public int tmax { get; set; }
        public string condition { get; set; }
        public string condition_key { get; set; }
        public string icon { get; set; }
        public string icon_big { get; set; }
        public HourlyData hourly_data { get; set; }
    }

    public class FcstDay3
    {
        public string date { get; set; }
        public string day_short { get; set; }
        public string day_long { get; set; }
        public int tmin { get; set; }
        public int tmax { get; set; }
        public string condition { get; set; }
        public string condition_key { get; set; }
        public string icon { get; set; }
        public string icon_big { get; set; }
        public HourlyData hourly_data { get; set; }
    }

    public class FcstDay4
    {
        public string date { get; set; }
        public string day_short { get; set; }
        public string day_long { get; set; }
        public int tmin { get; set; }
        public int tmax { get; set; }
        public string condition { get; set; }
        public string condition_key { get; set; }
        public string icon { get; set; }
        public string icon_big { get; set; }
        public HourlyData hourly_data { get; set; }
    }

    public class ForecastInfo
    {
        public object latitude { get; set; }
        public object longitude { get; set; }
        public string elevation { get; set; }
    }

    public class HourlyData
    {
        [JsonProperty("0H00")]
        public _0H00 _0H00 { get; set; }

        [JsonProperty("1H00")]
        public _1H00 _1H00 { get; set; }

        [JsonProperty("2H00")]
        public _2H00 _2H00 { get; set; }

        [JsonProperty("3H00")]
        public _3H00 _3H00 { get; set; }

        [JsonProperty("4H00")]
        public _4H00 _4H00 { get; set; }

        [JsonProperty("5H00")]
        public _5H00 _5H00 { get; set; }

        [JsonProperty("6H00")]
        public _6H00 _6H00 { get; set; }

        [JsonProperty("7H00")]
        public _7H00 _7H00 { get; set; }

        [JsonProperty("8H00")]
        public _8H00 _8H00 { get; set; }

        [JsonProperty("9H00")]
        public _9H00 _9H00 { get; set; }

        [JsonProperty("10H00")]
        public _10H00 _10H00 { get; set; }

        [JsonProperty("11H00")]
        public _11H00 _11H00 { get; set; }

        [JsonProperty("12H00")]
        public _12H00 _12H00 { get; set; }

        [JsonProperty("13H00")]
        public _13H00 _13H00 { get; set; }

        [JsonProperty("14H00")]
        public _14H00 _14H00 { get; set; }

        [JsonProperty("15H00")]
        public _15H00 _15H00 { get; set; }

        [JsonProperty("16H00")]
        public _16H00 _16H00 { get; set; }

        [JsonProperty("17H00")]
        public _17H00 _17H00 { get; set; }

        [JsonProperty("18H00")]
        public _18H00 _18H00 { get; set; }

        [JsonProperty("19H00")]
        public _19H00 _19H00 { get; set; }

        [JsonProperty("20H00")]
        public _20H00 _20H00 { get; set; }

        [JsonProperty("21H00")]
        public _21H00 _21H00 { get; set; }

        [JsonProperty("22H00")]
        public _22H00 _22H00 { get; set; }

        [JsonProperty("23H00")]
        public _23H00 _23H00 { get; set; }
    }

    public class Root
    {
        public CityInfo city_info { get; set; }
        public ForecastInfo forecast_info { get; set; }
        public CurrentCondition current_condition { get; set; }
        public FcstDay0 fcst_day_0 { get; set; }
        public FcstDay1 fcst_day_1 { get; set; }
        public FcstDay2 fcst_day_2 { get; set; }
        public FcstDay3 fcst_day_3 { get; set; }
        public FcstDay4 fcst_day_4 { get; set; }
    }
}


