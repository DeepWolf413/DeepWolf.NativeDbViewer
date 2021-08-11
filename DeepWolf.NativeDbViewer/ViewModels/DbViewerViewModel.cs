using System;
using System.Collections.Generic;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using DeepWolf.NativeDbViewer.Models;
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

        public DbViewerViewModel()
        {
            LoadNativeDbCommand = new DelegateCommand<string>(LoadNativeDb);
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

        public ICommand LoadNativeDbCommand { get; }

        public ICommand SearchCommand { get; }
        
        private async void LoadNativeDb(string dbLink)
        {
            if (string.IsNullOrEmpty(dbLink) || isNativeDbLoaded)
            { return; }

            StatusText = "Loading native db...";
            IsBusy = true;

            using (WebClient client = new WebClient())
            {
                try
                {
                    string json = await client.DownloadStringTaskAsync(dbLink);
                    var parsedJson = JObject.Parse(json);

                    await Task.Run(() =>
                    {
                        foreach (var nativeNamespace in parsedJson)
                        {
                            string namespaceName = nativeNamespace.Key;

                            foreach (var native in nativeNamespace.Value.Children())
                            {
                                Native nativeObject = native.First.ToObject<Native>();
                                nativeObject.Namespace = namespaceName;
                                nativeObject.Hash = ((JProperty)native).Name;
                                loadedNatives.Add(new NativeViewModel(nativeObject));
                            }
                        }
                    });
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Something went wrong!");
                    return;
                }
            }

            NativeList.AddRange(loadedNatives);
            isNativeDbLoaded = true;

            IsBusy = false;
        }

        /// <summary>
        /// Initiates a search in the native db, based on the given <paramref name="searchText"/>.
        /// </summary>
        /// <param name="searchText">The text to use for searching the native db.</param>
        private void StartSearch(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                ClearSearch();
                return;
            }

            StatusText = "Searching...";
            IsBusy = true;

            NativeList.Clear();

            // TODO: Implement search logic.
            string searchTextLowered = searchText.ToLower();
            foreach (var native in loadedNatives)
            {
                if (!native.Name.ToLower().Contains(searchTextLowered))
                { continue; }

                NativeList.Add(native);
            }
            
            IsBusy = false;
        }

        private void ClearSearch()
        {
            NativeList.Clear();
            NativeList.AddRange(loadedNatives);
        }
    }
}
