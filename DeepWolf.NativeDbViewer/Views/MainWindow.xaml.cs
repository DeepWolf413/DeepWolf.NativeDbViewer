using System.Windows;

namespace DeepWolf.NativeDbViewer.Views
{
    public partial class MainWindow
    {
        private enum EGame { Gta5, Rdr2, Mp3 }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SwitchNativeDb(EGame selectedGame)
        {
            GtaVView.Visibility = Visibility.Collapsed;
            Rdr2View.Visibility = Visibility.Collapsed;
            Mp3View.Visibility = Visibility.Collapsed;

            switch (selectedGame)
            {
                case EGame.Gta5:
                    GtaVView.Visibility = Visibility.Visible;
                    break;
                case EGame.Rdr2:
                    Rdr2View.Visibility = Visibility.Visible;
                    break;
                case EGame.Mp3:
                    Mp3View.Visibility = Visibility.Visible;
                    break;
                default:
                    MessageBox.Show("The selected game is unknown.");
                    break;
            }
        }

        private void OnGta5Selected(object sender, RoutedEventArgs e) => SwitchNativeDb(EGame.Gta5);

        private void OnRdr2Selected(object sender, RoutedEventArgs e) => SwitchNativeDb(EGame.Rdr2);

        private void OnMp3Selected(object sender, RoutedEventArgs e) => SwitchNativeDb(EGame.Mp3);
    }
}