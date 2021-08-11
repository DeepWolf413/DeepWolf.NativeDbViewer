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

        public static readonly DependencyProperty NativeDbLinkProperty = DependencyProperty.Register("NativeDbLink", typeof(string), typeof(DbViewer), new PropertyMetadata(default(string)));

        public string NativeDbLink
        {
            get => (string) GetValue(NativeDbLinkProperty);
            set => SetValue(NativeDbLinkProperty, value);
        }
    }
}
