using Prism.Mvvm;

namespace DeepWolf.NativeDbViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Native DB Viewer";

        public MainWindowViewModel()
        {

        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}