using KZ;
using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace NFRV
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configuration : Window
    {
        public Configuration()
        {
            InitializeComponent();
            this.webServerTxtBox.Text = ConfigurationManager.AppSettings.Get("WebServer");
        }
        public static void LoadConfiguration()
        {
            object loadedSetting = ConfigurationManager.AppSettings.Get("WebServer");
            App.Current.Properties["WebServer"] = loadedSetting;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings["WebServer"] == null)
                {
                    settings.Add("WebServer", this.webServerTxtBox.Text);
                }
                else
                {
                    settings["WebServer"].Value = this.webServerTxtBox.Text;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
            ConfigurationManager.AppSettings.Set("WebServer", this.webServerTxtBox.Text);
            this.Close();
        }
    }
}
