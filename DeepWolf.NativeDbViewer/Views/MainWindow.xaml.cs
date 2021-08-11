using HandyControl.Themes;
using HandyControl.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HandyControl.Controls;
using TabItem = System.Windows.Controls.TabItem;

namespace DeepWolf.NativeDbViewer.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Change Theme

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is RadioButton button)
            {
                if (button.Tag is ApplicationTheme tag)
                { ((App) Application.Current).UpdateTheme(tag); }
            }
        }

        #endregion
    }
}