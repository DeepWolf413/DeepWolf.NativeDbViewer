using System;
using System.Collections.Generic;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DeepWolf.NativeDbViewer.Models;
using ICSharpCode.AvalonEdit.Document;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DeepWolf.NativeDbViewer.ViewModels
{
    public class DbViewerViewModel : BindableBase
    {
        private NativeViewModel selectedNativeItem;

        private List<NativeViewModel> loadedNatives;
        private bool isNativeDbLoaded;

        private bool isBusy;
        private string statusText;
        private TextDocument textDocument;

        public DbViewerViewModel()
        {
            LoadNativesCommand = new DelegateCommand<string>(LoadNatives);
            SearchCommand = new DelegateCommand<string>(StartSearch);

            TextDocument = new TextDocument();

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

        public bool IsAnyNativeSelected => SelectedNativeItem != null;

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

        public TextDocument TextDocument
        {
            get => textDocument;
            private set => SetProperty(ref textDocument, value);
        }

        /// <summary>
        /// The list of natives that will be shown in the viewer.
        /// </summary>
        public ObservableCollection<NativeViewModel> NativeList { get; }

        public ICommand LoadNativesCommand { get; }

        public ICommand SearchCommand { get; }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            if (args.PropertyName == nameof(SelectedNativeItem))
            {
                if (SelectedNativeItem != null)
                {
                    TextDocument.Text = SelectedNativeItem.ScriptUsage;
                }
            }
        }

        private async void LoadNatives(string gameName)
        {
            if (isNativeDbLoaded)
            { return; }

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
                if (!File.Exists(GameInfo.GamesNativeInfoFilePath))
                {
                    MessageBox.Show($"The file path '{GameInfo.GamesNativeInfoFilePath}' doesn't exist.", "Invalid file path",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await Task.Run(async () =>
                {
                    var (hasFoundGameInfo, gameInfo) = TryGetGameInfo(gameName);
                    if (!hasFoundGameInfo)
                    { return; }

                    await gameInfo.UpdateCachedNativeDb();

                    var (hasFoundNativeDb, nativeDb) = await gameInfo.TryGetNativeDbFromCache();
                    if (!hasFoundNativeDb)
                    { return; }

                    var (hasFoundScriptUsages, scriptUsages) = await gameInfo.TryGetScriptUsagesFromCache();

                    foreach (var nativeDbNamespace in nativeDb)
                    {
                        string namespaceName = nativeDbNamespace.Key;
                        var natives = nativeDbNamespace.Value;

                        foreach (var native in natives)
                        {
                            string nativeHash = native.Key;
                            native.Value.Namespace = namespaceName;
                            native.Value.Hash = nativeHash;

                            if (hasFoundScriptUsages)
                            {
                                ScriptUsage scriptUsage = scriptUsages.FirstOrDefault(source => source.Hash == nativeHash);
                                if (scriptUsage != null)
                                { native.Value.ScriptUsage = scriptUsage.CodeExample; }
                            }

                            loadedNatives.Add(new NativeViewModel(native.Value));
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

        

        private (bool, GameInfo) TryGetGameInfo(string gameName)
        {
            if (!File.Exists(GameInfo.GamesNativeInfoFilePath))
            {
                MessageBox.Show($"The file path '{GameInfo.GamesNativeInfoFilePath}' doesn't exist.", "Invalid file path",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return (false, null);
            }

            string availableGamesJson = File.ReadAllText(GameInfo.GamesNativeInfoFilePath);
            var parsedJson = JObject.Parse(availableGamesJson);
            JToken gameDataToken = parsedJson[gameName];
            if (gameDataToken == null)
            {
                MessageBox.Show(
                    $"The game '{gameName}' could not be found in the '{Path.GetFileNameWithoutExtension(GameInfo.GamesNativeInfoFilePath)}' file.",
                    "Failed to find game info", MessageBoxButton.OK, MessageBoxImage.Error);
                return (false, null);
            }

            return (true, gameDataToken.ToObject<GameInfo>());
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