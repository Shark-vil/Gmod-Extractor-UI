using GmaExtractorLibrary;
using GmodExtractorUI.Services;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GmodExtractorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string FileSettingsPath;

        public MainWindow()
        {
            InitializeComponent();
            Storage.WindowsStorage.MainWindow = this;

            ConfigValidator();
            Events();
        }

        private void Events()
        {
            TextBox_ExtractFolder.TextChanged += TextBox_ExtractFolder_TextChanged;
            TextBox_GameFolder.TextChanged += TextBox_GameFolder_TextChanged;
            TextBox_SevenZipExe.TextChanged += TextBox_SevenZipExe_TextChanged;
            TextBox_WorkshopFolder.TextChanged += TextBox_WorkshopFolder_TextChanged;

            Button_Addons.Click += Button_Addons_Click;
        }

        private void Button_Addons_Click(object sender, RoutedEventArgs e)
        {
            var AddonsWindow = new Windows.AddonsWindow();
            Storage.WindowsStorage.AddonsWindow = AddonsWindow;

            Hide();
            AddonsWindow.Show();
        }

        private void TextBox_WorkshopFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.UpdateContentPath(TextBox_WorkshopFolder.Text);
        }

        private void TextBox_SevenZipExe_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.UpdateSevenZipExePath(TextBox_SevenZipExe.Text);
        }

        private void TextBox_GameFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.UpdateGameFolderPath(TextBox_GameFolder.Text);
        }

        private void TextBox_ExtractFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.UpdateExtractPath(TextBox_ExtractFolder.Text);
        }

        private void ConfigValidator()
        {
            ConfigManager.Initialization();

            TextBox_ExtractFolder.Text = Extractor.ExtractPath;
            TextBox_GameFolder.Text = Extractor.GameFolderPath;
            TextBox_WorkshopFolder.Text = Extractor.ContentPath;
            TextBox_SevenZipExe.Text = Extractor.SevenZipExePath;
        }
    }
}
