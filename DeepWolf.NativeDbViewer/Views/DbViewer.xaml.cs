using System.Windows;
using System.Windows.Controls;

namespace DeepWolf.NativeDbViewer.Views
{
    /// <summary>
    /// Interaction logic for DbViewer
    /// </summary>
    public partial class DbViewer : UserControl
    {
        public DbViewer()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty GameNameProperty = DependencyProperty.Register("GameName", typeof(string), typeof(DbViewer), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ViewerTitleProperty = DependencyProperty.Register("ViewerTitle", typeof(string), typeof(DbViewer), new PropertyMetadata(default(string)));

        public string GameName
        {
            get => (string) GetValue(GameNameProperty);
            set => SetValue(GameNameProperty, value);
        }

        public string ViewerTitle
        {
            get => (string)GetValue(ViewerTitleProperty);
            set => SetValue(ViewerTitleProperty, value);
        }
    }
}
