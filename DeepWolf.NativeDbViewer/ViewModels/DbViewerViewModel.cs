using System;
using System.Collections.Generic;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DeepWolf.NativeDbViewer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DeepWolf.NativeDbViewer.ViewModels
{
    public class DbViewerViewModel : BindableBase
    {
        private const string NativeDBFilePath = "games-native-info.json";

        private NativeViewModel selectedNativeItem;

        private List<NativeViewModel> loadedNatives;
        private bool isNativeDbLoaded;

        private bool isBusy;
        private string statusText;

        public DbViewerViewModel()
        {
            LoadNativesCommand = new DelegateCommand<string>(LoadNatives);
            SearchCommand = new DelegateCommand<string>(StartSearch);

            loadedNatives = new List<NativeViewModel>();
            NativeList = new ObservableCollection<NativeViewModel>();

            isNativeDbLoaded = false;
            StatusText = string.Empty;
        }

        public NativeViewModel SelectedNativeItem
        {
            get => selectedNativeItem;
            set => SetProperty(ref selectedNativeItem, value);
        }

        /// <summary>
        /// The status text the view will show when busy.
        /// </summary>
        public string StatusText
        {
            get => statusText;
            private set => SetProperty(ref statusText, value);
        }

        /// <summary>
        /// Whether or not the viewer is busy doing something (like doing a search operation, or load native db, etc).
        /// </summary>
        public bool IsBusy
        {
            get => isBusy;
            private set => SetProperty(ref isBusy, value);
        }

        /// <summary>
        /// The list of natives that will be shown in the viewer.
        /// </summary>
        public ObservableCollection<NativeViewModel> NativeList { get; }

        public ICommand LoadNativesCommand { get; }

        public ICommand SearchCommand { get; }

        private async void LoadNatives(string gameName)
        {
            if (isNativeDbLoaded)
            {
                return;
            }

            if (string.IsNullOrEmpty(gameName))
            {
                MessageBox.Show($"Failed to load natives. The given game name is null or empty.",
                    "Something went wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StatusText = "Loading natives...";
            IsBusy = true;

            try
            {
                if (!File.Exists(NativeDBFilePath))
                {
                    MessageBox.Show($"The file path '{NativeDBFilePath}' doesn't exist.", "Invalid file path",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await Task.Run(async () =>
                {
                    string availableGamesJson = File.ReadAllText(NativeDBFilePath);
                    var parsedJson = JObject.Parse(availableGamesJson);
                    JToken gameDataToken = parsedJson[gameName];
                    if (gameDataToken == null)
                    {
                        MessageBox.Show(
                            $"The game '{gameName}' could not be found in the '{Path.GetFileNameWithoutExtension(NativeDBFilePath)}' file.",
                            "Failed to find game info", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var gameInfo = gameDataToken.ToObject<GameInfo>();

                    using (WebClient client = new WebClient())
                    {
                        string nativeDbJson = await client.DownloadStringTaskAsync(gameInfo.NativesLink).ConfigureAwait(false);
                        parsedJson = JObject.Parse(nativeDbJson);
                        ScriptUsage[] scriptUsages = new ScriptUsage[0];

                        if (!string.IsNullOrEmpty(gameInfo.ScriptUsagesMapLink))
                        {
                            string scriptUsagesMapJson = await client.DownloadStringTaskAsync(gameInfo.ScriptUsagesMapLink).ConfigureAwait(false);
                            scriptUsages = JsonConvert.DeserializeObject<ScriptUsage[]>(scriptUsagesMapJson);
                        }
                        
                        foreach (var nativeNamespace in parsedJson)
                        {
                            string namespaceName = nativeNamespace.Key;

                            foreach (var native in nativeNamespace.Value.Children())
                            {
                                Native nativeObject = native.First.ToObject<Native>();
                                nativeObject.Namespace = namespaceName;
                                nativeObject.Hash = ((JProperty) native).Name;

                                if (scriptUsages.Length > 0)
                                {
                                    ScriptUsage scriptUsage = scriptUsages.FirstOrDefault(source => source.Hash == nativeObject.Hash);

                                    if (scriptUsage != null)
                                    { nativeObject.ScriptUsage = scriptUsage.CodeExample; }
                                }

                                loadedNatives.Add(new NativeViewModel(nativeObject));
                            }
                        }
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Something went wrong!", MessageBoxButton.OK, MessageBoxImage.Error);
                isBusy = false;
                return;
            }

            await Application.Current.Dispatcher.BeginInvoke((Action) delegate { NativeList.AddRange(loadedNatives); },
                DispatcherPriority.Background);

            isNativeDbLoaded = true;

            IsBusy = false;
        }

        /// <summary>
        /// Initiates a search in the native db, based on the given <paramref name="searchText"/>.
        /// </summary>
        /// <param name="searchText">The text to use for searching the native db.</param>
        private async void StartSearch(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                await ClearSearch();
                return;
            }

            StatusText = "Searching...";
            IsBusy = true;

            await Application.Current.Dispatcher.BeginInvoke((Action) delegate
            {
                NativeList.Clear();

                string searchTextLowered = searchText.ToLower();
                foreach (var native in loadedNatives)
                {
                    string nativeNameLowered = native.Name.ToLower();
                    if (!nativeNameLowered.Contains(searchTextLowered))
                    { continue; }

                    NativeList.Add(native);
                }
            }, DispatcherPriority.Background);

            IsBusy = false;
        }

        private async Task ClearSearch()
        {
            StatusText = "Clearing search...";
            IsBusy = true;

            await Application.Current.Dispatcher.BeginInvoke((Action) delegate
            {
                NativeList.Clear();
                NativeList.AddRange(loadedNatives);
            }, DispatcherPriority.Background);

            IsBusy = false;
        }
    }
}