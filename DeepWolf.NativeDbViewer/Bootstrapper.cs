using Prism.DryIoc;
using Prism.Ioc;
using System.Windows;
using DeepWolf.NativeDbViewer.Views;
namespace DeepWolf.NativeDbViewer
{
    public class Bootstrapper : PrismBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
