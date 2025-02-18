using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CloudStore.UI.ViewModels;
using CloudStore.UI.Views;

namespace CloudStore.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new LoginAndRegistrationWindow
                {
                    DataContext = new LoginAndRegistrationViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}