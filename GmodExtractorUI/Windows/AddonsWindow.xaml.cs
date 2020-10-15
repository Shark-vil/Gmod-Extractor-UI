using GmodExtractorUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GmaExtractorLibrary;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using GmaExtractorLibrary.Models;
using System.Threading;
using System.Net;
using System.Windows.Threading;

namespace GmodExtractorUI.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddonsWindow.xaml
    /// </summary>
    public partial class AddonsWindow : Window
    {
        public ObservableCollection<AddonModel> AddonsCollection = new ObservableCollection<AddonModel>();
        public ObservableCollection<AddonModel> SearchCollection = new ObservableCollection<AddonModel>();
        public ObservableCollection<ComboCheckBox> FiltersCollection = new ObservableCollection<ComboCheckBox>()
        {
            new ComboCheckBox
            {
                Key = "global",
                Name = "Deep search",
                IsChecked = true
            },
            new ComboCheckBox
            {
                Key = "normal",
                Name = "Normal search",
                IsChecked = false
            },
            new ComboCheckBox
            {
                Key = "desc",
                Name = "Search in description",
                IsChecked = false
            }
        };

        private DispatcherTimer SearchTimer;
        private string OldSearchText;
        private bool SearchClose = false;
        private bool IsFirstLoading = true;
        protected internal static GmadProcessModel GmadObject = null;

        public AddonsWindow()
        {
            InitializeComponent();
            Storage.WindowsStorage.AddonsWindow = this;

            DataGrid_Addons.ItemsSource = AddonsCollection;
            ComboBox_Filters.ItemsSource = FiltersCollection;

            Events();
            InitializationAddons();
        }

        private void Events()
        {
            TextBox_Search.TextChanged += TextBox_Search_TextChanged;
            Extract_Selected.Click += Extract_Selected_Click;
            Extract_All.Click += Extract_All_Click;
            this.Closed += AddonsWindow_Closed;
        }

        private void Extract_All_Click(object sender, RoutedEventArgs e)
        {
            var t = Task.Run(delegate
            {
                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => DataGrid_Addons.IsEnabled = false);
                foreach (var AddonItem in AddonsCollection)
                    ExtractAddon(AddonItem);
                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => DataGrid_Addons.IsEnabled = true);
            });
        }

        private void Extract_Selected_Click(object sender, RoutedEventArgs e)
        {
            SyncCheckedCollection();

            var t = Task.Run(delegate
            {
                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => DataGrid_Addons.IsEnabled = false);
                foreach (var AddonItem in AddonsCollection)
                {
                    if (AddonItem.IsChecked)
                    {
                        ExtractAddon(AddonItem);
                        AddonItem.IsChecked = false;
                    }
                }
                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => DataGrid_Addons.IsEnabled = true);
            });
        }

        private void SyncCheckedCollection()
        {
            if (SearchCollection.Count != 0)
                foreach (var AddonItem in SearchCollection)
                    AddonsCollection.FirstOrDefault(x => x.Id == AddonItem.Id).IsChecked = AddonItem.IsChecked;
        }

        private void TextBox_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            OldSearchText = TextBox_Search.Text;
            StartSearchAddonTimer();
        }

        private void StartSearchAddonTimer()
        {
            if (SearchTimer != null && SearchTimer.IsEnabled)
                SearchTimer.Stop();

            SearchTimer = new DispatcherTimer();
            SearchTimer.Interval = TimeSpan.FromMilliseconds(500);
            SearchTimer.Tick += SearchAddonTimerHandler;
            SearchTimer.Start();
        }

        private void SearchAddonTimerHandler(object sender, EventArgs e)
        {
            if (TextBox_Search.Text != OldSearchText)
                return;

            SearchAddon();
            SearchTimer.Stop();
        }

        private void SearchAddon()
        {
            if (TextBox_Search.Text.Length == 0)
            {
                SyncCheckedCollection();
                DataGrid_Addons.ItemsSource = AddonsCollection;
                return;
            }

            if (IsFirstLoading)
            {
                TextBox_Search.Text = "";
                return;
            }

            try
            {
                if (Regex.IsMatch(@"^[a-zA-Z0-9\_]+", TextBox_Search.Text))
                {
                    if (TextBox_Search.Text.Length != 0)
                        TextBox_Search.Text = TextBox_Search.Text.Remove(TextBox_Search.Text.Length - 1);
                }
            }
            catch
            {
                if (TextBox_Search.Text.Length != 0)
                    TextBox_Search.Text = TextBox_Search.Text.Remove(TextBox_Search.Text.Length - 1);
            }

            SearchClose = true;
            SearchCollection.Clear();
            DataGrid_Addons.ItemsSource = SearchCollection;

            string SearchWord = TextBox_Search.Text.ToLower();
            SearchClose = false;

            var t = Task.Run(delegate
            {
                bool IsGlobal = FiltersCollection.FirstOrDefault(x => x.Key == "global").IsChecked;
                bool IsNormal = FiltersCollection.FirstOrDefault(x => x.Key == "normal").IsChecked;
                bool IsDescription = FiltersCollection.FirstOrDefault(x => x.Key == "desc").IsChecked;

                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => ProgressBar_Loading.IsIndeterminate = true);

                foreach (var AddonItem in AddonsCollection)
                {
                    if (SearchClose)
                        break;

                    if (SearchCollection.FirstOrDefault(x => x.Id == AddonItem.Id) != null)
                        continue;

                    string Pattern = @$"(\w*)({SearchWord})(\w*)";

                    if (!IsGlobal && IsNormal)
                        Pattern = @$"^({SearchWord})(\w*)";

                    if (RegexMatchEx(AddonItem.Name.ToLower(), Pattern))
                    {
                        Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => SearchCollection.Add(AddonItem));
                        continue;
                    }

                    if (IsDescription)
                        if (RegexMatchEx(AddonItem.Description.ToLower(), Pattern))
                        {
                            Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => SearchCollection.Add(AddonItem));
                            continue;
                        }

                    Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() =>
                    StatusBar_ProcessStep.Text = $"Found - {AddonItem.Name}");
                }

                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() =>
                    StatusBar_ProcessStep.Text = $"Search completed");

                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => ProgressBar_Loading.IsIndeterminate = false);
            });
        }

        private bool RegexMatchEx(string _regexInput, string _regexPattern, RegexOptions _regexOptions = RegexOptions.IgnoreCase)
        {
            try
            {
                return Regex.IsMatch(_regexInput, _regexPattern, _regexOptions);
            }
            catch
            {
                return false;
            }
        }

        private void AddonsWindow_Closed(object sender, EventArgs e)
        {
            var FindGmadProcess = Process.GetProcesses().Where(pr => pr.ProcessName == "gmad");
            foreach (var GmadProcess in FindGmadProcess)
                GmadProcess.Kill();

            Storage.WindowsStorage.MainWindow.Show();
        }

        private void InitializationAddons()
        {
            Extractor.ParseDirectory();

            var t = Task.Run(delegate
            {
                List<Extractor.ExtractData> ExtractItems = Extractor.GetContentData();

                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() =>
                {
                    ProgressBar_Loading.Minimum = 0;
                    ProgressBar_Loading.Maximum = ExtractItems.Count;
                    DataGrid_Addons.IsEnabled = false;
                });

                int index = 0;

                string ImagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string ErrorImage = System.IO.Path.Combine(ImagesDir, "error.jpg");

                string CacheImagesDir = System.IO.Path.Combine(ImagesDir, "Caches");

                if (!System.IO.Directory.Exists(CacheImagesDir))
                    System.IO.Directory.CreateDirectory(CacheImagesDir);

                foreach (var ExtractItem in ExtractItems)
                {
                    Workshop.AddonData WorkshopItem = Workshop.GetAddonData(ExtractItem.AddonUid);
                    var AddonItem = new AddonModel();

                    if (WorkshopItem.Uid != string.Empty)
                    {
                        Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => 
                            StatusBar_ProcessStep.Text = $"Loading - {WorkshopItem.Title}");

                        string CacheImage = System.IO.Path.Combine(CacheImagesDir, WorkshopItem.Uid + ".jpg");
                        string ImageUrl = (System.IO.File.Exists(CacheImage)) ? CacheImage : WorkshopItem.ImageUrl;

                        var DownloadTask = Task.Run(delegate()
                        {
                            if (ImageUrl == WorkshopItem.ImageUrl)
                            {
                                try
                                {
                                    if (!System.IO.File.Exists(CacheImage))
                                    {
                                        HttpWebRequest request = WebRequest.Create(ImageUrl) as HttpWebRequest;
                                        request.Method = "HEAD";
                                        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                                        response.Close();

                                        using (var client = new WebClient())
                                            client.DownloadFileAsync(new Uri(WorkshopItem.ImageUrl), CacheImage);
                                    }
                                }
                                catch
                                {
                                    ImageUrl = ErrorImage;
                                }
                            }
                        });

                        AddonItem = new AddonModel
                        {
                            Id = index,
                            AddonId = WorkshopItem.Uid,
                            Description = WorkshopItem.Description,
                            Name = WorkshopItem.Title,
                            Image = new Uri(ImageUrl),
                            IsChecked = false
                        };
                    }
                    else
                    {
                        Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => 
                            StatusBar_ProcessStep.Text = $"Loading - {ExtractItem.AddonFileName}");

                        string CacheImage = System.IO.Path.Combine(CacheImagesDir, ExtractItem.AddonUid + ".jpg");
                        string ImageUrl = (System.IO.File.Exists(CacheImage)) ? CacheImage : ErrorImage;

                        AddonItem = new AddonModel
                        {
                            Id = index,
                            AddonId = ExtractItem.AddonUid,
                            Description = ExtractItem.AddonPath,
                            Name = ExtractItem.AddonFileName,
                            Image = new Uri(ImageUrl),
                            IsChecked = false
                        };
                    }

                    int MaxDescriptionSize = 300;
                    if (AddonItem.Description.Length > MaxDescriptionSize)
                    {
                        int Start = MaxDescriptionSize;
                        int Count = AddonItem.Description.Length - MaxDescriptionSize;
                        AddonItem.Description = AddonItem.Description.Remove(Start, Count);
                    }

                    Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() =>
                    {
                        AddonsCollection.Add(AddonItem);
                        ProgressBar_Loading.Value = (ExtractItems.Count / 100) * index;
                    });

                    index++;
                }

                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() =>
                {
                    ProgressBar_Loading.Value = 0;
                    StatusBar_ProcessStep.Text = "List of addons loaded";
                    IsFirstLoading = false;
                    DataGrid_Addons.IsEnabled = true;
                });
            });
        }

        private void OnExtract(object sender, RoutedEventArgs e)
        {
            var AddonItem = ((FrameworkElement)sender).DataContext as AddonModel;
            var t = Task.Run(() =>
            {
                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => DataGrid_Addons.IsEnabled = false);
                ExtractAddon(AddonItem);
                Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() => DataGrid_Addons.IsEnabled = true);
            });
        }

        private void ExtractAddon(AddonModel AddonItem)
        {
            Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() =>
            {
                ProgressBar_Loading.IsIndeterminate = true;
                StatusBar_ProcessStep.Text = $"Extracting - {AddonItem.Name}";
            });

            GmadObject = Extractor.ExtractSingle(AddonItem.AddonId, false);
            Extractor.StartExtractGmaProcess(GmadObject);

            Storage.WindowsStorage.AddonsWindow.Dispatcher.Invoke(() =>
            {
                ProgressBar_Loading.IsIndeterminate = false;
                StatusBar_ProcessStep.Text = "Extraction completed";
            });
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var AddinItem = ((FrameworkElement)sender).DataContext as AddonModel;
            AddinItem.IsChecked = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var AddinItem = ((FrameworkElement)sender).DataContext as AddonModel;
            AddinItem.IsChecked = false;
        }

        private void CheckBox_Checked_Filter(object sender, RoutedEventArgs e)
        {
            var FilterItem = ((FrameworkElement)sender).DataContext as ComboCheckBox;

            if (FilterItem.Key == "normal")
                FiltersCollection.FirstOrDefault(x => x.Key == "global").IsChecked = false;

            if (FilterItem.Key == "global")
                FiltersCollection.FirstOrDefault(x => x.Key == "normal").IsChecked = false;

            SearchAddon();
        }

        private void CheckBox_Unchecked_Filter(object sender, RoutedEventArgs e)
        {
            var FilterItem = ((FrameworkElement)sender).DataContext as ComboCheckBox;

            bool IsGlobal = FiltersCollection.FirstOrDefault(x => x.Key == "global").IsChecked;
            bool IsNormal = FiltersCollection.FirstOrDefault(x => x.Key == "normal").IsChecked;

            if ((FilterItem.Key == "desc" && !FilterItem.IsChecked) || (!IsNormal && !IsGlobal))
                SearchAddon();
        }
    }
}
